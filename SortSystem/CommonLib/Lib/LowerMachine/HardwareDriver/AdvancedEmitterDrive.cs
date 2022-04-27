using CommonLib.Lib.Sort.ResultVO;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class AdvancedEmitterDrive:DriverBase
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public AdvancedEmitterDrive(ComLinkDriver cl) : base(cl)
    {
        int[] columns = cl.lowerConfig.Columns;
        first = columns.First();
        last = columns.Last();
        for (int i = 0; i <= last - first;i++)
        {
            completedMask |= (uint)(1 << i);
        }
        
    }

    public void EmitBulk(List<EmitResult> results)
    {
        logger.Debug("Advanced debugger emitting count:{},last triggerId of batch {}",results.Count,results.Count==0?-1:results.Last().TriggerId);
        long triggerID=0;
        foreach (var item in results)
        {
            var column = item.Column;
            if (column < first || column > last)
                continue;
            
            var outletNO = item.OutletNo; //Note, it ranges from 1-8, not start from 0
            triggerID = item.TriggerId;

            //EmitSingle(column, outletNO, triggerID);
            EmitRecord? er;
            if (_emitRecords.TryGetValue(triggerID, out er))
            {
                er.results[column - first] = outletNO;
                er.mask |= ((uint)1 << (column - first));
            }
            else
            {
                er = new EmitRecord(((uint)1 << (column - first)),new int[last - first+1]);
                er.results[column - first] = outletNO;
                _emitRecords.Add(triggerID, er);
            }
        }

        long [] historyTriggerIDs = _emitRecords.Keys.ToArray();
        foreach (long id in historyTriggerIDs)
        {
            if (triggerID - id > 100)
                _emitRecords.Remove(id);

            if (_emitRecords[id].mask == completedMask)
            { 
                byte[] data = new byte[last-first+1+4];
                Array.Copy(_emitRecords[id].results,data,last-first+1);
                data[last - first + 1] = (byte)(triggerID % 256);
                data[last - first + 2] = (byte)(triggerID/256 % 256);
                data[last - first + 3] = (byte)(triggerID/256/256 % 256);
                data[last - first + 4] = (byte)(triggerID/256/256/256 % 256);

                comlink.writeMultipleRegs(new byte[2] { 0x00, 0x40 },data, 0, 30);
            }
        }
    }

    public void EmitSingle(int column,int outletNo,long triggerID)
    {
        //logger.Debug("Emitter triggered column:{}-outletNo:{} triggerID:{}",column,outletNo,triggerID);
        //TODO: link to com communication
        
    }

    Dictionary<long,EmitRecord> _emitRecords = new Dictionary<long,EmitRecord>();
    uint completedMask = 0;
    int first;
    int last;
}

public class EmitRecord
{
    public EmitRecord(uint mask,int[] results)
    { 
        this.mask = mask;
        this.results = results;
    }
    public uint mask;
    public int[] results;
}