using CommonLib.Lib.vo;

namespace CommonLib.Lib.Sort.ResultVO;

public class ESLBResult:LBResult
{
    public ESLBResult(Coordinate coordinate, int expectedFeatureCount, long createdTimestamp, List<Feature> features, Outlet[] outlets, Outlet[] loadBalancedOutlet, string machineId, string projectId, long projectTimestamp) : base(coordinate, expectedFeatureCount, createdTimestamp, features, outlets, loadBalancedOutlet)
    {
        _machineID = machineId;
        this.projectId = projectId;
        this.projectTimestamp = projectTimestamp;
        MachineID = machineId;
    }

    public string lbOutletNo { 
        get=>this.LoadBalancedOutlet.First().ChannelNo;      
    }
    
    public int lbOutletNoIndex { 
        get=>Int32.Parse(this.LoadBalancedOutlet.First().ChannelNo)-1;      
    }

    public string outletNo
    {
        get => string.Join(",", this.Outlets.SelectMany(value => value.ChannelNo)); 
    }
    
 

    public long TimeTook
    {
        get => DateTimeOffset.Now.ToUnixTimeMilliseconds() - CreatedTimestamp;
    }

    public string projectId
    {
        get;
        set;
    }

    public long projectTimestamp
    {
        get;
        set;
    }

    private string _machineID="";
    public string MachineID {
        get;
        set;
    }

    private string id = "";
    public string Id
    {
        get => (projectTimestamp == null?"":projectTimestamp)+"-"+Coordinate.Key();
    }
}