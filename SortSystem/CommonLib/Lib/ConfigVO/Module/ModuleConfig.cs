using CommonLib.Lib.ConfigVO.Emission;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;
using NLog;

namespace CommonLib.Lib.ConfigVO;

public class ModuleConfig
{
    public static Logger logger = LogManager.GetCurrentClassLogger();
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
    private ElasticSearchConfig elasticSearchConfig;
    private bool standalone = true;

    public bool Standalone
    {
        get => standalone;
        set => standalone = value;
    }

    public ElasticSearchConfig ElasticSearchConfig => elasticSearchConfig;

    public RecognizerConfig RecognizerConfig => recognizerConfig;

    private string uuid = Guid.NewGuid().ToString();

    private string machineID;
    public string MachineID
    {
        get => string.IsNullOrEmpty(machineID)?"需要在LowerMachineDriver初始化之后才能调用machineID这个属性，如果不在上位机，需要自己从上位机获取这个属性":machineID;
        set=> machineID = value;
    }

    public string Id
    {

        get => MachineID+"-"+uuid;
    }

    public MachineState[] MachineState{
        get;
        set;
    }

    public Emitter[] emiiters
    {
        get;
        set;
    }


    public ModuleConfig(string author,bool standalone, ConsolidatePolicy consolidatePolicy, Dictionary<string, CriteriaMapping> criteriaMapping, string description, GenreName genre, LowerConfig[] lowerConfig, CameraConfig[] cameraConfigs, string minimumCoreVersion, JoyModule module, string name, NetworkConfig networkConfig, SortConfig sortConfig, bool lowerMachineSimulationMode, bool cameraSimulationMode, bool recognizerSimulationMode, RecognizerConfig recognizerConfig, ElasticSearchConfig elasticSearchConfig, string machineId, string title, int version, MachineState[] machineState, Emitter[] emiiters,string uuid)
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
        this.elasticSearchConfig = elasticSearchConfig;
        machineID = machineId;
        this.title = title;
        this.version = version;
        MachineState = machineState;
        this.emiiters = emiiters;
        this.standalone = standalone;
        if(uuid != null)
            if(uuid.Length > 0)
                this.uuid = uuid;
    }

    public string Uuid
    {
        get => uuid; 
        set => uuid = value;
    }

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
    private int initCols;
    private int initRows;
    private int[] columns;

    public RecognizerConfig(string dllPaht, string initializationImagePath, int initCols, int initRows, int[] columns)
    {
        this.dllPaht = dllPaht;
        this.initializationImagePath = initializationImagePath;
        this.initCols = initCols;
        this.initRows = initRows;
        this.columns = columns;
    }

    public int[] Columns
    {
        get => columns == null ?new int[]{0,9999}:columns;
        set => columns = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string DllPaht => dllPaht;

    public string InitializationImagePath => initializationImagePath;

    public int InitCols => initCols;
    public int InitRows => initRows;


}

public class ElasticSearchConfig
{
    public string url;
    public bool enabled;
 
}