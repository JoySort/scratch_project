namespace CameraLib.Lib.vo;

public class Project
{
    private string id;
    private string name;
    private Genre genre;
    private Category category;
    private Criteria[] criterias;
    private Outlet[] outlets;

    public string Id => id;

    public string Name => name;

    public Genre Genre => genre;

    public Category Category => category;

    public Criteria[] Criterias => criterias;

    public Outlet[] Outlets => outlets;

    private long createdTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public long TimeStamp
    {
        get => createdTimestamp;
        set => createdTimestamp=value;
    }

    public Project(string id, string name, Genre genre, Category category, Criteria[] criterias, Outlet[] outlets)
    {
        this.id = id;
        this.name = name;
        this.genre = genre;
        this.category = category;
        this.criterias = criterias;
        this.outlets = outlets;
    }
}