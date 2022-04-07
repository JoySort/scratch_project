namespace CommonLib.Lib.vo;

public class Filter
{
    /**
     * <summary>Directly from json, UI pass filterRangeIndex directly</summary>
     */
    public int[] FilterBoundrryIndices => filterBoundrryIndices;
    public Criteria Criteria => _criteria;
    public float[][] FilterBoundaries => filterBoundaries;
    
    private int[] filterBoundrryIndices;
    /**
     * 这里界限设置为2维数组，原因是为了支持潜在的一个分选标准里面选择2个区间，比如 width 0,10,20,30,100  选择0，1区间，则是0，10-10-20 理论上目前还没有这个需要，所以，二维数组的第一个维度只有一个区间是选中的。
     */
    private float[][] filterBoundaries;
    private Criteria _criteria;

    public Filter(int[] boundrryIndices, Criteria criteria)
    {
        filterBoundrryIndices = boundrryIndices;
        _criteria = criteria;
        filterBoundaries = fillBoundries().ToArray();


    }

    private List<float[]> fillBoundries()
    {
        List<float[]> temp_boundaries = new List<float[]>();
        
        for (var i =0 ; i< filterBoundrryIndices.Length;i++)
        {
            if (filterBoundrryIndices[i] == 0)
            {
                float[] boundry = {_criteria.Min, _criteria.Boundaries[0]};
                temp_boundaries.Add(boundry);
            }
            else if(filterBoundrryIndices[i] == _criteria.Boundaries.Length)
            {
                float[] boundry = { _criteria.Boundaries[filterBoundrryIndices[i]-1],_criteria.Max};
                temp_boundaries.Add(boundry);
            }
            else
            {
                if (filterBoundrryIndices[i] > _criteria.Boundaries.Length)
                    throw new Exception("Filter was out of bound 通道过滤规则选择配置超过分选条件:"+_criteria.Name+"总共只有"+_criteria.Boundaries.Length+"个区间，目前选择了"+filterBoundrryIndices[i]);
                float[] boundry = { _criteria.Boundaries[filterBoundrryIndices[i]-1],_criteria.Boundaries[filterBoundrryIndices[i]]};
                temp_boundaries.Add(boundry);
            }
            
        }

        return temp_boundaries;

    }




}

