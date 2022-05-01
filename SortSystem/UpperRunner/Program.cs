using CameraLib.Lib.ConfigVO;
using CameraLib.Lib.LowerMachine;
using CameraLib.Lib.Network;
using CameraLib.Lib.Util;
using CameraLib.Lib.Worker;
using CameraLib.Lib.Worker.Upper;
using Initializer;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

//处理命令行参数
CMDArgumentUtil.parse(args);// use cmd option --config_folder=../config to setup a config folder outside the program folder to avoid lose config when upgrade 
ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);

//LowerMachineDriver.getInstance();
//initiate network http client 
ModuleCommunicationWorker.getInstance();
UpperToCameraHTTPClientWorker.getInstance();

//Piple line wireup;
UpperWorkerManager.getInstance();

//WebInitializer must last line of code, no code beyond this point will be executed
WebInitializer.init();
//Do not put code below, won't run.
UpperWorkerManager.getInstance().tearDown();
