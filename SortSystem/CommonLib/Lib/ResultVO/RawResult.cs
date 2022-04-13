namespace CommonLib.Lib.Sort.ResultVO;

public class RawResult
{
    private Coordinate coordinate;
    //苹果会分别给出重量和识别结果，因此，可能会有2批结果需要缓存并且最终整合后才能使用。
    private int expectedFeatureCount;
    
    private long process_timestamp = DateTime.Now.ToFileTime()  ;
    public long ProcessTimestamp
    {
        get => process_timestamp;
        set => process_timestamp = value;

    }

    public RawResult(Coordinate coordinate, int expectedFeatureCount)
    {
        this.coordinate = coordinate;
        this.expectedFeatureCount = expectedFeatureCount;
    }

    public Coordinate Coordinate => coordinate;

    public int ExpectedFeatureCount => expectedFeatureCount;
}