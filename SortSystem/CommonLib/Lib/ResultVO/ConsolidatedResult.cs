namespace CommonLib.Lib.Sort.ResultVO;

public class ConsolidatedResult:RecResult
{
    public ConsolidatedResult(Coordinate coordinate, int expectedFeatureCount, List<Feature> features) : base(coordinate, expectedFeatureCount, features)
    {
    }
}