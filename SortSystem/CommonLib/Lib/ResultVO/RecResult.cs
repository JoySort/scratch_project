using System.Collections;

namespace CommonLib.Lib.Sort.ResultVO;

public class RecResult:RawResult
{
 
    private Feature[] features;
    
    

    public Feature[] Features => features;


    public RecResult(Coordinate[] coordinates, int offset, Feature[] features) : base(coordinates, offset)
    {
        this.features = features;
    }
}