using System.Reflection;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MesageOnionTest.Services;
using MessagePack.Resolvers;


// Connect to the server using gRPC channel.
var channel = GrpcChannel.ForAddress("http://localhost:5002");

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

var counter = 1;
long totalTook = 0;
long max = 0;
long min = 100000;
int runCountTarget = 100;
long resultCounter = 0;
int batchCount = 1;
while(counter++<=runCountTarget)
{
    //Console.WriteLine($"execution counter {counter}");
   
    for (var i = counter * batchCount; i < counter * batchCount+ batchCount; i++)
    {
        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var cpls = new List<CameraPayLoad>();
        var data = new CameraPayLoad
        {
            pictureData = picture,
            triggerId = i,
            column = i,
            Coordinate = new Coordinate(i)
        };

        cpls.Add(data);
        Task.Run(async () =>
        {
            Console.WriteLine($"sending     triggerId {data.triggerId}");
            var result2 = await client2.sendPicture(cpls);
            var took = DateTimeOffset.Now.ToUnixTimeMilliseconds() - start;
            totalTook += took;
            max = took > max ? took : max;
            min = took < min ? took : min;
            resultCounter++;
            Console.WriteLine(
                             $"Result:     triggerid {cpls.First().triggerId} took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - start} avg timeTook {totalTook / counter} max:{max} min:{min}");
        });
    }
    Thread.Sleep(1000/14);
}
while (resultCounter < runCountTarget*batchCount)
{
    Thread.Sleep(1000);
}