using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Network;
using CommonLib.Lib.Sort;
using CommonLib.Lib.Util;

using LowerRunner;
using NDesk.Options;
using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

//处理命令行参数
CMDArgumentUtil.parse(args);// use cmd option --config_folder=../config to setup a config folder outside the program folder to avoid lose config when upgrade 
ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);
NetworkUtil.UDPDiscoverSetup();


//Piple line wireup;
ConsolidateWorker.getInstance().OnResult+=((sender, args) => SortingWorker.getInstance().processBulk(args.Results));
SortingWorker.getInstance().OnResult+=((sender, args) => LBWorker.getInstance().processBulk(args.Results));
LBWorker.getInstance().OnResult+=((sender, args) => EmitWorker.getInstance().processBulk(args.Results));
EmitWorker.getInstance().OnResult+=((sender, args) => LowerMachineWorker.getInstance().processBulk(args.Results));







//WebInitializer must last line of code, no code beyond this point will be executed
WebInitializer.init();
//Do not put code below, won't run.


