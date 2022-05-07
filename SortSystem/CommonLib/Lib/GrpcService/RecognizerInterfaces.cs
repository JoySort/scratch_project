using CommonLib.Lib.Sort.ResultVO;
using MagicOnion;

namespace CommonLib.Lib.GrpcServiceInterface;

public interface ICameraPayloadTransmissionService : IService<ICameraPayloadTransmissionService>
{
    UnaryResult<long> OnCameraPicture(CameraPayLoad  cameraPayLoads);
}