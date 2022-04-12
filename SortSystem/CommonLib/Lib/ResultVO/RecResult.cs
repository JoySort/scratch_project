using System.Collections;

namespace CommonLib.Lib.Sort.ResultVO;

public class RecResult:RawResult
{
 
    private List<Feature> features;

    public List<Feature> Features
    {
        get => features;
        set => features = value ?? throw new ArgumentNullException(nameof(value));
    }

    public RecResult(Coordinate coordinate, int expectedFeatureCount, List<Feature> features) : base(coordinate, expectedFeatureCount)
    {
        this.features = features;
    }
    
}