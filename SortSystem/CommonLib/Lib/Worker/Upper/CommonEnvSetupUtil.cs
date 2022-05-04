using CommonLib.Lib.Util;

namespace CommonLib.Lib.Worker.Upper;

public static class CommonEnvSetupUtil
{
    public static void init(string[] args)
    {
        //处理命令行参数
        CMDArgumentUtil.parse(args);// use cmd option --config_folder=../config to setup a config folder outside the program folder to avoid lose config when upgrade 
        
        //Config setup
        ConfigUtil.setConfigFolder(CMDArgumentUtil.configRoot);
        ConfigUtil.setConfigFile(CMDArgumentUtil.configFile);
        
        //initiate network http client 
        ModuleCommunicationWorker.getInstance();
    }
}