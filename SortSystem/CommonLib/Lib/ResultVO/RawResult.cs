namespace CameraLib.Lib.Sort.ResultVO;

public class RawResult
{
    private Coordinate coordinate;
    //苹果会分别给出重量和识别结果，因此，可能会有2批结果需要缓存并且最终整合后才能使用。
    private int expectedFeatureCount;
    
    private long created_timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()  ;
    public long CreatedTimestamp
    {
        get => created_timestamp;
        set => created_timestamp = value;

    }
    public RawResult(Coordinate coordinate, int expectedFeatureCount,long createdTimestamp)
    {
        this.coordinate = coordinate;
        this.expectedFeatureCount = expectedFeatureCount;
        this.created_timestamp = createdTimestamp;
    }

    public Coordinate Coordinate => coordinate;

    public int ExpectedFeatureCount => expectedFeatureCount;
}