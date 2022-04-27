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

    public RecResult(Coordinate coordinate, int expectedFeatureCount, long createdTimestamp, List<Feature> features) : base(coordinate, expectedFeatureCount, createdTimestamp)
    {
        this.features = features;
    }

    public string toLog()
    {
        string result = "  F:";
        foreach (var feature in features)
        {
            result += feature.CriteriaIndex + ":" + feature.Value +" " ;
        }

        return Coordinate.Key() + result;
    }



}