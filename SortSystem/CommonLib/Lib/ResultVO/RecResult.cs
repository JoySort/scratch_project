using System.Collections;

namespace CommonLib.Lib.Sort.ResultVO;

public class RecResult:RawResult
{
 
    private Feature[] features;
    public Feature[] Features => features;



    public RecResult(Coordinate coordinate, int expectedFeatureCount, Feature[] features) : base(coordinate, expectedFeatureCount)
    {
        this.features = features;
    }
    
}