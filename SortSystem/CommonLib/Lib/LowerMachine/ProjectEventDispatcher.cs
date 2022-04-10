using CommonLib.Lib.vo;

namespace CommonLib.Lib.LowerMachine;

public class ProjectEventDispatcher
{
 
    private ProjectEventDispatcher()
    {
    }

    private Project currentProject;
    private ProjectState projectState = ProjectState.stop;
    
    private static ProjectEventDispatcher gl = new ProjectEventDispatcher();
    public static ProjectEventDispatcher getInstance()
    {
        return gl;
    }
    
    public event EventHandler<ProjectStatusEventArgs> ProjectStatusChanged;
    protected virtual void OnProjectStatusChange(ProjectStatusEventArgs e)
    {
        var handler = ProjectStatusChanged;
        handler?.Invoke(this, e);
    }

    public void dispatchProjectStatusChangeEvent(Project? p,ProjectState s)
    {

        if (currentProject!=null && currentProject.Id == p.Id && projectState == s)
        {
            throw new Exception("Duplicated project "+s+" state in project "+p.Name+" with project id:"+p.Id);
        }
        if (projectState != ProjectState.start && projectState == s)
        {
            throw new Exception("Project  status is already in this state ");
        }

    
        var discoverEventArgs = new ProjectStatusEventArgs
        {
            currentProject = p ,
            State =s
        };
    
        
        if (s == ProjectState.start && p != null)
        {
            currentProject = p;
            projectState = s;
        }
        else
        {
            currentProject = null;
            projectState = s;
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
    } = null!;

    public ProjectState State
    {
        set;
        get;
    }
}

public enum ProjectState
{
    start,
    stop,
    pause,
    washing,
    reverse,
    invalid
}