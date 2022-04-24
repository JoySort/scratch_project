using System.Collections.Concurrent;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Controllers;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using CommonLib.Lib.Worker.Recognizer;
using NLog;

namespace CommonLib.Lib.Worker.Upper;

public class CameraToUpperHTTPClientWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool isProjectRunning = false;
    private Project currentProject;
  
    private static CameraToUpperHTTPClientWorker me = new CameraToUpperHTTPClientWorker();
    public static CameraToUpperHTTPClientWorker getInstance()
    {
        return me;
    }
    private void init()
    {
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
        
    }

    public void onRecResultGenerated(object? sender, List<RecResult> results)
    {
        var joyHttpClient = new JoyHTTPClient.JoyHTTPClient();
        var remoteEndPoints = ModuleCommunicationWorker.getInstance().RpcEndPoints;
        foreach ((var module, var rdps) in remoteEndPoints)
        {
            if (module == JoyModule.Upper)
                foreach ((var Key, var item) in rdps)
                {
                    var remoteURI = remoteCallProtocal + item.Address + ":" + item.Port + consolidateURL;
                    joyHttpClient.PostToRemote<Object>(
                        remoteURI,
                        results);
                    logger.Debug($"Sending RecResult count {results.Count} of lastTriggerID {results.Last().Coordinate.Key()} to {remoteURI}");
                }
        }
    }

    private CameraToUpperHTTPClientWorker()
    {
        init();

    }
    private void OnProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        var remoteEndPoints = ModuleCommunicationWorker.getInstance().RpcEndPoints;
        
        foreach ((JoyModule module,ConcurrentDictionary<string,RpcEndPoint> rdps )in remoteEndPoints)
        {
            if (module == ConfigUtil.getModuleConfig().Module)
            {
                logger.Debug("ignore same type module as this module {}",Enum.GetName(module));
                continue;
            }

          
        }
       
    }

    private string remoteCallProtocal = "http://";
    private string consolidateURL = "/sort/consolidate_batch";

}