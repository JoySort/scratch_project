using CommonLib.Lib.Sort.ResultVO;

namespace CommonLib.Lib.vo;

public class Filter
{
    /**
     * <summary>Directly from json, UI pass filterRangeIndex directly</summary>
     */
    public int[] FilterBoundryIndices => _filterBoundryIndices;
    public Criteria Criteria => _criteria;
    public float[][] FilterBoundaries => filterBoundaries;
    
    private int[] _filterBoundryIndices;
    /**
     * 这里界限设置为2维数组，原因是为了支持潜在的一个分选标准里面选择2个区间，比如 width 0,10,20,30,100  选择0，1区间，则是0，10-10-20 理论上目前还没有这个需要，所以，二维数组的第一个维度只有一个区间是选中的。
     */
    private float[][] filterBoundaries;
    private Criteria _criteria;

    // public Filter(int[] boundryIndices, Criteria criteria)
    // {
    //     _filterBoundryIndices = boundryIndices;
    //     _criteria = criteria;
    //    // filterBoundaries = fillBoundries().ToArray();
    //
    //
    // }

    public Filter(int[] filterBoundryIndices,Criteria criteria ,float[][] filterBoundaries)
    {
        this._filterBoundryIndices = filterBoundryIndices;
        this.filterBoundaries = filterBoundaries;
        _criteria = criteria;
    }

    public bool doFilter(RecResult recResult)
    {
        bool concerned = false;
        foreach (var feature in recResult.Features)
        {
            if (_criteria.Index != feature.CriteriaIndex) continue;
            concerned = true;

            foreach (var boundary in filterBoundaries)
            {
                if (boundary.First() <= feature.Value && boundary.Last() > feature.Value) return true;
            }
        }

        if (concerned)
            return false;
        else
            return true;
    }

    public static List<float[]> fillBoundries(Criteria _criteria,int[] _filterBoundryIndices)
    {
        List<float[]> temp_boundaries = new List<float[]>();
        
        for (var i =0 ; i< _filterBoundryIndices.Length;i++)
        {
            if (_filterBoundryIndices[i] == 0)
            {
                float[] boundry = {_criteria.Min, _criteria.Boundaries[0]};
                temp_boundaries.Add(boundry);
            }
            else if(_filterBoundryIndices[i] == _criteria.Boundaries.Length)
            {
                float[] boundry = { _criteria.Boundaries[_filterBoundryIndices[i]-1],_criteria.Max};
                temp_boundaries.Add(boundry);
            }
            else
            {
                if (_filterBoundryIndices[i] > _criteria.Boundaries.Length)
                    throw new Exception("Filter was out of bound 通道过滤规则选择配置超过分选条件:"+_criteria.Name+"总共只有"+_criteria.Boundaries.Length+"个区间，目前选择了"+_filterBoundryIndices[i]);
                float[] boundry = { _criteria.Boundaries[_filterBoundryIndices[i]-1],_criteria.Boundaries[_filterBoundryIndices[i]]};
                temp_boundaries.Add(boundry);
            }
            
        }

        return temp_boundaries;

    }




}

