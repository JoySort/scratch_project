namespace CommonLib.Lib.vo;

/**
 * <summary>分拣标准， 比如可能出现宽度，高度，颜色，每个都有一个名字Name, 一个序号，CriteriaIndex ，最大值和最小值是边界</summary>
 */
public class Criteria
{
    public string Name => name;

    public int Index => index;

    public float Min => min;

    public float Max => max;
    
    public string Code => code;
    /**
     * 区间，这个区间结合最大和最小值可以决定完整区间，比如区间是 最小值8，最大值50，区间给出27,33,41 那么，完整区间就是8-26，27-33，33-40，41-44
     * 注意，如果边界是整数，边界值是起点，终点不能包含边界值。比如上门 27,33,41 都是边界的起点，在结束的时候，不能包含。
     */
    public float[] Boundaries => boundaries;

    private string name;
    private string code;


    private int index;
    private float min;
    private float max;
    private float[] boundaries;
    private bool isChecked;

    public bool IsChecked
    {
        get => isChecked;
        set => isChecked = value;
    }

    public Criteria(string name, string code, int index, float min, float max, float[] boundaries, bool isChecked)
    {
        this.name = name;
        this.code = code;
        this.index = index;
        this.min = min;
        this.max = max;
        this.boundaries = boundaries;
        this.isChecked = isChecked;
    }
}