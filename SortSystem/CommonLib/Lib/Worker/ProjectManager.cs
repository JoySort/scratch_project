using System.Runtime.Serialization.Formatters.Binary;
using CommonLib.Lib.vo;
using NLog;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;

namespace CommonLib.Lib.LowerMachine;

public class ProjectManager
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
 
    private ProjectManager()
    {
    }

    private  long startTimestamp;

    public  long StartTimestamp => startTimestamp;

    private Project currentProject;
    private ProjectState projectState = ProjectState.stop;
    
    
    public ProjectState ProjectState => projectState;

    private static ProjectManager gl = new ProjectManager();
    public static ProjectManager getInstance()
    {
        return gl;
    }
    
    public event EventHandler<ProjectStatusEventArgs> ProjectStatusChanged;
    protected virtual void OnProjectStatusChange(ProjectStatusEventArgs e)
    {
        logger.Info($"Dispatching project status change from ProjectManager {Enum.GetName(e.State)}");
        var handler = ProjectStatusChanged;
        handler?.Invoke(this, e);
    }

    public void dispatchProjectStatusStartEvent(Project p, ProjectState s)
    {
        if (currentProject!=null && currentProject.Id == p.Id && projectState == s)
        {
            throw new Exception("Duplicated project "+s+" state in project "+p.Name+" with project id:"+p.Id);
        }

        if (projectState == ProjectState.pause && s == ProjectState.start)
        {
            throw new Exception("Invalid project state change from pause to start, it should resume first");
        }

        var discoverEventArgs = new ProjectStatusEventArgs
        {
            currentProject = p ,
            State =s
        };
        currentProject = JsonConvert.DeserializeObject<Project>(JsonConvert.SerializeObject(p));
        projectState = s;
        OnProjectStatusChange(discoverEventArgs);
    }
   
    public void dispatchProjectStatusChangeEvent(ProjectState s)
    {


        if (projectState != ProjectState.start && projectState == s)
        {
            throw new Exception("Project  status is already in this state ");
        }

        if (s == ProjectState.pause && projectState == ProjectState.stop)
        {
            throw new Exception("Invalid project status change to pause from stop");
        }

        var discoverEventArgs = new ProjectStatusEventArgs
        {
            State =s
        };
        if (s == ProjectState.start)
        {
           this.startTimestamp= DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        projectState = s;
        OnProjectStatusChange(discoverEventArgs);
    }

    public Project CurrentProject => currentProject;
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
    resume,
    washing,
    reverse,
    update,
    invalid
}