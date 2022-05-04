using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
using CommonLib.Lib.Worker.Upper;
using Initializer;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

//处理命令行参数
CommonEnvSetupUtil.init(args);


CameraWorkerManager.getInstance().setup();


WebInitializer.init();


CameraWorkerManager.getInstance().tearDown();