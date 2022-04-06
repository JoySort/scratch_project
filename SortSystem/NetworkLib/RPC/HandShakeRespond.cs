namespace NetworkLib.RPC;

public class HandShakeVO
{

    
    public const string ROLE_UPPER="UPPER";
    public const string ROLE_LOWER = "lower";
    public const string ROLE_SENSOR = "SENSOR";//camera, infred etc
    public const string ROLE_UI = "UI";
        
    
    private string role;
    private int[] position;
    
}