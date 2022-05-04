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
    private static string configFile = "module.json";
    public static void setConfigFolder(string cFolder)
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,cFolder);
        logger.Info("ConfigUtil setConfigFolder using config path :{}",path);
        
        bool dirExists = Directory.Exists(path);
        if (!dirExists)
        {
            throw new Exception("Folder doesn't exist "+path+" original using "+cFolder);
        }
        configFolder = cFolder;
    }
    
    public static void setConfigFile(string cFile)
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,configFolder+"/"+cFile);
        logger.Info("ConfigUtil setConfigFile config file:{}",path);
        
        bool dirExists = File.Exists(path);
        if (!dirExists)
        {
            throw new Exception("Folder doesn't exist "+path+" original using "+cFile);
        }
        configFile = cFile;
    }


    private static ModuleConfig? _moduleConfig;
    private static MachineState[]? _machineStates;
    private static Emitter[]? _emitters;
    public static ModuleConfig? getModuleConfig()
    {
        if (_moduleConfig == null) {
        string filePath = configFile;
        _moduleConfig =   JsonConvert.DeserializeObject<ModuleConfig>(loadSubConfig(filePath).ToString());
            if (CMDArgumentUtil.standalone != -1 && (CMDArgumentUtil.standalone==1 != _moduleConfig.Standalone))
            {
                logger.Warn($"Configuration file standalone:{_moduleConfig.Standalone} has a different value than CMD parameter {CMDArgumentUtil.standalone == 1}, Using CMD Parameter");
                _moduleConfig.Standalone = CMDArgumentUtil.standalone == 1 ? true : false;
            }
        }
        return _moduleConfig;

    }
    
    public static MachineState[] getMachineState()
    {
        if(_machineStates == null){
            _machineStates = getModuleConfig().MachineState ;
        }
        return _machineStates;
    }
    
    public static Emitter[] getEmitters()
    {
        if (_emitters == null)
        {
            _emitters = getModuleConfig().emiiters;
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