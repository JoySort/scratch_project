using CameraLib.Lib.Util;
using CameraLib.Lib.Worker;
using Initializer;
using NLog;
using NLog.Web;
using RecognizerLib.Lib.Worker;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

//处理命令行参数
CMDArgumentUtil.parse(args);// use cmd option --config_folder=../config to setup a config folder outside the program folder to avoid lose config when upgrade 
ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);

//initiate network http client 
ModuleCommunicationWorker.getInstance();

RecognizerWorkerManager.getInstance().setup(CMDArgumentUtil.standalone);


WebInitializer.init();


//Do not put code below, won't run.
RecognizerWorkerManager.getInstance().tearDown();