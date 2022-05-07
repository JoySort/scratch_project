namespace CommonLib.Lib.Sort.ResultVO;

public class ConsolidatedResult:RecResult
{
    public ConsolidatedResult(Coordinate coordinate, int expectedFeatureCount, long recTimestamp, List<Feature> features) : base(coordinate, expectedFeatureCount, recTimestamp, features)
    {
    }
}