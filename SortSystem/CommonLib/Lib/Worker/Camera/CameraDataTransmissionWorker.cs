using System.Collections.Concurrent;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.GrpcServiceInterface;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Grpc.Core;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Worker.Camera;

public class CameraTransmissionWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool isProjectRunning = false;
    private Project currentProject;
  
    private static CameraTransmissionWorker me = new CameraTransmissionWorker();
    public static CameraTransmissionWorker getInstance()
    {
        return me;
    }
    private void init()
    {
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
        
    }

    public void onCameraPayLoad(object? sender, CameraPayLoad cameraPayLoad)
    {
        var tmpResult = new List<CameraPayLoad>();
        tmpResult.Add(cameraPayLoad);
        processCameraData(tmpResult);
    }

    public void processCameraData(List<CameraPayLoad> results)
    {
        var joyHttpClient = new JoyHTTPClient.JoyHTTPClient();
        var toBeSent = filterCameraPayloadAgainstRecognizerCoverage(results);
        
        foreach((var key,var value) in toBeSent){
            //var remoteURI = remoteCallProtocal + key.Address + ":" + key.WebPort + recognizerProcessCameraPayloadURL;
            //joyHttpClient.Upload(remoteURI,value);
            var client = new GrpcTransmissionClient(key.Address, key.TcpPort);
            foreach (var cameraPayLoad in value)
            {
                Task.Run(() =>
                {
                    var stats = new StatsHolder();
                    stats.StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    stats.TriggerId = cameraPayLoad.TriggerId;
                    var triggerId = cameraPayLoad.TriggerId;
                    var result =  client.sendCameraData(cameraPayLoad);
                    stats.FinishTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    onCameraDataSent?.Invoke(this,stats);
                    if (triggerId != result.Result)
                    {
                        throw new Exception($"GRPC service Sent triggerID {triggerId} does not match returned triggerid {result.Result}");
                    }
                });
                
            }
            
            if(logger.IsDebugEnabled)logger.Debug($"Sending CameraPayLoad count {results.Count} of lastTriggerID {results.Last().TriggerId} with Column coverage {string.Join(",",results.Last().CamConfig.Columns)} ");
        }
    }

  
    private Dictionary<RpcEndPoint,List<CameraPayLoad>> filterCameraPayloadAgainstRecognizerCoverage(List<CameraPayLoad> results)
    {
        var toBeSent = new Dictionary<RpcEndPoint, List<CameraPayLoad>>();
        if (!(recognizerRpcEndPoints.Count > 0))
        {
            logger.Error("Can not send images as remote endpoint is not ready! Start Recognizer first and make sure Recoginzer column coverage can cover camera column coverage");
            return null;
        }

        var totalAllocatedCount = 0;
        var lostCameraLoad = new List<CameraPayLoad>();
            foreach (var result in results)
            {
                var found = false;
                int[] columnCoverage = null;
                
                
                foreach(var rpcEndPoint in recognizerRpcEndPoints)
                {
                    if(!toBeSent.ContainsKey(rpcEndPoint)) toBeSent.Add(rpcEndPoint,new List<CameraPayLoad>());
                    
                    var remoteColumnCoverage = rpcEndPoint.ModuleConfig.RecognizerConfig.Columns;

                   
                        
                        if (result.CamConfig.Columns[1] <= remoteColumnCoverage[1] &&
                            result.CamConfig.Columns[0] >= remoteColumnCoverage[0])
                        {
                            toBeSent[rpcEndPoint].Add(result);
                            totalAllocatedCount++;
                            found = true;
                        }

                }

                if (!found)
                {
                    lostCameraLoad.Add(result);
                }
            }
            

        if (totalAllocatedCount != results.Count && lostCameraLoad.Count>0)
        {
            logger.Error("Camera data will be lost!!! CameraPayLoad lost  range {} with Recognizer Coverage ranges {} ",JsonConvert.SerializeObject(lostCameraLoad.Select(value=>value.CamConfig.Columns)).Distinct(),JsonConvert.SerializeObject(recognizerRpcEndPoints.Select(value=>value.ModuleConfig.RecognizerConfig.Columns))) ;
        }

        return toBeSent;

    }

    private CameraTransmissionWorker()
    {
        init();

    }

    private List<RpcEndPoint> recognizerRpcEndPoints;
    private void OnProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        if (e.State == ProjectState.start)
        {
            recognizerRpcEndPoints = new List<RpcEndPoint>();
            var remoteEndPoints = ModuleCommunicationWorker.getInstance().RpcEndPoints;
            foreach ((var module, var rdps) in remoteEndPoints)
            {
                
                var localModuleConfig = ConfigUtil.getModuleConfig().CameraConfigs;
                if (module == JoyModule.Recognizer )
                    
                   
                
                    foreach ((var Key, var item) in rdps)
                    {
                        var found = false;
                        var remoteModuleconfig = item.ModuleConfig.RecognizerConfig;
                        
                      
                            foreach (var cameraConfig in localModuleConfig)
                            {
                                // check range overlap, this would include 1, cam totally in lower range. 2, cam partical in lower range
                                if (cameraConfig.Columns[0] <= remoteModuleconfig.Columns[1] &&
                                    cameraConfig.Columns[1] >= remoteModuleconfig.Columns[0])
                                {
                                    found = true;
                                   
                                }
                            }
                        
                        if(found && !recognizerRpcEndPoints.Contains(item)) recognizerRpcEndPoints.Add(item);
                    }
                    
                   
            }
            logger.Info("CameraHttpClientWorker initialized on project change event with eligible Recognizer {}",
                JsonConvert.SerializeObject(recognizerRpcEndPoints.Select(value=> $"{value.Address}:{value.WebPort} with lower column range {JsonConvert.SerializeObject(value.ModuleConfig.RecognizerConfig.Columns)}")));
        }
    }

    public  event EventHandler<StatsHolder>? onCameraDataSent;
    private string remoteCallProtocal = "http://";
    private string recognizerProcessCameraPayloadURL = "/recognize/process_camera_data";

}

