using System.Reflection;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MesageOnionTest.Services;
using MessagePack.Resolvers;


// Connect to the server using gRPC channel.
var channel = GrpcChannel.ForAddress("http://10.10.40.152:5002");

// NOTE: If your project targets non-.NET Standard 2.1, use `Grpc.Core.Channel` class instead.
// var channel = new Channel("localhost", 5001, new SslCredentials());

// Create a proxy to call the server transparently.
var client = MagicOnionClient.Create<IMyFirstService>(channel);

// Call the server-side method using the proxy.
var result = await client.SumAsync(123, 456);

var path = Path.Combine(
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
    "assets/" + 1 + ".bmp");
byte[] picture = File.ReadAllBytes(path);
Console.WriteLine($"picture length {picture.Length}");



var client2 = MagicOnionClient.Create<IPictureService>(channel);
var cpls1 = new List<CameraPayLoad>();
var data1 = new CameraPayLoad
{
    pictureData = picture,
    triggerId = -1,
    column = -1,
    Coordinate = new Coordinate(-1)
};
cpls1.Add(data1);
await client2.sendPicture(cpls1);

var counter = 1;
long totalTook = 0;
long max = 0;
long min = 0;
int runCountTarget = 99910000;
long resultCounter = 0;
int batchCount = 4;
while(counter++<=runCountTarget)
{
    //Console.WriteLine($"execution counter {counter}");
    var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    var cpls = new List<CameraPayLoad>();
    for (var i = counter * batchCount; i < counter * batchCount+ batchCount; i++)
    {
       
        var data = new CameraPayLoad
        {
            pictureData = picture,
            triggerId = i,
            column = i,
            Coordinate = new Coordinate(i)
        };
        cpls.Add(data);
        
    }
   
    Task.Run(async () =>
    {
        //Console.WriteLine($"sending     triggerId {cpls.First().triggerId}");
        var result2 = await client2.sendPicture(cpls);
        var took = DateTimeOffset.Now.ToUnixTimeMilliseconds() - start;
        totalTook += took;
        if(cpls.First().triggerId>0){ 
            totalTook += took;
            max = took > max ? took : max;
            min = min == 0 ? took : min;
            min= took < min ? took : min;
        }
        resultCounter++;
        Console.WriteLine(
            $"             triggerid {cpls.First().triggerId} took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start} counter:{counter} avg  {totalTook / counter} max:{max} min:{min} Result: {result2}");
    });
    Thread.Sleep(1000/14);
}
while (resultCounter+10 < runCountTarget*batchCount)
{
    Thread.Sleep(1000);
}