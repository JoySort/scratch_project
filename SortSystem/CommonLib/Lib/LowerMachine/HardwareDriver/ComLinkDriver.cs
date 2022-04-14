using System.Collections;
using System.IO.Ports;
using CommonLib.Lib.ConfigVO;
using NLog;

namespace CommonLib.Lib.LowerMachine.HardwareDriver;

public class ComLinkDriver
{

    internal LowerConfig lowerConfig;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public ComLinkDriver(LowerConfig lowerConfig)
    {
        this.lowerConfig = lowerConfig;
    }

    public LowerConfig LowerConfig
    {
        get => lowerConfig;
        set => lowerConfig = value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual void init()
    {
        //TODO: implement actual link with lowerconfig 
        logger.Info("ComLink init begin with address:{}",lowerConfig.HardwarePort);
        var comPort = lowerConfig.HardwarePort;
        Thread readThread = new Thread(ReadSerialPort);

        _serialPort = new SerialPort();

         _serialPort.PortName = lowerConfig.HardwarePort;
         _serialPort.BaudRate = lowerConfig.BaudRate;
        _serialPort.ReadTimeout = 500;
        _serialPort.WriteTimeout = 500;
        _serialPort.Open();
        _continue = true;
        
        readThread.Start();
        readThread.Join();
        _serialPort.Close();
    }

    public bool _continue { get; set; }

    public SerialPort _serialPort { get; set; }

    public void send(BitArray cmd)
    {
        //TODO: send cmd to lowermachine
    }

    private void ReadSerialPort()
    {
        while (_continue)
        {
            try
            {
                // Byte[] data ;
                // _serialPort.Read(data, 0, 96);
                invoke(new byte[3]);
            }
            catch (TimeoutException) { }
        }
    }

    public void invoke(byte[] cmd)

    {

        switch (parseHeader(cmd))
        {
            case  HEADER_TYPE.Servo :
                onServoCMD?.Invoke(this,cmd);
                break;
            case HEADER_TYPE.StepMotor:
                onStepMotorrCMD?.Invoke(this,cmd);
                break;
            case HEADER_TYPE.Switch:
                onSwitchCMD?.Invoke(this,cmd);
                break;
            case HEADER_TYPE.Trigger:
                OnTriggerCMD?.Invoke(this,cmd); 
                break;
            case HEADER_TYPE.Emitter:
                onEmitterCMD?.Invoke(this,cmd); 
                break;
            case HEADER_TYPE.MACHINE_ID:
                onMachineID.Invoke(this, parseMachineID(cmd));
                break;
        }
        

        //TODO: when recieve lower machine communication notify other interested party
    }

    internal string machineID;

    public string MachineId => machineID;

    private string parseMachineID(byte[] cmd)
    {
        machineID = cmd.ToString();
        return machineID;
    }

    public HEADER_TYPE parseHeader(byte[] cmd)
    {
        // TODO: implement code to parse header
        return HEADER_TYPE.Servo;
    }
    


    public   event EventHandler<byte[]> OnTriggerCMD;
    public event EventHandler<byte[]> onServoCMD;
    public event EventHandler<byte[]> onStepMotorrCMD;
    public event EventHandler<byte[]> onSwitchCMD;
    public event EventHandler<byte[]> onEmitterCMD;
    
    public  event EventHandler<string> onMachineID;
   
    protected virtual void onMachineIDChanged(string e)
    {
        // Safely raise the event for all subscribers
        onMachineID?.Invoke(this, e);
    }
    
    protected virtual void OnTriggerCMDFired(byte[] e)
    {
        // Safely raise the event for all subscribers
        OnTriggerCMD?.Invoke(this, e);
    }
}
public enum HEADER_TYPE {
    Servo,
    StepMotor,
    Switch,
    Trigger,
    Emitter,
    MACHINE_ID
}

