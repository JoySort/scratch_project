using CommonLib.Lib.vo;

namespace CommonLib.Lib.ConfigVO;

public class ModuleConfig
{
    private string author;
    private ConsolidatePolicy consolidatePolicy;
    private Dictionary<string, CriteriaMapping> criteriaMapping;

    private string description;
    private GenreName genre;
    private LowerConfig[] lowerConfig;
    private string minimumCoreVersion;
    private JoyModule module;
    private string name;
    private NetworkConfig networkConfig;
    private SortConfig sortConfig;
    private string title;
    private int version;

    public ModuleConfig(string author, ConsolidatePolicy consolidatePolicy, string description, GenreName genre,
        LowerConfig[] lowerConfig, string minimumCoreVersion, JoyModule module, string name,
        NetworkConfig networkConfig, SortConfig sortConfig, string title, int version,
        Dictionary<string, CriteriaMapping> criteriaMapping)
    {
        this.author = author;
        this.consolidatePolicy = consolidatePolicy;
        this.description = description;
        this.genre = genre;
        this.lowerConfig = lowerConfig;
        this.minimumCoreVersion = minimumCoreVersion;
        this.module = module;
        this.name = name;
        this.networkConfig = networkConfig;
        this.sortConfig = sortConfig;
        this.title = title;
        this.version = version;
        this.criteriaMapping = criteriaMapping;
    }

    public Dictionary<string, CriteriaMapping> CriteriaMapping => criteriaMapping;

    public SortConfig SortConfig
    {
        get => sortConfig;
        set => sortConfig = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Name => name;

    public JoyModule Module => module;

    public LowerConfig[] LowerConfig => lowerConfig;

    public NetworkConfig NetworkConfig => networkConfig;

    public GenreName Genre
    {
        get => genre;
        set { }
    }

    public string Description
    {
        get => description;
        set => description = value ?? throw new ArgumentNullException(nameof(value));
    }

    public int Version
    {
        get => version;
        set => version = value;
    }

    public string MinimumCoreVersion
    {
        get => minimumCoreVersion;
        set => minimumCoreVersion = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Title
    {
        get => title;
        set => title = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string Author
    {
        get => author;
        set => author = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ConsolidatePolicy ConsolidatePolicy => consolidatePolicy;
}

public enum JoyModule
{
    Camera,
    Lower,
    Recognizer,
    UI
}