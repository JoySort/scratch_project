using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
using CommonLib.Lib.Worker.Upper;
using Initializer;
using NLog;
using NLog.Web;
using RecognizerLib.Lib.Worker;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");


//处理命令行参数
CommonEnvSetupUtil.init(args);



RecognizerWorkerManager.getInstance().setup();


WebInitializer.init();


//Do not put code below, won't run.
RecognizerWorkerManager.getInstance().tearDown();