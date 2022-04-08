namespace CommonLib.Lib.ConfigVO;

public class MachineState
{
    private StateName name;
    private bool isDefault;
    private State state;

    public MachineState(StateName name, bool isDefault, State state)
    {
        this.name = name;
        this.isDefault = isDefault;
        this.state = state;
    }

    public StateName Name => name;

    public bool IsDefault => isDefault;

    public State State => state;
}

public class State
{
    private Trigger[] triggers;
    private Servo[] servos;
    private StepMotoer[] stepMotoers;
    private Switch[] switches;

    public Trigger[] Triggers => triggers;

    public Servo[] Servos => servos;

    public StepMotoer[] StepMotoers => stepMotoers;

    public Switch[] Switches => switches;

    public State(Trigger[] triggers, Servo[] servos, StepMotoer[] stepMotoers, Switch[] switches)
    {
        this.triggers = triggers;
        this.servos = servos;
        this.stepMotoers = stepMotoers;
        this.switches = switches;
    }
}

public enum StateName
{
    start,
    stop,
    pause,
    washing,
    reverse
}