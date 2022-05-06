using System.Reflection;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;

namespace MesageOnionTest.Services;

public interface IMyFirstService : IService<IMyFirstService>
{
    // The return type must be `UnaryResult<T>`.
    UnaryResult<int> SumAsync(int x, int y);
}

public interface IPictureService : IService<IPictureService>
{
    // The return type must be `UnaryResult<T>`.
    UnaryResult<int> sendPicture(List<CameraPayLoad> list);
}
[MessagePackObject(keyAsPropertyName: true)]
public struct CameraPayLoad
{
    
    public int  column;
    public byte[] pictureData;
    public int triggerId;
    public Coordinate Coordinate;
}
[MessagePackObject(keyAsPropertyName: true)]
public struct Coordinate
{
    public int column;

    public Coordinate(int column)
    {
        this.column = column;
    }
}
public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
{
    // `UnaryResult<T>` allows the method to be treated as `async` method.
    public async UnaryResult<int> SumAsync(int x, int y)
    {
        Console.WriteLine($"Received:{x}, {y}");
        return x + y;
    }
}
public class MyPictureService :ServiceBase<IPictureService>,IPictureService
{
   // public static 
    public async UnaryResult<int> sendPicture(List<CameraPayLoad> list)
    {
        // var path = Path.Combine(
        //     Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
        //     cameraPayLoad.CamConfig.SavePath)+"/"+timestamp;
        // bool dirExists = Directory.Exists(path);
        // if (!dirExists)
        // {
        //     DirectoryInfo di = Directory.CreateDirectory(path);
        // }
        // //triggerID补足7位，比如1是0000001 避免排序的时候出问题。
        // var triggerID = cameraPayLoad.TriggerId.ToString("D7");
        //                 
        // var filename = triggerID+"-"+timestamp+".bmp";
        // File.WriteAllBytes(path+"/"+filename, cameraPayLoad.PictureData);
        return list.Count;
    }
}