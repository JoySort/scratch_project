using CommonLib.Lib.Sort.ResultVO;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server;

namespace CommonLib.Lib.GrpcServiceInterface;

public class GrpcTransmissionClient
{
    public GrpcTransmissionClient(string host, int port)
    {
         channel = GrpcChannel.ForAddress($"http://{host}:{port}", new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,

            }
        });
    }

    private GrpcChannel channel;

    public async Task<long> sendCameraData(CameraPayLoad cameraPayLoad)
    {
        var client = MagicOnionClient.Create<ICameraPayloadTransmissionService>(channel);
        var result = await client.OnCameraPicture(cameraPayLoad);
        return result;
    }
}

