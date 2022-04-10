using CommonLib.Lib.LowerMachine;

namespace CommonLib.Lib.ConfigVO;

public class MachineState
{
    private ProjectState stateName;
    private bool isDefault;
    private State state;

    public MachineState(ProjectState stateName, bool isDefault, State state)
    {
        this.stateName = stateName;
        this.isDefault = isDefault;
        this.state = state;
    }

    public ProjectState StateName => stateName;

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

