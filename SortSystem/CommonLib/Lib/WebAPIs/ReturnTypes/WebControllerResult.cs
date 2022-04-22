namespace CommonLib.Lib.Controllers;

public class WebControllerResult
{
    private string message;

    public WebControllerResult(string message)
    {
        this.message = message;
    }

    public string Message => message;
}