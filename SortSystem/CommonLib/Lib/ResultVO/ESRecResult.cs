using System.Collections;

namespace CommonLib.Lib.Sort.ResultVO;

public class ESRecResult:RecResult
{
    public ESRecResult(Coordinate coordinate, int expectedFeatureCount, long createdTimestamp, List<Feature> features, string projectId, long projectTimestamp, string machineId) : base(coordinate, expectedFeatureCount, createdTimestamp, features)
    {
        _machineID = machineId;
        this.projectId = projectId;
        this.projectTimestamp = projectTimestamp;
        MachineID = machineId;
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