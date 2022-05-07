using CommonLib.Lib.vo;

namespace CommonLib.Lib.Sort.ResultVO;

public class SortResult:RecResult
{
  
    private Outlet[] outlets;


    public SortResult(Coordinate coordinate, int expectedFeatureCount, long recTimestamp, List<Feature> features, Outlet[] outlets) : base(coordinate, expectedFeatureCount, recTimestamp, features)
    {
        this.outlets = outlets;
    }

    public Outlet[] Outlets => outlets;
}