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
    private CameraConfig[] cameraConfigs;
    private string minimumCoreVersion;
    private JoyModule module;
    private string name;
    private NetworkConfig networkConfig;
    private SortConfig sortConfig;
    private bool lowerMachineSimulationMode = false;
    private bool cameraSimulationMode = false;
    private bool recognizerSimulationMode = false;
    private RecognizerConfig recognizerConfig;

    public RecognizerConfig RecognizerConfig => recognizerConfig;

    private string uuid = Guid.NewGuid().ToString();

    public ModuleConfig(string author, ConsolidatePolicy consolidatePolicy, Dictionary<string, CriteriaMapping> criteriaMapping, string description, GenreName genre, LowerConfig[] lowerConfig, CameraConfig[] cameraConfigs, string minimumCoreVersion, JoyModule module, string name, NetworkConfig networkConfig, SortConfig sortConfig, bool lowerMachineSimulationMode, bool cameraSimulationMode, bool recognizerSimulationMode, RecognizerConfig recognizerConfig, string title, int version)
    {
        this.author = author;
        this.consolidatePolicy = consolidatePolicy;
        this.criteriaMapping = criteriaMapping;
        this.description = description;
        this.genre = genre;
        this.lowerConfig = lowerConfig;
        this.cameraConfigs = cameraConfigs;
        this.minimumCoreVersion = minimumCoreVersion;
        this.module = module;
        this.name = name;
        this.networkConfig = networkConfig;
        this.sortConfig = sortConfig;
        this.lowerMachineSimulationMode = lowerMachineSimulationMode;
        this.cameraSimulationMode = cameraSimulationMode;
        this.recognizerSimulationMode = recognizerSimulationMode;
        this.recognizerConfig = recognizerConfig;
        this.title = title;
        this.version = version;
    }

    public string Uuid => uuid;

    public bool LowerMachineSimulationMode
    {
        get => lowerMachineSimulationMode;
        set => lowerMachineSimulationMode = value;
    }

    public bool CameraSimulationMode
    {
        get => cameraSimulationMode;
        set => cameraSimulationMode = value;
    }

    
    public bool RecognizerSimulationMode => recognizerSimulationMode;

    public CameraConfig[] CameraConfigs => cameraConfigs;

    private string title;
    private int version;


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
    Upper,
    UI
}

public class RecognizerConfig
{
    private string dllPaht;
    private string initializationImagePath;

    public string DllPaht => dllPaht;

    public string InitializationImagePath => initializationImagePath;

    public RecognizerConfig(string dllPaht, string initializationImagePath)
    {
        this.dllPaht = dllPaht;
        this.initializationImagePath = initializationImagePath;
    }
}