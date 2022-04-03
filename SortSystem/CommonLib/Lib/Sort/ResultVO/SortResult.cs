using CommonLib.Lib.vo;

namespace CommonLib.Lib.Sort.ResultVO;

public class SortResult
{
    private Coordinate[] coordinates;
    private Feature[] features;
    private Outlet[] outlets;

    public SortResult(Coordinate[] coordinates, Feature[] features, Outlet[] outlets)
    {
        this.coordinates = coordinates;
        this.features = features;
        this.outlets = outlets;
    }

    public Coordinate[] Coordinates => coordinates;

    public Feature[] Features => features;

    public Outlet[] Outlets => outlets;
}