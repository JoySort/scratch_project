namespace CommonLib.Lib.vo;

public class Project
{
    private Genre genre;
    private Category category;
    private Criteria[] criterias;
    private Outlet[] outlets;

    public Project(Genre genre, Category category, Criteria[] criterias, Outlet[] outlets)
    {
        this.genre = genre;
        this.category = category;
        this.criterias = criterias;
        this.outlets = outlets;
    }
}