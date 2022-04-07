namespace CommonLib.Lib.ConfigVO;

public class Configuration
{

    private string name;
    private JoyModule module;
    private int[] column;

    public Configuration(string name, JoyModule module, int[] column)
    {
        this.name = name;
        this.module = module;
        this.column = column;
    }

    public string Name
    {
        get => name;
        set => name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public JoyModule Module
    {
        get => module;
        set => module = value;
    }

    public int[] Column
    {
        get => column;
        set => column = value ?? throw new ArgumentNullException(nameof(value));
    }
}

public enum JoyModule
{
    Camera,
    Lower,
    Upper,
    UI
}
