using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort.ResultVO;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
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
    private List<RecResult> toBeProcessedResults = new List<RecResult>();
    private List<RecResult> consolidatedResults = new List<RecResult>();
    private Dictionary<string,List<Feature>> uncompleteRecResultList = new Dictionary<string,List<Feature>>();
    private Dictionary<string,Coordinate> uncompleteCoordinate = new Dictionary<string,Coordinate>();
    private bool isProjectRunning;
    private int sortingInterval;
    private Project currentProject;
    private int expectedFeatureCount;
    private ConsolidatePolicy consolidationPolicy;
    private Dictionary<string,CriteriaMapping> criteriaMapping;

    private ConsolidateWorker()
    {
        consolidationPolicy = ConfigUtil.getModuleConfig().ConsolidatePolicy;
        criteriaMapping = ConfigUtil.getModuleConfig().CriteriaMapping;
        this.sortingInterval = ConfigUtil.getModuleConfig().SortConfig.SortingInterval;
        ProjectEventDispatcher.getInstance().ProjectStatusChanged += OnProjectStatusChange;
    }

    public void OnProjectStatusChange(object sender, ProjectStatusEventArgs statusEventArgs)
    {
        if (statusEventArgs.State == ProjectState.start && statusEventArgs.currentProject != null)
        {
            this.currentProject = statusEventArgs.currentProject;
            this.isProjectRunning = true;
            this.expectedFeatureCount = currentProject.Criterias.Length;
            
            
            processResult();
        }

        if (statusEventArgs.State == ProjectState.stop || statusEventArgs.State == ProjectState.reverse ||
            statusEventArgs.State == ProjectState.washing)
        {
            isProjectRunning = false;
        }
    }

    public void Consolidate(List<RecResult> rrs)
    {
        foreach (var value in rrs)
        {
            Consolidate(value);
        }
    }

    public void Consolidate(RecResult recResult)
    {
        if (!cacheRecResultDictionary.ContainsKey(recResult.Coordinate.Key()))
        {
            cacheRecResultDictionary.Add(recResult.Coordinate.Key(),new List<RecResult>());
        }
        cacheRecResultDictionary[recResult.Coordinate.Key()].Add(recResult);
        
        if(!uncompleteRecResultList.ContainsKey(recResult.Coordinate.Key()))uncompleteRecResultList.Add(recResult.Coordinate.Key(),new List<Feature>());
        if(!uncompleteCoordinate.ContainsKey(recResult.Coordinate.Key())) uncompleteCoordinate.Add(recResult.Coordinate.Key(),recResult.Coordinate);
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
                    for(var i=0;i<consolidationPolicy.OffSetRowCount.Length;i++){
                        if (value.Last().Coordinate.RowOffset == consolidationPolicy.OffSetRowCount[i]-1)//rowOffset starts from 0 so, count -1 
                        {
                            var criteriaIndex = criteriaMapping[consolidationPolicy.CriteriaCode[i]].Index;
                            float consolidatedFeatureValue = consolidateFeature(cacheRecResultDictionary[key],
                                key,criteriaIndex,i);
                            uncompleteRecResultList[key].Add(new Feature(criteriaIndex,consolidatedFeatureValue));
                            
                        }
                    }

                }

                //check for merge completion;
                var completeResult = new List<RecResult>();
                foreach( (var key, var features) in uncompleteRecResultList){
                    if (features.Count == expectedFeatureCount)
                    {
                        completeResult.Add(new RecResult(uncompleteCoordinate[key],expectedFeatureCount,features));
                       
                    }
                }
                
                //cleanup the caches
                foreach (var value in completeResult)
                {
                    cacheRecResultDictionary.Remove(value.Coordinate.Key());
                    uncompleteRecResultList.Remove(value.Coordinate.Key());
                    uncompleteCoordinate.Remove(value.Coordinate.Key());
                }

                cacheStatus[0] = cacheRecResultDictionary.Count;
                cacheStatus[1] = uncompleteRecResultList.Count;
                cacheStatus[2] = uncompleteCoordinate.Count;

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

        var calculationSelector = new List<float>();
        toBeConsolidatePartialRecResultList = toBeConsolidatePartialRecResultList.OrderBy(rrs => rrs.Coordinate.RowOffset).ToList();//order to make sure first and last operations are correct
        
        foreach (var recResult in toBeConsolidatePartialRecResultList)
        {
            foreach (var feature in recResult.Features)
                {
                    if (feature.CriteriaIndex == criteriaIndex)
                    {
                        calculationSelector.Add(feature.Value);
    
                    }
                }
        }

        var result = calculate(calculationSelector, consolidationPolicy.ConsolidationOperation[consolidatePolicyIndex],consolidatePolicyIndex);

        return result;
    }

    private float calculate(List<float> listValues,ConsolidateOperation ops,int consolidatePolicyIndex)
    {
        List<float> tmpList;
        
        switch (ops){
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
                for (var i = 0; i < consolidationPolicy.ConsolidateArg[consolidatePolicyIndex]; i++)
                {
                    tmpList.Add(listValues[i]);
                }

                return tmpList.Average();
            case ConsolidateOperation.lastNAvg:
                 tmpList = new List<float>();
                for (var i = consolidationPolicy.ConsolidateArg[consolidatePolicyIndex]-1 ; i >=0 ; i--)
                {
                    tmpList.Add(listValues[i]);
                }
                return tmpList.Average();
            case ConsolidateOperation.maxNAvg:
                tmpList = new List<float>();
                listValues = listValues.OrderBy(v=>v).ToList();
                for (var i = 0; i < consolidationPolicy.ConsolidateArg[consolidatePolicyIndex]; i++)
                {
                    tmpList.Add(listValues[i]);
                }
                return tmpList.Average();
            case ConsolidateOperation.minNAvg:
                tmpList = new List<float>();
                listValues = listValues.OrderByDescending(v=>v).ToList();
                for (var i = 0; i < consolidationPolicy.ConsolidateArg[consolidatePolicyIndex]; i++)
                {
                    tmpList.Add(listValues[i]);
                }
                return tmpList.Average();
        }

        return 0;
    }
}

