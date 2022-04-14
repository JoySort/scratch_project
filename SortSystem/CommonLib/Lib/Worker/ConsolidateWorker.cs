using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.Exception;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using Newtonsoft.Json;
using NLog;

namespace CommonLib.Lib.Sort;

public class ConsolidateWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private static ConsolidateWorker worker = new ConsolidateWorker();

    public static ConsolidateWorker getInstance()
    {
        return worker;
    }

    private Dictionary<string,List<RecResult>> cacheRecResultDictionary = new Dictionary<string,List<RecResult>>();
    private Dictionary<string,List<Feature>> incompleteRecResultList = new Dictionary<string,List<Feature>>();
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

        ProjectEventDispatcher.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public void OnProjectStatusChange(object sender, ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            this.isProjectRunning = true;
            this.expectedFeatureCount = currentProject.Criterias.Length;
            this.enabledCriterias = currentProject.Criterias;
            
            consolidationPolicy = ConfigUtil.getModuleConfig().ConsolidatePolicy;
            criteriaMapping = ConfigUtil.getModuleConfig().CriteriaMapping;
            this.sortingInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
            
            cacheRecResultDictionary = new Dictionary<string,List<RecResult>>();
            incompleteRecResultList = new Dictionary<string,List<Feature>>();
            incompleteCoordinate = new Dictionary<string,Coordinate>();
            processResult();
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse ||
            statusEventArgs.State == ProjectState.washing)
        {
            isProjectRunning = false;
        }
    }

    public void processSingle(List<RecResult> rrs)
    {
        foreach (var value in rrs)
        {
            processBulk(value);
        }
    }

    public void processBulk(RecResult recResult)
    {
        if (!isProjectRunning) throw new ProjectDependencyException("ConsolidateWorker");
        if (!cacheRecResultDictionary.ContainsKey(recResult.Coordinate.Key()))
        {
            cacheRecResultDictionary.Add(recResult.Coordinate.Key(),new List<RecResult>());
        }
        cacheRecResultDictionary[recResult.Coordinate.Key()].Add(recResult);
        
        if(!incompleteRecResultList.ContainsKey(recResult.Coordinate.Key()))incompleteRecResultList.Add(recResult.Coordinate.Key(),new List<Feature>());
        if(!incompleteCoordinate.ContainsKey(recResult.Coordinate.Key())) incompleteCoordinate.Add(recResult.Coordinate.Key(),recResult.Coordinate);
    }

    private int[] cacheStatus = new int[3];
    private long runningCounter = 0;
   
    private void processResult()
    {
        Task.Run(() =>
        {   logger.Info("Consolidation worker starts");
            while (isProjectRunning)
            {   
                Thread.Sleep(sortingInterval);
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
                                incompleteRecResultList[key].Add(new Feature(criteriaIndex,consolidatedFeatureValue));
                                break;
                            }
                        }

                      
                    }

                }

                //check for merge completion;
                var completeResult = new List<RecResult>();
                foreach( (var key, var features) in incompleteRecResultList){
                    if (features.Count == expectedFeatureCount)
                    {
                        completeResult.Add(new RecResult(incompleteCoordinate[key],expectedFeatureCount,features));
                       logger.Debug("consolidated results{}",JsonConvert.SerializeObject(completeResult.Last()));
                    }
                }

                if (completeResult.Count > 0) DispatchResultEvent(new ResultEventArg(completeResult));
                
                //cleanup the caches
                foreach (var value in completeResult)
                {
                    cacheRecResultDictionary.Remove(value.Coordinate.Key());
                    incompleteRecResultList.Remove(value.Coordinate.Key());
                    incompleteCoordinate.Remove(value.Coordinate.Key());
                }

                cacheStatus[0] = cacheRecResultDictionary.Count;
                cacheStatus[1] = incompleteRecResultList.Count;
                cacheStatus[2] = incompleteCoordinate.Count;

                if (runningCounter * sortingInterval % 30000 == 0)
                {
                    logger.Info("Consolidate worker cache stats main cache {} processing cache {}",cacheStatus[0],cacheStatus[1]);
                }
            }
            
            logger.Info("Consolidation worker stops");
        });

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
}

public class ResultEventArg
{
    public List<RecResult> Results;

    public ResultEventArg(List<RecResult> results)
    {
        Results = results;
    }
}
