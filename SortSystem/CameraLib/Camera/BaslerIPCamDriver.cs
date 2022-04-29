using System.Runtime.InteropServices;
using Basler.Pylon;
using CommonLib.Lib.ConfigVO;

namespace CommonLib.Lib.Camera
{
    public class BaslerIPCamDriver:IPCameraDriver
    {
        private PixelDataConverter converter = new PixelDataConverter();

        private Basler.Pylon.Camera? camera;

        public BaslerIPCamDriver(CameraConfig camConfig) : base(camConfig)
        {
        }

        public override void InitCam()
        {
            //camera = new Basler.Pylon.Camera(new camerai)
            List<ICameraInfo> allCameras = CameraFinder.Enumerate();
            if (allCameras == null)
            {
                logger.Error("No camera found!");
                return;
            }
            logger.Info("Found " + allCameras.Count.ToString() + " cameras");

            ICameraInfo? selectedCameraInfo = null;

            foreach (ICameraInfo ici in allCameras)
            {
                string iciIp = ici.GetValueOrDefault("IpAddress", "invalid ip");
                if (iciIp == this.camConfig.Address)
                {
                    selectedCameraInfo = ici;
                    break;
                }
            }

            if (selectedCameraInfo == null)
            {
                logger.Error("Target camera with address "+ this.camConfig.Address + " was not found!");
                return ;
            }

            camera = new Basler.Pylon.Camera(selectedCameraInfo);


            camera.CameraOpened += Configuration.AcquireContinuous;
            camera.StreamGrabber.ImageGrabbed += OnImageGrabbedAll;

            camera.Open();
            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);

            camera.Parameters[PLCamera.LineDebouncerTimeAbs].SetValue(150);

            camera.Parameters[PLCamera.ChunkModeActive].SetValue(true);

            camera.Parameters[PLCamera.ChunkSelector].SetValue(PLCamera.ChunkSelector.Triggerinputcounter);
            camera.Parameters[PLCamera.ChunkEnable].SetValue(true);
            camera.Parameters[PLCamera.ChunkSelector].SetValue(PLCamera.ChunkSelector.Timestamp);
            camera.Parameters[PLCamera.ChunkEnable].SetValue(true);


            camera.Parameters[PLCamera.CounterSelector].SetValue(PLCamera.CounterSelector.Counter1);
            camera.Parameters[PLCamera.CounterResetSource].SetValue(PLCamera.CounterResetSource.Software);
            camera.Parameters[PLCamera.CounterReset].Execute();

            camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
        }

        public override void CloseCam()
        {
            camera?.Close();
        }
        void OnImageGrabbedAll(Object? sender, ImageGrabbedEventArgs e)
        {
            try
            {
                IGrabResult grabResult = e.GrabResult;
                
                if (!grabResult.IsValid)
                {
                    logger.Error("Camera " + this.camConfig.Address + " invalid grab");
                    return;
                }

                converter.OutputPixelFormat = PixelType.BGR8packed;// PixelType.RGB8packed;
                                                                   // 
                byte[] data = new byte[3*camConfig.Width*camConfig.Height];
                IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data,0);
                converter.Convert(dataPtr, data.Length, grabResult);
                onRecivingPicture(data);

            }
            catch (Exception exception)
            {
                logger.Error(exception);                
            }
        }
    }
}
