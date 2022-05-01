using CameraLib.Lib.vo;

namespace CameraLib.Lib.Sort.ResultVO;

public class ESFeature
{
    public ESFeature(Feature feature,string filters,Outlet[] outlets, Outlet[] loadBalancedOutlet,string criteriaCode ,string machineId, string projectId, long projectTimestamp, Coordinate coordinate,bool consolidated,long createdTimestamp) 
    {
        _machineID = machineId;
        this.projectId = projectId;
        this.projectTimestamp = projectTimestamp;
        this.coordinate = coordinate;
        this.consolidated = consolidated;
        this.createdTimestamp = createdTimestamp;
        this.feature = feature;
        this.criteriaCode = criteriaCode;
        this.filters = filters;
        this.outlets = outlets;
        this.loadBalancedOutlet = loadBalancedOutlet;
    }

    private Outlet[] outlets;
    private Outlet[] loadBalancedOutlet;
    public string lbOutletNo { 
        get=>loadBalancedOutlet==null?"N/A":loadBalancedOutlet.First().ChannelNo;      
    }
    
    public int lbOutletNoIndex { 
        get=>loadBalancedOutlet==null?-1:Int32.Parse(this.loadBalancedOutlet?.First().ChannelNo)-1;      
    }

    public string outletNo
    {
        get => outlets==null?"N/A":string.Join(",", this.outlets?.SelectMany(value => value.ChannelNo)); 
    }

    
    public string filters;
   
    public Coordinate coordinate;

    public Feature feature
    {
        get;
        set;
    }

    public string criteriaCode;
    public bool consolidated ;
    public string projectId
    {
        get;
        set;
    }

    public long createdTimestamp
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

    // private string id = "";
    // // public string Id
    // // {
    // //     get => (projectTimestamp == null?"":projectTimestamp)+"-"+coordinate.Key()+"-csld"+(consolidated?1:0);
    // // } 
}