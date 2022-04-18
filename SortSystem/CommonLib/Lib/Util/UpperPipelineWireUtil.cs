using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.Sort;

namespace CommonLib.Lib.Util;

public class UpperPipelineWireUtil
{
    public static void setup()
    {
        //Piple line wireup;
        ConsolidateWorker.getInstance().OnResult+=((sender, args) => SortingWorker.getInstance().processBulk(args.Results));
        SortingWorker.getInstance().OnResult+=((sender, args) => LBWorker.getInstance().processBulk(args.Results));
        LBWorker.getInstance().OnResult+=((sender, args) => EmitWorker.getInstance().processBulk(args.Results));
        EmitWorker.getInstance().OnResult+=((sender, args) => LowerMachineWorker.getInstance().processBulk(args.Results));
        LowerMachineWorker.init();
    }
}