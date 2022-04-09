using CommonLib.Lib.vo;

namespace CommonLib.Lib.ConfigVO;

public class ModuleConfig
{

    private string name;
    private JoyModule module;
    private LowerConfig[] lowerConfig;
    private Network network;
    private GenreName genre;
    private string description;
    private int version;
    private string minimumCoreVersion;
    private string title;
    private string author;
    public string Name => name;

    public JoyModule Module => module;

    public LowerConfig[] LowerConfig => lowerConfig;

    public Network Network => network;

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
    public ModuleConfig(string name, JoyModule module, LowerConfig[] lowerConfig, Network network, GenreName genre, string description, int version, string minimumCoreVersion, string title, string author)
    {
        this.name = name;
        this.module = module;
        this.lowerConfig = lowerConfig;
        this.network = network;
        this.genre = genre;
        this.description = description;
        this.version = version;
        this.minimumCoreVersion = minimumCoreVersion;
        this.title = title;
        this.author = author;
    }

}

public enum JoyModule
{
    Camera,
    Lower,
    Recognizer,
    UI
}
