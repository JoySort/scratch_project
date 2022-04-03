using System.Collections;

namespace CommonLib.Lib.Sort.ResultVO;

public class RecResult
{
    private Coordinate coordinate;
    private Feature[] features;
    
    
    public Coordinate Coordinate => coordinate;

    public Feature[] Features => features;
    
    
    //TODO: write bit transform code 
    public RecResult(Coordinate coordinate, BitArray featureBits)
    {
        this.coordinate = coordinate;

    }

}