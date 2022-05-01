using CameraLib.Lib.Util;
using CameraLib.Lib.Worker;
using Initializer;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

CMDArgumentUtil.parse(args);// use cmd option --config_folder=../config to setup a config folder outside the program folder to avoid lose config when upgrade 
ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);
ModuleCommunicationWorker.getInstance();


//RecognizerWorkerManager.getInstance().setup(CMDArgumentUtil.standalone);
WebInitializer.init();