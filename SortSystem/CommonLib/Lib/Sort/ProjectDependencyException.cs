namespace CommonLib.Lib.Sort;

public class ProjectDependencyException:Exception
{
    public ProjectDependencyException(string? message) : base(message+"This API requires a project to be running, please start a project before using this API")
    {
    }
}