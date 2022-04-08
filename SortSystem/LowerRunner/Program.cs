using CommonLib.Lib.Util;
using LowerRunner;
using NDesk.Options;
using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

//处理命令行参数
CMDArgumentUtil.parse(args);// use cmd option --config_folder=../../config to setup a config folder outside the program folder to avoid lose config when upgrade 
ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);
WebInitializer.init();
NetworkInitializer.UDPDiscoverSetup();

