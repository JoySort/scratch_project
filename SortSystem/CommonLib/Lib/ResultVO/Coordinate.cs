namespace CommonLib.Lib.Sort.ResultVO;

public class Coordinate
{
    private int section;
    private int column;
    private int rowOffset;


    private long triggerID;

    public Coordinate(int section, int column, int rowOffset, long triggerId)
    {
        this.section = section;
        this.column = column;
        this.rowOffset = rowOffset;
        triggerID = triggerId;
    }

    public int Section => section;

    public int Column => column;

    public long TriggerId => triggerID;
    
    public int RowOffset => rowOffset;
}