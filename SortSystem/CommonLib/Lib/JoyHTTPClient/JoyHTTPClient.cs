using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using NLog;

namespace CommonLib.Lib.JoyHTTPClient;

public class JoyHTTPClient
{
    private static Logger  logger = LogManager.GetCurrentClassLogger();
    
    public JoyHTTPClient()
    {
        
    }

    private readonly HttpClient httpClient = new HttpClient();
    public  async Task<T?> GetFromRemote<T>(string uri)
    {
        logger.Debug("getFromRemoteAssync {}",uri );
        T? result = default;
        try
        {
           result =  await httpClient.GetFromJsonAsync<T>(uri);
           
          
        }
        catch (HttpRequestException exception) // Non success
        {
            logger.Error("An error occurred while making http get request from {},{}.",uri,exception.ToString());
        }
        catch (NotSupportedException) // When content type is not valid
        {
            logger.Error("The content type is not supported.");
        }
        catch (JsonException exception) // Invalid JSON
        {
            logger.Error("Invalid JSON.{}",exception.Message);
        }

        return result;
    }

    public async Task<T?> PostToRemote<T>(string uri,T msg)
    {
        logger.Debug("PostAsJsonAsync at {}",uri);
        T? result = default;
        try
        {
            
            var postResponse = await httpClient.PostAsJsonAsync<T>(uri, (T)msg);

            postResponse.EnsureSuccessStatusCode();


        }
        catch (HttpRequestException exception) // Non success
        {
            logger.Error("{} An error occurred.{}", uri, exception.Message);
        }
        catch (NotSupportedException) // When content type is not valid
        {
            logger.Error("The content type is not supported.");
        }
        catch (JsonException e) // Invalid JSON
        {
            logger.Error("Invalid JSON. {}", e.Message);
        }
        catch (Exception e)
        {
            logger.Error("uncaught error {}",e.Message);
        }

        return result;
    }
}