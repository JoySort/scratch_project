using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using NLog;

namespace CommonLib.Lib.JoyHTTPClient;

public class HTTPClientWorker
{
    private static Logger  logger = LogManager.GetCurrentClassLogger();

    private static HTTPClientWorker instance = new HTTPClientWorker();

    public static HTTPClientWorker getInstance()
    {
        return instance;
    }

    public HTTPClientWorker()
    {
        
    }

    private readonly HttpClient httpClient = new HttpClient();
    public  async Task<T?> GetFromRemote<T>(string uri)
    {
        T? result = default;
        try
        {
           result =  await httpClient.GetFromJsonAsync<T>(uri);
           logger.Info(result);
        }
        catch (HttpRequestException) // Non success
        {
            logger.Error("An error occurred.");
        }
        catch (NotSupportedException) // When content type is not valid
        {
            logger.Error("The content type is not supported.");
        }
        catch (JsonException) // Invalid JSON
        {
            Console.WriteLine("Invalid JSON.");
        }

        return result;
    }
}