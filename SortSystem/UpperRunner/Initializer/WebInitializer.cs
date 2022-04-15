using CommonLib.Lib.Util;
using NLog;
using NLog.Web;

namespace LowerRunner;

public class WebInitializer
{
    
    private static Logger  logger = LogManager.GetCurrentClassLogger();
    public static string[] rpcSetup() {
        
        var rpcPort = ConfigUtil.getModuleConfig().NetworkConfig.RpcPort;
        string rpcListenKey = "--urls";
        string rpcUrl = "http://0.0.0.0:" + rpcPort;
        string[] joyArgs = new[] {rpcListenKey, rpcUrl};
        //logger.Info("Listening on {}",rpcUrl);
        return joyArgs;
    }
    public static void init()
    {
        
        var builder = WebApplication.CreateBuilder(rpcSetup());

// Add services to the container.

        builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        builder.Host.UseNLog();
        //builder.("http://*:"+ConfigUtil.getModuleConfig().Network.RpcPort);
        
        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}