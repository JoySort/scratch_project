namespace CommonLib.Lib.Worker;

public class StatsHolder
{
    public long StartTime;
    public long FinishTime;
    public long extraTimestamp;
    public long TriggerId;
    public int count;
    public string extra;
    public long timeTook => FinishTime - StartTime;


}