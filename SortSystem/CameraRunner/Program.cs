using CommonLib.Lib.Network;
using CommonLib.Lib.Util;
using Initializer;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

//处理命令行参数
CMDArgumentUtil.parse(args);// use cmd option --config_folder=../config to setup a config folder outside the program folder to avoid lose config when upgrade 
ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);
NetworkUtil.getInstance().UDPDiscoverSetup();


WebInitializer.init();
//Do not put code below, won't run.