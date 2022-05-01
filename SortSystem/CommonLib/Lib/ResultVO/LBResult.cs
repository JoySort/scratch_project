using CameraLib.Lib.vo;

namespace CameraLib.Lib.Sort.ResultVO;

public class LBResult:SortResult
{

    private Outlet[] loadBalancedOutlet;

    public Outlet[] LoadBalancedOutlet => loadBalancedOutlet;

    public LBResult(Coordinate coordinate, int expectedFeatureCount, long createdTimestamp, List<Feature> features, Outlet[] outlets, Outlet[] loadBalancedOutlet) : base(coordinate, expectedFeatureCount, createdTimestamp, features, outlets)
    {
        this.loadBalancedOutlet = loadBalancedOutlet;
    }
}