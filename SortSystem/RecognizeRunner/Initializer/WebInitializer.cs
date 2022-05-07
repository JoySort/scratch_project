using System.Net;
using CommonLib.Lib.Util;
using Grpc.Net.Client;
using Newtonsoft.Json;
using NLog;
using NLog.Web;

namespace Initializer;

public class WebInitializer
{
    
    private static Logger  logger = LogManager.GetCurrentClassLogger();
    public static string[] rpcSetup() {
        
        var rpcPort = ConfigUtil.getModuleConfig().NetworkConfig.RpcPort;
        
        var bindAddressString = ConfigUtil.getModuleConfig().NetworkConfig.RpcBindIp;
        IPAddress ipaddress = null;
        if (!(IPAddress.TryParse(bindAddressString, out ipaddress)))
        {
            if (bindAddressString == null || bindAddressString == "*")
            {
                ipaddress=IPAddress.Any;
            }
        }
        
        string rpcListenKey = "--urls";
        string rpcUrl = "http://"+ipaddress.ToString() +":"+ rpcPort;
        string[] joyArgs = new[] {rpcListenKey, rpcUrl};
        //logger.Info("Listening on {}",rpcUrl);
        return joyArgs;
    }

    private static (int webapiPort,int tcpPort,IPAddress webAPIAddress, IPAddress tcpIpAddress ) getWebConfig()
    {
        var webAPIPort = ConfigUtil.getModuleConfig().NetworkConfig.RpcPort;
        var webAPIBindAddressString = ConfigUtil.getModuleConfig().NetworkConfig.RpcBindIp;

        var tcpPort = ConfigUtil.getModuleConfig().NetworkConfig.TcpPort;
        var tcpBindAddressString = ConfigUtil.getModuleConfig().NetworkConfig.TcpBindIp;
        
        logger.Info($"Using Networking Configuration {JsonConvert.SerializeObject(ConfigUtil.getModuleConfig().NetworkConfig)}");
        
        IPAddress webAPIIPaddress = null;
        if (!(IPAddress.TryParse(webAPIBindAddressString, out webAPIIPaddress)))
        {
            if (webAPIBindAddressString == null || webAPIBindAddressString == "*")
            {
                webAPIIPaddress=IPAddress.Any;
            }
        }
        
        IPAddress tcpIPaddress = null;
        if (!(IPAddress.TryParse(tcpBindAddressString, out tcpIPaddress)))
        {
            if (tcpBindAddressString == null || tcpBindAddressString == "*")
            {
                tcpIPaddress=IPAddress.Any;
            }
        }

        return (webAPIPort, tcpPort, webAPIIPaddress, tcpIPaddress);

    }

    public static void init()
    {
        var rpcPort = rpcSetup();
        var configInfo = getWebConfig();
       
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            if (ConfigUtil.getModuleConfig().Standalone)
            {
                serverOptions.Listen(configInfo.tcpIpAddress, configInfo.tcpPort,
                    cfg => { cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2; });
            }

            serverOptions.Listen(configInfo.webAPIAddress, configInfo.webapiPort, cfg =>
            {
                cfg.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
            }); 
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        builder.Host.UseNLog();
        
        if(ConfigUtil.getModuleConfig().Standalone){
            builder.Services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 200 * 1024 * 1024; // 200 MB
                options.MaxSendMessageSize = 5 * 1024 * 1024; // 5 MB
        
            });
            builder.Services.AddMagicOnion();
        }
        var app = builder.Build();

   
        app.UseRouter(builder =>
        {
            builder.MapGet("/", context =>
            {
                context.Response.Redirect("./swagger/index.html", permanent: false);
                return Task.FromResult(0);
            });
        });
        if (ConfigUtil.getModuleConfig().Standalone)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionHttpGateway("_",
                    ((IApplicationBuilder) app).ApplicationServices
                    .GetService<MagicOnion.Server.MagicOnionServiceDefinition>().MethodHandlers,
                    GrpcChannel.ForAddress($"http://{configInfo.tcpIpAddress.ToString()}:{configInfo.tcpPort}"));
                endpoints.MapMagicOnionSwagger("swagger",
                    ((IApplicationBuilder) app).ApplicationServices
                    .GetService<MagicOnion.Server.MagicOnionServiceDefinition>().MethodHandlers, "/_/");

                endpoints.MapMagicOnionService();


            });
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        

        //app.UseHttpsRedirection();

        //app.UseAuthorization();

        app.MapControllers();
        
        app.Run();
        

    }
}