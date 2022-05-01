using CameraLib.Lib.Sort.ResultVO;
using NLog;

namespace CameraLib.Lib.LowerMachine.HardwareDriver;

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
                er.results[column - first] = (byte)outletNO;
                er.mask |= ((uint)1 << (column - first));
            }
            else
            {
                er = new EmitRecord(((uint)1 << (column - first)),new byte[last - first+1]);
                er.results[column - first] = (byte)outletNO;
                _emitRecords.Add(triggerID, er);
            }
        }

        long [] historyTriggerIDs = _emitRecords.Keys.ToArray();
        foreach (long id in historyTriggerIDs)
        {
            if (triggerID - id > 100)
            {
                _emitRecords.Remove(id);
                continue;

            }
            if (_emitRecords[id].mask == completedMask)
            {
                SendEmitResultCMD(_emitRecords[id].results, id);               
                _emitRecords.Remove(id);
            }
        }
    }

    public void EmitSingle(int column,int outletNo,long triggerID)
    {
        //logger.Debug("Emitter triggered column:{}-outletNo:{} triggerID:{}",column,outletNo,triggerID);
        //TODO: link to com communication
        
    }

    public void SendEmitResultCMD(byte[] results,long triggerID)
    {
        int resLen = results.Length;
        byte[] data = new byte[resLen  + 4];
        Array.Copy(results, data, resLen);

        triggerID -= 3;
        if(triggerID <0)
            triggerID = 0;
        data[resLen + 0] = (byte)(triggerID % 256);
        data[resLen + 1] = (byte)(triggerID / 256 % 256);
        data[resLen + 2] = (byte)(triggerID / 256 / 256 % 256);
        data[resLen + 3] = (byte)(triggerID / 256 / 256 / 256 % 256);

        comlink.writeMultipleRegs(new byte[2] { 0x00, 0x40 }, data, 0, 30);
    }

    Dictionary<long,EmitRecord> _emitRecords = new Dictionary<long,EmitRecord>();
    uint completedMask = 0;
    int first;
    int last;
}

public class EmitRecord
{
    public EmitRecord(uint mask,byte[] results)
    { 
        this.mask = mask;
        this.results = results;
    }
    public uint mask;
    public byte[] results;
}