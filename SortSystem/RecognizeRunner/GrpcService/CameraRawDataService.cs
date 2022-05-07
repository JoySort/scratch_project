using CommonLib.Lib.GrpcServiceInterface;
using CommonLib.Lib.Sort.ResultVO;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using RecognizerLib.Lib.Worker;

namespace RecognizeRunner.GrpcService;


public class CameraPayloadTransmissionService :ServiceBase<ICameraPayloadTransmissionService>,ICameraPayloadTransmissionService
{
    public async UnaryResult<long> OnCameraPicture(CameraPayLoad cameraPayLoad)
    {
        
        RawDataBridge.wrapCameraData(this,cameraPayLoad);
        return cameraPayLoad.TriggerId;
    }
}