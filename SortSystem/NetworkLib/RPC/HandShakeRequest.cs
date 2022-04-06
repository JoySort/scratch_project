namespace NetworkLib.RPC;

public class HandShakeRequest
{
    private string target_addr;
    private int target_port;

    public HandShakeRequest(string targetAddr, int targetPort)
    {
        target_addr = targetAddr;
        target_port = targetPort;
    }

    public string TargetAddr => target_addr;

    public int TargetPort => target_port;
}