using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Worker.HTTP;

public class CameraHttpClientWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool isProjectRunning = false;
    private Project currentProject;
  
    private static CameraHttpClientWorker me = new CameraHttpClientWorker();
    public static CameraHttpClientWorker getInstance()
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
        var toBeSent = filterResultAgainstLowerMachineRange(results);
        
        foreach((var key,var value) in toBeSent){
            
            var remoteURI = remoteCallProtocal + key.Address + ":" + key.WebPort + consolidateURL;
                joyHttpClient.PostToRemote<Object>(
                    remoteURI,
                    value);
                logger.Debug($"Sending RecResult count {results.Count} of lastTriggerID {results.Last().Coordinate.Key()} to {remoteURI}");
        }
    }

    /**
     * <summary>
     * 有可能发生以下情况：
     * 1， 封袋或者异物检测的情况下，1个大相机，覆盖多个上位机带下位机，但是每个上位机只负责一部分column，就会存在相机的结果里面，有的是跟上位机不相关的。这种情况下，就需要把多余的记录分配到不同的上位机上去。
     * 2， 椰枣机的情况下，多个上位机，多个相机，每个相机覆盖范围在上位机覆盖范围内，这种时候，也需要根据上位机范围，把记录中跟相应上位机相关的记录提取出来，只发给相应上位机，让上位机不至于收到用不到的信息。
     * 3， 以后可能出现某种倾下，比如两排传送带，分别用2个下位机控制，但是相机升级了，升级成3个，也就是说，1个半相机覆盖一个边的下位机，这种情况下，这个中间的覆盖2部分下位机的相机的照片，需要分开发给不同的上位机。
     * 下面的逻辑，就是看某条记录的column值，是否在对应的上位机所覆盖的范围内，只发送上位机覆盖范围内的给相应的上位机。
     * </summary>
     */
    private Dictionary<RpcEndPoint,List<RecResult>> filterResultAgainstLowerMachineRange(List<RecResult> results)
    {
        var toBeSent = new Dictionary<RpcEndPoint, List<RecResult>>();
        var totalAllocatedCount = 0;
        
        var lostRange = new Dictionary<int[][], int[]>();
        var lostDataRange = new int[2]{1000,0};//lower end initialize with 100 to find lowest ; higher end initialize with 0 to find hightest.
            
            foreach (var result in results)
            {
                var found = false;
                int[][] lowerRange = null;
                
                
                foreach(var rpcEndPoint in upperRpcEndPoints)
                {
                    if(!toBeSent.ContainsKey(rpcEndPoint)) toBeSent.Add(rpcEndPoint,new List<RecResult>());
                    
                    var remoteLowerConfigs = rpcEndPoint.ModuleConfig.LowerConfig;
                    lowerRange = remoteLowerConfigs.Select(value=>value.Columns).ToArray();
                    foreach (var remoteLowerConfig in remoteLowerConfigs)
                    {
                        
                        if (result.Coordinate.Column <= remoteLowerConfig.Columns[1] &&
                            result.Coordinate.Column >= remoteLowerConfig.Columns[0])
                        {
                            toBeSent[rpcEndPoint].Add(result);
                            totalAllocatedCount++;
                            found = true;
                        }
                       


                    }

                }

                if (!found)
                {
                    lostDataRange[0] = result.Coordinate.Column<= lostDataRange[0]? result.Coordinate.Column:lostDataRange[0];
                    lostDataRange[1] = result.Coordinate.Column>= lostDataRange[1]? result.Coordinate.Column:lostDataRange[1];
                   
                }
            }
            

        if (totalAllocatedCount != results.Count && (lostDataRange[0]!=1000)&&lostDataRange[1]!=0)
        {
            logger.Error("Camera data will be lost!!! ResultSet lost  range {} with Lower Coverage ranges {} ",JsonConvert.SerializeObject(lostDataRange),JsonConvert.SerializeObject(upperRpcEndPoints.Select(value=>value.ModuleConfig.LowerConfig.Select(value=>value.Columns)))) ;
        }

        return toBeSent;

    }

    private CameraHttpClientWorker()
    {
        init();

    }

    private List<RpcEndPoint> upperRpcEndPoints;
    private void OnProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        upperRpcEndPoints = new List<RpcEndPoint>();
        var remoteEndPoints = ModuleCommunicationWorker.getInstance().RpcEndPoints;
        foreach ((var module, var rdps) in remoteEndPoints)
        {
            
            var localModuleConfig = ConfigUtil.getModuleConfig().CameraConfigs;
            if (module == JoyModule.Upper )
                
               
            
                foreach ((var Key, var item) in rdps)
                {
                    var found = false;
                    var remoteModuleconfig = item.ModuleConfig.LowerConfig;
                    
                    foreach (var remoteLowerconfig in remoteModuleconfig)
                    {
                        foreach (var cameraConfig in localModuleConfig)
                        {
                            // check range overlap, this would include 1, cam totally in lower range. 2, cam partical in lower range
                            if (cameraConfig.Columns[0] <= remoteLowerconfig.Columns[1] &&
                                cameraConfig.Columns[1] >= remoteLowerconfig.Columns[0])
                            {
                                found = true;
                               
                            }
                        }
                    }
                    if(found && !upperRpcEndPoints.Contains(item)) upperRpcEndPoints.Add(item);
                }
                
               
        }
        logger.Info("CameraToUpperHttpClientWorker initialized on project change event with eligible Upper {}",JsonConvert.SerializeObject(upperRpcEndPoints.Select(value=> $"{value.Address}:{value.WebPort} with lower column range {JsonConvert.SerializeObject(value.ModuleConfig.LowerConfig.Select(value=>value.Columns))}")));

    }

    private string remoteCallProtocal = "http://";
    private string consolidateURL = "/sort/consolidate_batch";

}