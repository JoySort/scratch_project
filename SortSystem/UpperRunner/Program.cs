using CommonLib.Lib.Util;
using CommonLib.Lib.Worker;
using CommonLib.Lib.Worker.HTTP;
using CommonLib.Lib.Worker.Upper;
using Initializer;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("init main");

//Common env setup , must be the first to be initialized!
CommonEnvSetupUtil.init(args);

//Piple line wire;
UpperWorkerManager.getInstance().setup();

//WebInitializer must last line of code, no code beyond this point will be executed
WebInitializer.init();

//Do not put code below, won't run.
UpperWorkerManager.getInstance().tearDown();
