namespace CommonLib.Lib.Sort.ResultVO;

public class ConsolidatedResult:RecResult
{
    public ConsolidatedResult(Coordinate coordinate, int expectedFeatureCount, long createdTimestamp, List<Feature> features) : base(coordinate, expectedFeatureCount, createdTimestamp, features)
    {
    }
}