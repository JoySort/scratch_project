using CameraLib.Lib.vo;

namespace CameraLib.Lib.Sort.ResultVO;

public class Feature
{
    private int criteriaIndex;
    private float value;

    public Feature(int criteriaIndex, float value)
    {
        this.criteriaIndex = criteriaIndex;
        this.value = value;
    }

    public int CriteriaIndex
    {
        get => criteriaIndex;
        set => criteriaIndex = value;
    }

    public float Value
    {
        get => value;
        set => this.value = value;
    }
}