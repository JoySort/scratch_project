namespace CameraLib.Lib.ConfigVO;

public class SortConfig
{
    private OutletPriority outletPriority;
    private int sortingInterval;
    private ConsolidatePolicy consolidatePolicy;

    public SortConfig(OutletPriority outletPriority, int sortingInterval, ConsolidatePolicy consolidatePolicy)
    {
        this.outletPriority = outletPriority;
        this.sortingInterval = sortingInterval;
        this.consolidatePolicy = consolidatePolicy;
    }

    public ConsolidatePolicy ConsolidatePolicy => consolidatePolicy;

    public int SortingInterval => sortingInterval;

    public OutletPriority OutletPriority
    {
        get => outletPriority;
        set => outletPriority = value;
    }
}

public enum OutletPriority
{
    ASC,
    DESC
}

public class CriteriaMapping{
    private int index;
    private string name;
    private string key;

    public CriteriaMapping(int index, string name, string key)
    {
        this.index = index;
        this.name = name;
        this.key = key;
    }

    public int Index => index;

    public string Name => name;

    public string Key => key;
}

/**
 * <summary>
 * for Apple:
 * {
 *  offSetRowCount:[2], //称重数据和识别数据一共是2个
 *  offSetRowStep:[23],//到称重的距离
 *  consolidationOperation: ["merge"] ,// 直接合并
 *  criteriaCode:[wt] //weight
 *  consolidateArg:[0] //这里没有用
 * }
 * * for pd:
 * {
 *  offSetRowCount:[4], //4张照片
 *  offSetRowStep:[1],//每张照片差1行
 *  consolidationOperation: ["maxNAvg"] ,// 最大的2个平均值
 *  criteriaIndex:[yc] //yellow cap 的index
 *  consolidateArg:[2] //maxNAvg中的N是2
 * }
 * </summary>
 */
public class ConsolidatePolicy
{
    private int[] offSetRowCount;
    private int[] offSetRowStep;
    private  ConsolidateOperation[] consolidationOperation;
    private string[] criteriaCode;
    private int[] consolidateArg;

    public ConsolidatePolicy(int[] offSetRowCount, int[] offSetRowStep, ConsolidateOperation[] consolidationOperation, string[] criteriaCode, int[] consolidateArg)
    {
        this.offSetRowCount = offSetRowCount;
        this.offSetRowStep = offSetRowStep;
        this.consolidationOperation = consolidationOperation;
        this.criteriaCode = criteriaCode;
        this.consolidateArg = consolidateArg;
    }

    public int[] OffSetRowCount => offSetRowCount;

    public int[] OffSetRowStep => offSetRowStep;

    public ConsolidateOperation[] ConsolidationOperation => consolidationOperation;

    public string[] CriteriaCode => criteriaCode;

    public int[] ConsolidateArg => consolidateArg;
}

public enum ConsolidateOperation
{
    merge,
    max,
    min,
    avg,
    maxNAvg,
    minNAvg,
    firstNAvg,
    lastNAvg
}