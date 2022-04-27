using System.Collections;
using System.IO.Ports;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.Util;
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
        _serialPort.DataBits = 8;
        _serialPort.StopBits = StopBits.One;
        _serialPort.Parity = Parity.None;
        _serialPort.ReadTimeout = 200;
        _serialPort.WriteTimeout = -1;
        _serialPort.DataReceived += serialPort_DataReceived;
        _serialPort.Open();
        
        _continue = true;

        Thread writeThread = new Thread(Run);
        writeThread.Start();
        writeThread.Join();
        //readThread.Start();
        //readThread.Join();
        _serialPort.Close();
    }
    
    public virtual void Close()
    {
        _serialPort.Close();
    }

    private void HandleData()
    {
        if (_serialPort.BytesToRead > 0)
        {
            Byte[] receivceData = new Byte[_serialPort.BytesToRead];
            int nLen = _serialPort.Read(receivceData, 0, receivceData.Length);

            listDatBuffer.AddRange(receivceData);
            int nMinProto = 7;
            while (listDatBuffer.Count >= nMinProto)
            {
                if (listDatBuffer.Count > 40)
                {
                    listDatBuffer.Clear();
                    break;
                }

                if (listDatBuffer[0] != 0x01)
                {
                    listDatBuffer.RemoveAt(0);
                    continue;
                }
                if (!(listDatBuffer[1] == 0x03 || listDatBuffer[1] == 0x06 || listDatBuffer[1] == 0x10))
                {
                    listDatBuffer.RemoveAt(0);
                    continue;
                }


                if (listDatBuffer[0] == 0x01 && listDatBuffer[1] == 0x03)
                {
                    int dataLenRead = listDatBuffer[2];
                    int pkgLength = 3 + 2 + dataLenRead;
                    if (listDatBuffer.Count < pkgLength)
                        break;

                    if (listDatBuffer[2] == 4)
                    {
                        invoke(listDatBuffer.ToArray(), HEADER_TYPE.Trigger);
                    }

                    if (listDatBuffer[2] == 12)
                    {
                        invoke(listDatBuffer.ToArray(),HEADER_TYPE.MACHINE_ID);                       
                    }

                    listDatBuffer.RemoveRange(0, pkgLength);
                    continue;
                }

                if (listDatBuffer[0] == 0x01 && listDatBuffer[1] == 0x10)
                {
                    if (listDatBuffer.Count < 8)
                        break;
                    listDatBuffer.RemoveRange(0, 8);
                    continue;
                }

                if (listDatBuffer[0] == 0x01 && listDatBuffer[1] == 0x06)
                {
                    if (listDatBuffer.Count < 8)
                        break;

                    if (listDatBuffer[2] == 0x00 && listDatBuffer[3] == 0x50)
                    {
                        invoke(listDatBuffer.ToArray(), HEADER_TYPE.Servo);
                    }
                    if (listDatBuffer[2] == 0x00 && listDatBuffer[3] == 0x94)
                    {
                        invoke(listDatBuffer.ToArray(), HEADER_TYPE.Servo);
                    }
                    listDatBuffer.RemoveRange(0, 8);
                    continue;
                }
            }
        }
    }

    private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        HandleData();
    }


    public bool _continue { get; set; }

    public SerialPort _serialPort { get; set; }

    //modbus协议单个寄存器是16bit，也就是一个short

    //写单个寄存器 ，一次发送8个字节
    //0x01 0x06 + 2个字节地址+两字节数据（也就是一个short）+ 2个字节的crc
    public void writeSingleReg(byte[] addr, byte[] data,int delay=0,int wait=0)//data length =2
    {
        int len = 8;
        byte[] buff = new byte[len];

        buff[0] = 0x01;
        buff[1] = 0x06;
        buff[2] = addr[0];  // 高位
        buff[3] = addr[1];  // 低位

        buff[4] = data[0];  // 高位
        buff[5] = data[1];  // 低位

        byte[] crc = CRCHelper.CRC16Calc(buff, 0, len - 2);

        buff[6] = crc[0];
        buff[7] = crc[1];

        post(new ComLinkMsg(buff, delay, wait));
    }

    //写多个寄存器 ，一次发送（9+数据byte长度）个字节
    //0x01 0x10 + 2个字节地址+两字节长度（short数据的个数）+ 单字节数据长度（byte个数）+ 数据 + 2个字节的crc

    public void writeMultipleRegs(byte[] addr, byte[] data,int delay=0,int wait=0)
    {
        int len = 9+data.Length;
        byte[] buff = new byte[len];

        buff[0] = 0x01;
        buff[1] = 0x06;
        buff[2] = addr[0];  // 高位
        buff[3] = addr[1];  // 低位

        buff[4] = (byte)(data.Length / 2 / 256);// 高位
        buff[5] = (byte)(data.Length / 2 % 256);// 低位
        buff[6] = (byte)(data.Length); 

        Array.Copy(data,0,buff,7,data.Length);

        byte[] crc = CRCHelper.CRC16Calc(buff, 0, len - 2);

        buff[6] = crc[0];
        buff[7] = crc[1];

        post(new ComLinkMsg(buff, delay, wait));

    }

    //读单个或多个寄存器，一次发送8个字节
    //0x01 0x03 + 2个字节地址+两字节长度（short数据的个数）+ 2个字节的crc
    public void readReg(byte[] addr, ushort count,int delay=0, int wait=0)
    {
        int len = 8;
        byte[] buff = new byte[len];

        buff[0] = 0x01;
        buff[1] = 0x06;
        buff[2] = addr[0];  // 高位
        buff[3] = addr[1];  // 低位

        buff[4] = (byte)(count/256);  // 高位
        buff[5] = (byte)(count%256); ;  // 低位

        byte[] crc = CRCHelper.CRC16Calc(buff, 0, len - 2);

        buff[6] = crc[0];
        buff[7] = crc[1];

        post(new ComLinkMsg(buff, delay, wait));
    }

    private List<ComLinkMsg> comLinkMsgs = new List<ComLinkMsg>();
    private void post(ComLinkMsg comLinkMsg)
    {
        lock (comLinkMsgs)
        {
            comLinkMsgs.Append(comLinkMsg);
        }
    }

    

    private void send(byte[] data)
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Write(data, 0, data.Length);
        }
    }

    private void ReadSerialPort()
    {
        while (_continue)
        {
            try
            {
                HandleData();
            }
            catch (TimeoutException) { }
            Thread.Sleep(2);
        }
    }

    public void ClearMsgQueue()
    { 
        lock(comLinkMsgs)
        {
            comLinkMsgs.Clear();
        }
    }

    private void Run()
    {
        while (_continue)
        {
            try
            {               
                ComLinkMsg? comLinkMsg=null;
                lock (comLinkMsgs)
                {
                    if (comLinkMsgs.Count > 0)
                    {
                        comLinkMsg = comLinkMsgs[0];
                        comLinkMsgs.RemoveAt(0);
                    }
                }
                if (comLinkMsg != null)
                {
                    int delay = comLinkMsg.Delay;
                    int wait = comLinkMsg.Wait;
                    byte[] data = comLinkMsg.Data;

                    if (delay > 0)
                        Thread.Sleep(delay);

                    send(data);

                    if (wait > 0)
                        Thread.Sleep(wait);
                }
            }
            catch (TimeoutException) { 
            }
            Thread.Sleep(2);
        }
    }

    public void invoke(byte[] cmd, HEADER_TYPE hType)
    {
        switch (hType)
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
    private static List<byte> listDatBuffer = new List<byte>(); // 串口数据缓存

    public string MachineId => machineID;

    private string parseMachineID(byte[] cmd)
    {
        machineID = cmd.ToString();
        return machineID;
    }

    
    public  event EventHandler<byte[]> OnTriggerCMD;
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
    Invalid,
    Unknown,
    Servo,
    StepMotor,
    Switch,
    Trigger,
    Emitter,
    MACHINE_ID
}

public class ComLinkMsg
{
    byte[] _data;
    int _delay;
    int _wait;


    public ComLinkMsg(byte[] data, int delay, int wait)
    { 
        _data = data;
        _delay = delay;
        _wait = wait;

    }
    public byte[] Data => _data;
    public int Delay => _delay;
    public int Wait => _wait;
}

