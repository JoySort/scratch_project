namespace CommonLib.Lib.Controllers;

public class UIAPIResult
{
    private string code = "1";
    private string error_message = "";
    private JoyError error_var;
    private string status;
    private IJoyResult data;

    public string Code => code;

    public string Error_Message => error_message;

    public JoyError ErrorVar => error_var;

    public string Status => status;

    public IJoyResult Data => data;

    public UIAPIResult(string code, string errorMessage, JoyError errorVar, string status, IJoyResult data)
    {
        this.code = code;
        error_message = errorMessage;
        error_var = errorVar;
        this.status = status;
        this.data = data;
    }

   
}