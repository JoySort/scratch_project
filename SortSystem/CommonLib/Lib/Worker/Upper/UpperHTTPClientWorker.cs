using System.Collections.Concurrent;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.JoyHTTPClient;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.Worker.Upper;

public class UpperToCameraHTTPClientWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool isProjectRunning = false;
    private Project currentProject;

    private static UpperToCameraHTTPClientWorker me = new UpperToCameraHTTPClientWorker();
    public static UpperToCameraHTTPClientWorker getInstance()
    {
        return me;
    }
    private void init()
    {
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
        
    }
    private UpperToCameraHTTPClientWorker()
    {
        init();

    }
    private void OnProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        var remoteEndPoints = ModuleCommunicationWorker.getInstance().RpcEndPoints;
        if (!ConfigUtil.getModuleConfig().LowerConfig.Select(value => value.IsMaster).Contains(true))
        {
            logger.Info("This is a Slave node. Salve upper will not send project start cmd to other module");
            return;
        }

        foreach ((JoyModule module,ConcurrentDictionary<string,RpcEndPoint> rdps )in remoteEndPoints)
        {
            if (module == ConfigUtil.getModuleConfig().Module )
            {
                logger.Debug("ignore same type module as this module {}",Enum.GetName(module));
                continue;
            }

            foreach ((var Key,var item) in rdps)
            {
                var joyHttpClient = new JoyHTTPClient.JoyHTTPClient();
                
                switch (e.State)
                {
                    case ProjectState.start:
                        joyHttpClient.PostToRemote<Object>(remoteCallProtocal+item.Address+":"+item.Port+startProjectEndpointURI,e.currentProject);
                        break;
                    case ProjectState.stop :
                        joyHttpClient.GetFromRemote<WebControllerResult>(remoteCallProtocal + item.Address + ":" + item.Port +
                                                                         stopProjectEndpointURI);
                        break;
                }
                
            }
        }
       
    }

    private string remoteCallProtocal = "http://";
    private string startProjectEndpointURI = "/project/start";
    private string stopProjectEndpointURI = "/project/stop";

}