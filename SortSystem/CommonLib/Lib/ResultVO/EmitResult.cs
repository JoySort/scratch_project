namespace CameraLib.Lib.Sort.ResultVO;

public class EmitResult
{
    private int column;
    private int outletNo;
    private long triggerID;

    public EmitResult(int column, int outletNo, long triggerId)
    {
        this.column = column;
        this.outletNo = outletNo;
        triggerID = triggerId;
    }

    public int Column => column;

    public int OutletNo => outletNo;

    public long TriggerId => triggerID;
}