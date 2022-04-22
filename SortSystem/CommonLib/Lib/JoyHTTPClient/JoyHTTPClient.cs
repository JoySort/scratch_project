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
            logger.Error("Invalid JSON.");
        }

        return result;
    }

    public async Task<T?> PostToRemote<T>(string uri,Object msg)
    {
        T? result = default;
        try
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(msg)
            };

            var postResponse = await httpClient.PostAsJsonAsync(uri, postRequest);

            postResponse.EnsureSuccessStatusCode();

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
            logger.Error("Invalid JSON.");
        }

        return result;
    }
}