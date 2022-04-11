namespace CommonLib.Lib.ConfigVO;

public class SortConfig
{
    private OutletPriority outletPriority;
    private int sortingInterval;

    public SortConfig(OutletPriority outletPriority, int sortingInterval)
    {
        this.outletPriority = outletPriority;
        this.sortingInterval = sortingInterval;
    }

    public int SortingInterval => sortingInterval;

    public OutletPriority OutletPriority => outletPriority;
}

public enum OutletPriority
{
    ASC,
    DESC
}