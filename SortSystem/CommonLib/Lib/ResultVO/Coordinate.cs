using CommonLib.Lib.ConfigVO;

namespace CommonLib.Lib.Sort.ResultVO;

public class Coordinate
{

    private int column;
    private int rowOffset;
    private CameraPosition cameraPosition;

    private long triggerID;

    public Coordinate(int column, int rowOffset, CameraPosition cameraPosition, long triggerId)
    {
        this.column = column;
        this.rowOffset = rowOffset;
        this.cameraPosition = cameraPosition;
        triggerID = triggerId;
    }

    public string Key() =>  "t"+ triggerID+"-c" + column +"-p_"+cameraPosition;

    public bool isSame(Coordinate cd)
    {
        if (cd == null) return false;
        bool result =cd.cameraPosition==this.cameraPosition && cd.Column == this.Column && cd.RowOffset == this.RowOffset &&
                      cd.TriggerId == this.TriggerId;

        return result;
    }


    public int Column => column;

    public long TriggerId => triggerID;
    
    public int RowOffset => rowOffset;

    public CameraPosition CameraPosition => cameraPosition;
    
}