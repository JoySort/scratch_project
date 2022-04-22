using CommonLib.Lib.Camera;
using NLog;

namespace CommonLib.Lib.Worker.Recognizer;

public class RecognizerWorker
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private RecognizerWorker()
    {
        
    }

    private static RecognizerWorker me = new RecognizerWorker();

    public static RecognizerWorker getInstance()
    {
        return me;
    }

    private void init()
    {
        
    }

    public void process(CameraPayLoad payload)
    {
        
        return ;
    }
}