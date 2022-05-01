using System.Collections.Concurrent;
using CameraLib.Lib.ConfigVO;
using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Sort.ResultVO;
using CameraLib.Lib.Util;
using CameraLib.Lib.vo;
using CameraLib.Lib.Worker.Upper;
using Elasticsearch.Net;
using Nest;
using NLog;
using Filter = CameraLib.Lib.vo.Filter;

namespace CameraLib.Lib.Worker.Analytics;

public class ElasticSearchWorker
{
    
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static ElasticSearchWorker worker = new ElasticSearchWorker();
    private bool isProjectRunning;
    private Project currentProject;
    private ElasticClient client;
    private ConcurrentQueue<List<RecResult>> recResulttoBeProcessed = new ConcurrentQueue<List<RecResult>>();
    private ConcurrentQueue<List<LBResult>> lbResulttoBeProcessed = new ConcurrentQueue<List<LBResult>>();
    

    public static ElasticSearchWorker getInstance()
    {
        return worker;
    }
    
    private ElasticSearchWorker()
    {
        init();
    }

    public void init()
    {
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
        var criteriaMappingByCode = ConfigUtil.getModuleConfig().CriteriaMapping;
        
        foreach((var key , var value ) in criteriaMappingByCode)
        {
            criteriaMappingByIndex.Add(value.Index,value);
        }


        string esUrl=ConfigUtil.getModuleConfig().ElasticSearchConfig.url;
        var settings = new ConnectionSettings(new Uri(esUrl))
                .DefaultMappingFor<ESFeature>(m => m.IndexName("esfeature"))
                .DefaultMappingFor<Project>(m => m.IndexName("projects"))
                .DefaultMappingFor<ModuleConfig>(m => m.IndexName("module"))
            ;
           
    
         client = new ElasticClient(settings);
    }

    private Dictionary<int,CriteriaMapping> criteriaMappingByIndex = new Dictionary<int,CriteriaMapping>();
    private void sendProject()
    {
        //client.ind
        var indexResponse = client.IndexDocument(currentProject); 
    }

    private void sendMachineInfo()
    {
        client.IndexDocument(ConfigUtil.getModuleConfig());
    }
    

    public void OnProjectStatusChange(object sender, ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            this.isProjectRunning = true;
            projectStartTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            recResulttoBeProcessed = new ConcurrentQueue<List<RecResult>>();
            lbResulttoBeProcessed = new ConcurrentQueue<List<LBResult>>();
            
            machineID = LowerMachineDriver.getInstance().machineID;
            runtimeUuid = ConfigUtil.getModuleConfig().Uuid;
            
            sendMachineInfo();
            sendProject();
            sendRecResult();
            sendLBResult();
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse ||
            statusEventArgs.State == ProjectState.washing)
        {
            isProjectRunning = false;
        }
        
  
    }

    private string machineID;
    private string runtimeUuid;


    
    public void processBulkRecResult(List<RecResult> results)
    {
        Task.Run(() =>
        {
            recResulttoBeProcessed.Enqueue(results);
       });
        
    }
    public void processBulkLBResult(List<LBResult> results)
    {
        Task.Run(() =>
        {
            lbResulttoBeProcessed.Enqueue(results);
        });
        
    }

    private long projectStartTimestamp;
    private void sendRecResult()
    {
        Task.Run(() =>
        {
            var sendCount = 0;
            while(isProjectRunning){
                Thread.Sleep(5);
                if (recResulttoBeProcessed.Count > 0)
                {
                    List<RecResult> results;
                    List<ESFeature> toBeSent = new List<ESFeature>();
                    logger.Debug("processing ESRecResult Queue count {}, project running time {}",recResulttoBeProcessed.Count,DateTimeOffset.Now.ToUnixTimeMilliseconds()-projectStartTimestamp);
                    
                    while(recResulttoBeProcessed.Count>0){
                        if (recResulttoBeProcessed.TryDequeue(out results))
                        {
                            
                           
                            foreach (var item in results)
                            {
                                foreach(var feature in item.Features)
                                {
                                    ESFeature esFeature = new ESFeature(feature,"N/A",null,null,criteriaMappingByIndex[feature.CriteriaIndex].Key,machineID,currentProject.Id,currentProject.TimeStamp,item.Coordinate,false,item.CreatedTimestamp);
                                    toBeSent.Add(esFeature);
                                }
                            }
                            sendCount += toBeSent.Count;
                        }
                        
                    }
                    try
                    {
                        var indexResponse = client.IndexMany(toBeSent);
                                
                        logger.Debug("RecResult sent count {}",sendCount);
                    }
                    catch(Exception e)
                    {
                        logger.Error("Error while ssending recResult {}",e.Message);
                    }
                }
            }
        });
    }
    
    
    public string getFilterString (vo.Filter[][] filters)
    {
        string result="";
        foreach (var OrFilter in filters)
        {
            result += "{";
            foreach (var innerFilter in OrFilter)
            {
                result+= "[" + innerFilter.Criteria.Code + ":" + string.Join<float>(",", innerFilter.FilterBoundaries[0]) + "]";
            }
            result += "}";
        }

        return result;
    }
    private void sendLBResult()
    {
        Task.Run(() =>
        {
            var sentCount = 0;
            while(isProjectRunning){
                Thread.Sleep(1);
                if (lbResulttoBeProcessed.Count > 0)
                {
                    List<LBResult> results;
                    List<ESFeature> eslbResults = new List<ESFeature>();
                    logger.Debug("processing ESLBResult Queue count {}, with project start time {}",lbResulttoBeProcessed.Count,DateTimeOffset.Now.ToUnixTimeMilliseconds()-projectStartTimestamp);
                    while(lbResulttoBeProcessed.Count>0){
                        if (lbResulttoBeProcessed.TryDequeue(out results))
                        {

                            if (results.Count <= 0) continue;
                            foreach (var item in results)
                            {
                                foreach(var feature in item.Features)
                                {
                                    ESFeature esFeature = new ESFeature(feature,getFilterString(item.LoadBalancedOutlet.First().Filters),item.Outlets,item.LoadBalancedOutlet ,criteriaMappingByIndex[feature.CriteriaIndex].Key,machineID,currentProject.Id,currentProject.TimeStamp,item.Coordinate,true,item.CreatedTimestamp);
                                    eslbResults.Add(esFeature);
                                }
                            }
                            sentCount += eslbResults.Count;
                           
                            
                        }
                        
                    }
                    try{
                        var indexResponse = client.IndexMany(eslbResults);
                             
                        logger.Debug("LBResult sent count {}",sentCount);
                    }
                    catch(Exception e)
                    {
                        logger.Error("Error while sending recResult to ElasticSearch {}",e.Message);
                    }
                }
            }
        });
    }
}
