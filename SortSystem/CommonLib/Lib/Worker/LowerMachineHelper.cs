using CommonLib.Lib.vo;

namespace CommonLib.Lib.Worker;

public class LowerMachineHelper
{
 
    private LowerMachineHelper()
    {
    }

    private Project currentProject;
    private ProjectStatus projectStatus = ProjectStatus.stop;
    
    private static LowerMachineHelper gl = new LowerMachineHelper();
    public static LowerMachineHelper getInstance()
    {
        return gl;
    }
    
    public event EventHandler<ProjectStatusEventArgs> ProjectStatusChanged;
    protected virtual void OnProjectStatusChange(ProjectStatusEventArgs e)
    {
        var handler = ProjectStatusChanged;
        handler?.Invoke(this, e);
    }

    public void dispatchProjectStatusChangeEvent(Project p,ProjectStatus s)
    {
       
        var discoverEventArgs = new ProjectStatusEventArgs
        {
            currentProject = p ?? currentProject,
            status =s
        };

        if (s == ProjectStatus.start && p != null)
        {
            currentProject = p;
            projectStatus = s;
        }

        if (s == ProjectStatus.stop)
        {
            currentProject = null;
            projectStatus = s;
        }

        OnProjectStatusChange(discoverEventArgs);
    }
}

public class ProjectStatusEventArgs : EventArgs
{
    public Project currentProject
    {
        set;
        get;
    }

    public ProjectStatus status
    {
        set;
        get;
    }
}

public enum ProjectStatus
{
    start,
    stop,
    pause,
    invalid
}