using System.Collections.Concurrent;
using System.Reflection;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MesageOnionTest.Services;
using MessagePack.Resolvers;


Console.WriteLine(string.Join(",",args));
// Connect to the server using gRPC channel.
//var channel = GrpcChannel.ForAddress("http://localhost:5002");
var channel = GrpcChannel.ForAddress("http://localhost:5002", new GrpcChannelOptions
{
    HttpHandler = new SocketsHttpHandler
    {
        EnableMultipleHttp2Connections = true,
        
    }
});
// NOTE: If your project targets non-.NET Standard 2.1, use `Grpc.Core.Channel` class instead.
// var channel = new Channel("localhost", 5001, new SslCredentials());

// Create a proxy to call the server transparently.
var client = MagicOnionClient.Create<IMyFirstService>(channel);

var clients = new IPictureService[]
{
    MagicOnionClient.Create<IPictureService>(channel),
    MagicOnionClient.Create<IPictureService>(channel),
    MagicOnionClient.Create<IPictureService>(channel),
    MagicOnionClient.Create<IPictureService>(channel)
};

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
foreach (var c in clients)
{
    await c.sendPicture(cpls1);
}



var counter = 0;
long totalTook = 0;
long max = 0;
long min = 0;
int runCountTarget = 99910000;
long resultCounter = 0;

int batchArg = args.Length > 0 ? int.Parse(args[0]) : 1;

int batchCount = batchArg;

Console.WriteLine($"Counter before start:{counter}");
var counterQueue = new ConcurrentQueue<long>();
while(counter<=runCountTarget)
{
    Console.WriteLine($"Counter after start:{counter}");
    //Console.WriteLine($"execution counter {counter}");
    var startIndex = counter * batchCount;
    Console.WriteLine($"startIndex {startIndex}");
    for (var i = startIndex; i < startIndex + batchCount ; i++)
    {
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
            var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var clientIndex = cpls.First().triggerId % clients.Length;
            var result2 = await clients[clientIndex].sendPicture(cpls);
            var took = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime;
            totalTook += took;
            
           
                resultCounter++;
                totalTook += took;
                counterQueue.Enqueue(took);
                if (counterQueue.Count > 20)
                {
                    var temp = 0l;
                    counterQueue.TryDequeue(out temp);
                }

                max = took > max ? took : max;
                min = min == 0 ? took : min;
                min= took < min ? took : min;
                Console.WriteLine(
                    $"client{clientIndex} triggerid {cpls.First().triggerId} took {DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime} counter:{resultCounter}   avg  {counterQueue.Average()} totalTook:{totalTook} max:{max} min:{min} Result: {result2}");

            
            
        });
    }
   
    
    Thread.Sleep(1000/14);
    counter++;
}
while (resultCounter+10 < runCountTarget*batchCount)
{
    Thread.Sleep(1000);
}