using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.Worker.Upper;

public class UpperToCameraHTTPClientWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool isProjectRunning = false;
    private Project currentProject;

    private static UpperToCameraHTTPClientWorker me = new UpperToCameraHTTPClientWorker();
    public static UpperToCameraHTTPClientWorker getInstance()
    {
        return me;
    }
    private void init()
    {
        ProjectManager.getInstance().ProjectStatusChanged += OnProjectStatusChange;
        
    }
    private UpperToCameraHTTPClientWorker()
    {
        init();

    }
    private void OnProjectStatusChange(object? sender, ProjectStatusEventArgs e)
    {
        
    }
}