using NDesk.Options;
using NLog;

namespace CommonLib.Lib.Util;

public class CMDArgumentUtil
{
    public static Logger logger = LogManager.GetCurrentClassLogger();
    private const string devConfigRoot = "../../../../UnitTest/config";
    private const string devconfigFile = "module.json";
    
    public static string configRoot = devConfigRoot;
    public static string configFile = devconfigFile;
    public static int[] gid = new int[]{-1};
    public static int standalone = -1;
    public static void parse(string[] args)
    {
        
        var show_help = false;
        
        var configFolderSet = false;
        var p = new OptionSet ()
        {  
            { 
                "config_folder=", 
                "Specify config folder RELATIVE to app i.e. `../config` or `../../config`, DO NOT use folder inside program execution path to avoid overwritten by upgrade package",
                v => { 
                    if (v != null) 
                    {
                        configRoot = v;
                        configFolderSet = true;
                        logger.Info("Using cmd parameter {}:{}","config_folder",v);
                    } 
                } 
            },
            { 
                "config_file=", 
                "Specify main config entry file. the file is defaulted as `module.json` if not set",
                v => { 
                    if (v != null) 
                    {
                        configFile = v;
                        logger.Info("Using cmd parameter {}:{}","--configFile",v);
                    } 
                } 
            },
            { 
                "help",
                "show this message and exit",
                v => show_help = v != null
            },
            {
                "gid=",
                "Specify the sequence id of this program in the series, should be provided with int value i.e. --gid=1 or --gid=0,1,2,3",
                v =>
                {
                    string[] tmpGids = v.Split(",");
                    try
                    {
                        gid = tmpGids.Select(value => int.Parse(value)).ToArray();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"gid require int value,{v} is not int or comma seperated int array. i.e. --gid=1 or --gid=0,1,2,3");
                    }
                }
            },
            {
                "standalone=",
                "whether run as a standalone application or recieve data from remote",
                v =>
                {
                    var tmpstandalone = false;
                    bool.TryParse(v,out tmpstandalone);
                    if (tmpstandalone == true) standalone = 1;
                    else standalone = 0;
                }
            }
        };

        try
        {
            p.Parse (args);
            var devEnvString = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevEnv = "Development"==devEnvString;
            if (!configFolderSet && !isDevEnv)
            {
                throw new Exception("command line configuration `config_folder` must be provided in production!");
            }
        }
        catch (OptionException e)
        {
            Console.Write ("bundling: ");
            Console.WriteLine (e.Message);
            Console.WriteLine ("Try `--help' for more information.");
            return;
        }

        if (show_help)
        {
            Show_help (p);
            return;
        }

        

    }
    static void Show_help (OptionSet p)
    {
        Console.WriteLine ("Parameterlist to use:");
        p.WriteOptionDescriptions (Console.Out);
    }
}