using NDesk.Options;
using NLog;

namespace CommonLib.Lib.Util;

public class CMDArgumentUtil
{
    public static Logger logger = LogManager.GetCurrentClassLogger();
    public static string  configRoot   = "../../../config";
    public static void parse(string[] args)
    {
        
        var show_help = false;
        
      
     
        var p = new OptionSet ()
        {  
            { "config_folder=", "Specify config folder RELATIVE to app i.e. ../config ../../config,you need to put config out of program path to avoid overwrite from upgrade", v => { if (v != null) {configRoot = v; logger.Info("Using cmd parameter {}:{}","config_folder",v);} } },
            { "help",  "show this message and exit", v => show_help = v != null }
        };

        try
        {
            p.Parse (args);
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