namespace CommonLib.Lib.Sort.ResultVO;

public class Coordinate
{
    private int section;
    private int position;
    private long triggerID;

    public Coordinate(int section, int position, long triggerId)
    {
        this.section = section;
        this.position = position;
        triggerID = triggerId;
    }

    public int Section => section;

    public int Position => position;

    public long TriggerId => triggerID;
}