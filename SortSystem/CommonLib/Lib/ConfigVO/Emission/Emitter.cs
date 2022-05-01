namespace CameraLib.Lib.ConfigVO.Emission;

public class Emitter
{
    private int[][] delay;
    private int[][] duration;
    private int[] offset;

    public Emitter(int[][] delay, int[][] duration, int[] offset)
    {
        this.delay = delay;
        this.duration = duration;
        this.offset = offset;
    }

    public int[][] Delay => delay;

    public int[][] Duration => duration;

    public int[] Offset => offset;
}