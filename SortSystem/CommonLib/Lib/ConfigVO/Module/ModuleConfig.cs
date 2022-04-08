namespace CommonLib.Lib.ConfigVO;

public class ModuleConfig
{

    private string name;
    private JoyModule module;
    private LowerConfig[] lowerConfig;
    private Network network;

    public string Name => name;

    public JoyModule Module => module;

    public LowerConfig[] LowerConfig => lowerConfig;

    public Network Network => network;

    public ModuleConfig(string name, JoyModule module, LowerConfig[] lowerConfig, Network network)
    {
        this.name = name;
        this.module = module;
        this.lowerConfig = lowerConfig;
        this.network = network;
    }
}

public enum JoyModule
{
    Camera,
    Lower,
    Upper,
    UI
}
