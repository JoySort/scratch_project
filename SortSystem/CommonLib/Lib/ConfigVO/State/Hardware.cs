namespace CommonLib.Lib.ConfigVO;
public interface IHardwareconfig
{
        
}
public class HWBase:IHardwareconfig
{
    
    public HWBase(string name, int address, bool enabled, int delay)
    {
        this.name = name;
        this.address = address;
        this.enabled = enabled;
        this.delay = delay;
    }

    public string Name => name;

    public int Address => address;

    public bool Enabled => enabled;

    public int Delay => delay;

    private string name;
    private int address;
    private bool enabled;
    private int delay;

}
public class StepMotoer : HWBase
{
    public int Direction => direction;

    public int Speed => speed;

    public StepMotoer(string name, int address, bool enabled, int delay, int direction, int speed) : base(name, address, enabled, delay)
    {
        this.direction = direction;
        this.speed = speed;
    }

    private int direction;
    private int speed;
    
    
}
public class Servo:StepMotoer
{
    public Servo(string name, int address, bool enabled, int delay, int direction, int speed, int startTimeSpan) : base(name, address, enabled, delay, direction, speed)
    {
        this.startTimeSpan = startTimeSpan;
    }

    public int StartTimeSpan => startTimeSpan;

    private int startTimeSpan;
    
}

public class Switch : HWBase
{
    public Switch(string name, int address, bool enabled, int delay, string mode, string interval, int repeat) : base(name, address, enabled, delay)
    {
        this.mode = mode;
        this.interval = interval;
        this.repeat = repeat;
    }

    public string Mode => mode;

    public string Interval => interval;

    public int Repeat => repeat;

    private string mode;
    private string interval;
    private int repeat;     

}

public class Trigger : HWBase
{
    public Trigger(string name, int address, bool enabled, int delay, TriggerMode mode, int interval, int step) : base(name, address, enabled, delay)
    {
        this.mode = mode;
        this.interval = interval;
        this.step = step;
    }

    public TriggerMode Mode => mode;

    public int Interval => interval;

    public int Step => step;

    private TriggerMode mode;
    private int interval;
    private int step;
}

public enum TriggerMode
{
    Soft,
    PhotoelectricSwitch,
    Encoder
}