using System.Reflection;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.ConfigVO.Emission;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CommonLib.Lib.Util;

public class ConfigUtil
{
    public static Logger logger = LogManager.GetCurrentClassLogger();

    private static string configFolder ="../../../config";
    public static void setConfigFolder(string cFolder)
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,cFolder);
        logger.Info("ConfigUtil using path config:{}",path);
        
        bool dirExists = Directory.Exists(path);
        if (!dirExists)
        {
            throw new Exception("Folder doesn't exist "+path+" original using "+cFolder);
        }
        configFolder = cFolder;
    }


    private static ModuleConfig? _moduleConfig;
    private static MachineState[]? _machineStates;
    private static Emitter[]? _emitters;
    public static ModuleConfig? getModuleConfig()
    {
        if (_moduleConfig == null) {
        string filePath = configFolder+"/module.json";
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,filePath);
        logger.Debug("using path "+path);
        var jsonString = File.ReadAllText(path);
        
        _moduleConfig =   JsonConvert.DeserializeObject<ModuleConfig>(loadSubConfig(jsonString).ToString());
        }
        return _moduleConfig;

    }
    
    public static MachineState[] getMachineState()
    {
        if(_machineStates == null){
        string filePath = configFolder+"/state.json";
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,filePath);
        logger.Debug("using path "+path);
        var jsonString = File.ReadAllText(path);
        _machineStates = JsonConvert.DeserializeObject<MachineState[]>(jsonString);
        getModuleConfig().MachineState = _machineStates;
        }

        

        return _machineStates;
    }
    
    public static Emitter[] getEmitters()
    {
        if (_emitters == null)
        {
            string filePath = configFolder + "/emitters.json";
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                filePath);
            logger.Debug("using path " + path);
            var jsonString = File.ReadAllText(path);
            _emitters = JsonConvert.DeserializeObject<Emitter[]>(jsonString);
            getModuleConfig().emiiters = _emitters;
        }

        return _emitters;
    }

    private static void addFileToWatch(string filePath)
    {
        using var watcher = new FileSystemWatcher(filePath);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnChanged;
    }
    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        Console.WriteLine($"Changed: {e.FullPath}");
    }



   
    public static JToken loadSubConfig(string subConfigPath)
    {
        string filePath = configFolder+"/"+subConfigPath;
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,filePath);
        logger.Debug("loading sub config path "+path);
        var jsonString = File.ReadAllText(path);
        JToken jresult = null;
        try
        {
            var jObject= JObject.Parse(jsonString);
            var resultList = new Dictionary<string, object>();
            foreach (var jToken in jObject)
            {
                
                    foreach (var token in jToken.Value)
                    {
                        JProperty myProperty = null;
                        if (token is JProperty)
                        {
                            myProperty = (JProperty) token;
                            if (myProperty.Name == "import")
                            {
                                resultList.Add(jToken.Key,loadSubConfig((string)(myProperty.Value)));
                                //jObject[jToken.Key] = parseSubConfig(jresult);
                                break;
                            }

                            
                        }

                        
                    }
                
            }
            foreach (var keyValuePair in resultList)
            {
                jObject[keyValuePair.Key] = (JToken?) keyValuePair.Value;
            }

            jresult = jObject;
        }
        catch (Exception e1)
        {
            try
            {
                jresult = JArray.Parse(jsonString);
            }
            catch (Exception e2)
            {
                
            }
        }

       
        
    
    

        return jresult;
    }
}

public enum ConfigFile
{
    module,
    state,
    emitters
}