using NDesk.Options;
using NLog;

namespace CommonLib.Lib.Util;

public class CMDArgumentUtil
{
    public static Logger logger = LogManager.GetCurrentClassLogger();
    private const string devConfigRoot = "../../../../UnitTest/config";
    public static string configRoot =devConfigRoot;
    public static int[] gid = new int[]{-1};
    public static bool standalone = true;
    public static void parse(string[] args)
    {
        
        var show_help = false;


        var configFolderSet = false;
        var p = new OptionSet ()
        {  
            { 
                "config_folder=", 
                "Specify config folder RELATIVE to app i.e. ../config ../../config,you need to put config out of program path to avoid overwrite from upgrade",
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
                "help",
                "show this message and exit",
                v => show_help = v != null
            },
            {
                "gid=",
                "Specify the sequence id of this program in the series",
                v =>
                {
                    string[] tmpGids = v.Split(",");
                    gid = tmpGids.Select(value => int.Parse(value)).ToArray();
                    //int.TryParse(v,out gid);
                }
            },
            {
                "standalone=",
                "wether run as a standalone application or recieve data from remote",
                v => {
                    bool.TryParse(v,out standalone);
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
                throw new Exception("命令行参数config_folder在生产环境下必须提供！如果这不是生产环境，检查你的环境变量设置 ASPNETCORE_ENVIRONMENT=Development");
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