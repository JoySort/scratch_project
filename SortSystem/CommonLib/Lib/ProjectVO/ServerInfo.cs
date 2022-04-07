namespace CommonLib.Lib.vo;

public class ServerInfo
{
    public string Name { get; set; }
    public int ServiceCode { get; set; }

    public ServerInfo(string name, int serviceCode)
    {
        Name = name;
        ServiceCode = serviceCode;
    }
}