namespace CameraLib.Lib.Controllers;

public class UIAPIResult
{
    private string code = "1";
    private string error_message = "";
    private JoyError error_var;
    private string status;
    private Object data;

    public string Code => code;

    public string Error_Message => error_message;

    public JoyError ErrorVar => error_var;

    public string Status => status;

    public Object Data => data;

    public UIAPIResult(string code, string errorMessage, JoyError errorVar, string status, IJoyResult data)
    {
        this.code = errorVar.e != null ? "2" : "1";
        error_message = (errorVar.e ??"");
        error_var = errorVar;
        this.status = (errorVar.e != null ? "error" : "ok");
        this.data = data;
    }

    public UIAPIResult(JoyError errorVar, Object data)
    {
        error_var = errorVar;
        this.data = data;
        this.code = errorVar.e != null ? "2" : "1";
        this.error_message = (errorVar.e ??"");
        this.status = (errorVar.e != null ? "error" : "ok");
    }
}