namespace CameraLib.Lib.vo;

public class Outlet
{
    public string ChannelNo => channelNo;

    public string Type => type;

    public Filter[][] Filters => filters;

    private string channelNo;
    private string type;
    private  Filter[][] filters;

    public Outlet(string channelNo, string type, Filter[][] filters)
    {
        this.channelNo = channelNo;
        this.type = type;
        this.filters = filters;
    }
}