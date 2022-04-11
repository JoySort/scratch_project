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

    public bool isSame(Coordinate cd)
    {
        if (cd == null) return false;
        bool result = cd.Section == this.Section && cd.Column == this.Column && cd.RowOffset == this.RowOffset &&
                      cd.TriggerId == this.TriggerId;

        return result;
    }

    public int Section => section;

    public int Column => column;

    public long TriggerId => triggerID;
    
    public int RowOffset => rowOffset;
}