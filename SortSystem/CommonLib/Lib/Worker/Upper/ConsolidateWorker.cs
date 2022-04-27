using System.Collections.Concurrent;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.Exception;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Worker.Upper;

public class ConsolidateWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static ConsolidateWorker worker = new ConsolidateWorker();

    public static ConsolidateWorker getInstance()
    {
        return worker;
    }

    private ConcurrentQueue<List<RecResult>> recResultQueue = new ConcurrentQueue<List<RecResult>> ();
    private Dictionary<string,List<RecResult>> cacheRecResultDictionary = new Dictionary<string,List<RecResult>>();
    private Dictionary<string,List<Feature>> incompleteFeatureList = new Dictionary<string,List<Feature>>();
    private Dictionary<string,Coordinate> incompleteCoordinate = new Dictionary<string,Coordinate>();
    private bool isProjectRunning;
    private int sortingInterval;
    private Project currentProject;
    private int expectedFeatureCount;
    private Criteria[] enabledCriterias;
    private ConsolidatePolicy consolidationPolicy;
    private Dictionary<string,CriteriaMapping> criteriaMapping;

    private ConsolidateWorker()
    {

        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public void OnProjectStatusChange(object sender, ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            this.isProjectRunning = true;
            totalProcessed = 0;
            this.expectedFeatureCount = currentProject.Criterias.Length;
            this.enabledCriterias = currentProject.Criterias;
            
            consolidationPolicy = ConfigUtil.getModuleConfig().ConsolidatePolicy;
            criteriaMapping = ConfigUtil.getModuleConfig().CriteriaMapping;
            this.sortingInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
            
            cacheRecResultDictionary = new Dictionary<string,List<RecResult>>();
            incompleteFeatureList = new Dictionary<string,List<Feature>>();
            incompleteCoordinate = new Dictionary<string,Coordinate>();
            processQueue();
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse ||
            statusEventArgs.State == ProjectState.washing)
        {
            isProjectRunning = false;
        }
    }


    private long totalProcessed = 0;
    public void processBulk(List<RecResult> results)
    {
        
        recResultQueue.Enqueue(results);
        totalProcessed += results.Count;
        logger.Debug("recResult Queue count : {}, with total processed count {}",recResultQueue.Count,totalProcessed);
        onRecReceiving?.Invoke(this, results);
            // foreach (var value in results)
            // {
            //     processSingle(value);
            // }
            //
            // processResult();
    }

    
    private  void processQueue( )
    {
        Task.Run(() =>
        {
            while (isProjectRunning)
            {
                Thread.Sleep(10);
                if (! (recResultQueue.Count > 0))
                {
                    continue;
                }

                while (recResultQueue.Count > 0)
                {
                    List<RecResult> tmpRecResult = null;
                       var deQueueResult =  recResultQueue.TryDequeue(out tmpRecResult);
                       foreach(var item in tmpRecResult){
                        if(deQueueResult)processSingle(item);
                        //Thread.Sleep(1);
                       }
                }

                try
                {
                    processResult();
                }
                catch (Exception e)
                {
                    logger.Error("Exception when consolidating {}",e.Message);
                }
            }
        });
       

       
    }
    
    private void processSingle(RecResult recResult)
    {
      
        if (!isProjectRunning) throw new ProjectDependencyException("ConsolidateWorker");
        if (!cacheRecResultDictionary.ContainsKey(recResult.Coordinate.ConsolidationKey()))
        { 
            
                 cacheRecResultDictionary.Add(recResult.Coordinate.ConsolidationKey(),new List<RecResult>());
            
        }

      
        
        cacheRecResultDictionary[recResult.Coordinate.ConsolidationKey()].Add(recResult);
        

        if (!incompleteFeatureList.ContainsKey(recResult.Coordinate.ConsolidationKey()))
        {
           
                 incompleteFeatureList.Add(recResult.Coordinate.ConsolidationKey(),new List<Feature>());
           
        }

        if (!incompleteCoordinate.ContainsKey(recResult.Coordinate.ConsolidationKey()))
        {
         
                 incompleteCoordinate.Add(recResult.Coordinate.ConsolidationKey(),recResult.Coordinate);
           
        }
        
      
    }

    private int[] cacheStatus = new int[3];
    private long runningCounter = 0;
   
    private void processResult()
    {
 
            //logger.Info("Consolidation worker starts");
  
                
                runningCounter++;
                foreach ( (var key,  var value) in cacheRecResultDictionary)
                {
                    //
                    for(var i=0;i<consolidationPolicy.OffSetRowCount.Length;i++)
                    {
                        var found = false;
                        foreach (var criteria in enabledCriterias)
                        {
                            if (criteria.Code != null && criteria.Code == consolidationPolicy.CriteriaCode[i])
                            {
                                found = true;
                            }
                        }

                        if (!found) continue;

                        for (var j = value.Count - 1; j >= 0; j--)
                        {
                            if (value[j].Coordinate.RowOffset == consolidationPolicy.OffSetRowCount[i] - 1)
                            {
                                var criteriaIndex = criteriaMapping[consolidationPolicy.CriteriaCode[i]].Index;
                                float consolidatedFeatureValue = consolidateFeature(cacheRecResultDictionary[key],
                                    key,criteriaIndex,i);
                                incompleteFeatureList[key].Add(new Feature(criteriaIndex,consolidatedFeatureValue));
                                break;
                            }
                        }

                      
                    }

                }
                
                var completeResult = new List<ConsolidatedResult>();
                foreach( (var key, var features) in incompleteFeatureList){
                    if (features.Count == expectedFeatureCount)
                    {
                        completeResult.Add(new ConsolidatedResult(incompleteCoordinate[key],expectedFeatureCount,cacheRecResultDictionary[key].First().CreatedTimestamp,features));
                       //logger.Debug("consolidated results{}",JsonConvert.SerializeObject(completeResult.Last()));
                    }
                }
                logger.Debug("Consolidate Worker with count:{}",completeResult.Count);
               
                foreach (var value in completeResult)
                {
                    cacheRecResultDictionary.Remove(value.Coordinate.ConsolidationKey());
                    incompleteFeatureList.Remove(value.Coordinate.ConsolidationKey());
                    incompleteCoordinate.Remove(value.Coordinate.ConsolidationKey());
                }

                cacheStatus[0] = cacheRecResultDictionary.Count;
                cacheStatus[1] = incompleteFeatureList.Count;
                cacheStatus[2] = incompleteCoordinate.Count;

                if (runningCounter * sortingInterval % 300 == 0)
                {
                    logger.Info("Consolidate worker cache stats main cache {} processing cache {} ",cacheStatus[0],cacheStatus[1]);
                }
                
                
                if (cacheRecResultDictionary.Count > 0 || incompleteFeatureList.Count > 0 ||
                    incompleteCoordinate.Count>0)
                {
                    if(completeResult.Count>0){
                        var lastTrigger = completeResult.Last().Coordinate.TriggerId;
                         if(incompleteCoordinate.First().Value.TriggerId<lastTrigger){
                        logger.Error("Consolidate worker cache is none zero {} processing cache {} {}",cacheStatus[0],cacheStatus[1],cacheStatus[2]);
                        //throw new System.Exception("");
                    }
                    }
                }
                
                
                if (completeResult.Count > 0) DispatchResultEvent(new ResultEventArg(completeResult));

            
            //logger.Info("Consolidation worker stops");
      

    }

    private float consolidateFeature(List<RecResult> toBeConsolidatePartialRecResultList,string coordinateKey,int criteriaIndex,int consolidatePolicyIndex)
    {

        var featureValuesToBeConsolidated = new List<float>();
        toBeConsolidatePartialRecResultList = toBeConsolidatePartialRecResultList.OrderBy(rrs => rrs.Coordinate.RowOffset).ToList();//order to make sure first and last operations are correct
        
        foreach (var recResult in toBeConsolidatePartialRecResultList)
        {
            foreach (var feature in recResult.Features)
                {
                    if (feature.CriteriaIndex == criteriaIndex)
                    {
                        featureValuesToBeConsolidated.Add(feature.Value);
    
                    }
                }
        }

        var result = consolidateFeatureValue(
            featureValuesToBeConsolidated,
            consolidationPolicy.ConsolidationOperation[consolidatePolicyIndex],
            consolidationPolicy.ConsolidateArg[consolidatePolicyIndex]
            );

        return result;
    }
    
    
    private float consolidateFeatureValue(List<float> listValues,ConsolidateOperation consolidationOperation,int consolidatePolicyArg)
    {
        List<float> tmpList;
    
        switch (consolidationOperation){
            case ConsolidateOperation.avg:
                return listValues.Average();
            case ConsolidateOperation.min:
                return listValues.Min(); 
            case ConsolidateOperation.max: 
                return listValues.Max(); 
            case ConsolidateOperation.merge: 
                return listValues.First();// like apple weight sensor, feature weight only from sensor, so consolidation will only have one feature element and just choose first 1 to consolidate
            case ConsolidateOperation.firstNAvg:
                 tmpList = new List<float>();
                for (var i = 0; i < consolidatePolicyArg; i++)
                {
                    tmpList.Add(listValues[i]);
                }

                return tmpList.Average();
            case ConsolidateOperation.lastNAvg:
                 tmpList = new List<float>();
                for (var i = listValues.Count-1 ; i >= ((listValues.Count - 1) - consolidatePolicyArg ); i--)
                {
                    tmpList.Add(listValues[i]);
                }
                return tmpList.Average();
            case ConsolidateOperation.maxNAvg:
                tmpList = new List<float>();
                listValues = listValues.OrderByDescending(v=>v).ToList();
                for (var i = 0; i < consolidatePolicyArg; i++)
                {
                    tmpList.Add(listValues[i]);
                }
                return tmpList.Average();
            case ConsolidateOperation.minNAvg:
                tmpList = new List<float>();
                listValues = listValues.OrderBy(v=>v).ToList();
                for (var i = 0; i < consolidatePolicyArg; i++)
                {
                    tmpList.Add(listValues[i]);
                }
                return tmpList.Average();
        }

        return 0;
    }
    
    public event EventHandler<ResultEventArg> OnResult;
    protected virtual void DispatchResultEvent(ResultEventArg e)
    {
        var handler = OnResult;
        handler?.Invoke(this, e);
    }

    public event EventHandler<List<RecResult>> onRecReceiving;
}

public class ResultEventArg
{
    public List<ConsolidatedResult> Results;

    public ResultEventArg(List<ConsolidatedResult> results)
    {
        Results = results;
    }
}
