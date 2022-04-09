namespace CommonLib.Lib.Sort.ResultVO;

public class RawResult
{
    private Coordinate[] coordinates;
    private int offset ;

    public RawResult(Coordinate[] coordinates, int offset)
    {
        this.coordinates = coordinates;
        this.offset = offset;
    }

    public Coordinate[] Coordinates
    {
        get => coordinates;
        set => coordinates = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int Offset
    {
        get => offset;
        set => offset = value;
    }
}