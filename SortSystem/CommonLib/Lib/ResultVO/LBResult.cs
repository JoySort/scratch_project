using CommonLib.Lib.vo;

namespace CommonLib.Lib.Sort.ResultVO;

public class LBResult:SortResult
{

    private Outlet[] loadBalancedOutlet;

    public Outlet[] LoadBalancedOutlet => loadBalancedOutlet;

    public LBResult(Coordinate[] coordinates, Feature[] features, Outlet[] outlets, Outlet[] loadBalancedOutlet) : base(coordinates, features, outlets)
    {
        this.loadBalancedOutlet = loadBalancedOutlet;
    }
}