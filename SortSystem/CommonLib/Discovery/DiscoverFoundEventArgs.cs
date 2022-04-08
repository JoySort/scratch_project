namespace NetworkLib.Discovery;

public class DiscoverFoundEventArgs:EventArgs

{
    public string ipAddr { get; set; }
    public int rpcPort { get; set; }
}