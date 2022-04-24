namespace CommonLib.Lib.Controllers;

public class WebControllerResult
{
    private string message;
    private long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public WebControllerResult(string message)
    {
        this.message = message;
    }

    public string Message => message;
    public long Timestamp => timestamp;
}