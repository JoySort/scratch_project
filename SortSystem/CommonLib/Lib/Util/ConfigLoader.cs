using System.Reflection;
using CommonLib.Lib.ConfigVO;
using Newtonsoft.Json;

namespace CommonLib.Lib.Util;

public class ConfigLoader
{

    public static Configuration load()
    {
        string JsonFilePath = @"./config/joy_config.json";
        return load(JsonFilePath);
    }

    public static Configuration load(string filePath)
    { 
        
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,filePath);
        var porjectJsonString = File.ReadAllText(path);

        return  JsonConvert.DeserializeObject<Configuration>(porjectJsonString);

    }
}