using CommonLib.Lib.Network;
using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
using CommonLib.Lib.Worker.Camera;
using CommonLib.Lib.Worker.Recognizer;
using CommonLib.Lib.Worker.Upper;
using Initializer;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

//处理命令行参数
CMDArgumentUtil.parse(args);// use cmd option --config_folder=../config to setup a config folder outside the program folder to avoid lose config when upgrade 
ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);

//initiate network http client 
ModuleCommunicationWorker.getInstance();

CameraWorker.getInstance();//用于处理照片存储等任务
RecognizerWorker.getInstance().RecResultGenerated += CameraToUpperHTTPClientWorker.getInstance().onRecResultGenerated; //将识别结果通过HttpClient发出去。

WebInitializer.init();
//Do not put code below, won't run.