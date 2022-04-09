namespace CommonLib.Lib.Network;

public class DiscoverFoundEventArgs:EventArgs

{
    public string ipAddr { get; set; }
    public int rpcPort { get; set; }
}