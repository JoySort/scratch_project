namespace CommonLib.Lib.Network;

public class DiscoverFoundEventArgs:EventArgs

{
    public string ipAddr { get; set; } = null!;
    public string uuid { get; set; }
    public int rpcPort { get; set; }
}