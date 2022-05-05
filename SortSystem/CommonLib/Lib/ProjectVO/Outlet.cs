namespace CommonLib.Lib.vo;

public class Outlet
{
    public string ChannelNo => channelNo;

    public string Type
    {
        get => type;
        set => type = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Filter[][] Filters
    {
        get => filters;
        set => filters = value ?? throw new ArgumentNullException(nameof(value));
    }

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