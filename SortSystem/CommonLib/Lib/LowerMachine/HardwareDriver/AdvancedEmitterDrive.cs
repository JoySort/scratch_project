using CommonLib.Lib.Sort.ResultVO;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class AdvancedEmitterDrive:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public AdvancedEmitterDrive(ComLinkDriver cl) : base(cl)
    {
    }

    public void EmitBulk(List<EmitResult> results)
    {
        logger.Debug("Advanced debugger emitting count:{}",results.Count);
        foreach (var item in results)
        {
            var column = item.Column;
            var outletNO = item.OutletNo; //Note, it ranges from 1-8, not start from 0
            var triggerID = item.TriggerId;
            EmitSingle(column, outletNO, triggerID);
            //doing stuff
        }
    }

    public void EmitSingle(int column,int outletNo,long triggerID)
    {
        //logger.Debug("Emitter triggered column:{}-outletNo:{} triggerID:{}",column,outletNo,triggerID);
        //TODO: link to com communication
        
    }
}