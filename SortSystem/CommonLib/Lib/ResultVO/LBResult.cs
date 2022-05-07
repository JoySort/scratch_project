using CommonLib.Lib.vo;

namespace CommonLib.Lib.Sort.ResultVO;

public class LBResult:SortResult
{

    private Outlet[] loadBalancedOutlet;

    public Outlet[] LoadBalancedOutlet => loadBalancedOutlet;

    public LBResult(Coordinate coordinate, int expectedFeatureCount, long recTimestamp, List<Feature> features, Outlet[] outlets, Outlet[] loadBalancedOutlet) : base(coordinate, expectedFeatureCount, recTimestamp, features, outlets)
    {
        this.loadBalancedOutlet = loadBalancedOutlet;
    }
}