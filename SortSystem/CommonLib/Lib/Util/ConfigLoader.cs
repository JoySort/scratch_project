using System.Reflection;
using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;

namespace CommonLib.Lib.Util;

public class ConfigLoader
{

 
    public static ModuleConfig loadModuleConfig(string filePath)
    { 
        
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,filePath);
        var porjectJsonString = File.ReadAllText(path);

        return  JsonConvert.DeserializeObject<ModuleConfig>(porjectJsonString);

    }

    public static MachineState[] loadMachineState(string filePath)
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,filePath);
        var porjectJsonString = File.ReadAllText(path);

        return  JsonConvert.DeserializeObject<MachineState[]>(porjectJsonString);
    }
}