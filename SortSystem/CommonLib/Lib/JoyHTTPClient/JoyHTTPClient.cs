using System.Globalization;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using CommonLib.Lib.Sort.ResultVO;
using Newtonsoft.Json;
using NLog;
using JsonException = System.Text.Json.JsonException;

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
        //logger.Debug("getFromRemoteAssync {}",uri );
        T? result = default;
        try
        {
            result = await httpClient.GetFromJsonAsync<T>(uri);


        }
        catch (HttpRequestException exception) // Non success
        {
            logger.Error("An error occurred while making http get request from {},{}.", uri, exception.ToString());
        }
        catch (NotSupportedException) // When content type is not valid
        {
            logger.Error("The content type is not supported.");
        }
        catch (JsonException exception) // Invalid JSON
        {
            logger.Error("Invalid JSON.{}", exception.Message);
        }
        catch (Exception e)
        {
            logger.Error("httpClient.GetFromJsonAsync exception {}", e.Message);
        }

        return result;
    }

    public async Task<T?> PostToRemote<T>(string uri,T msg)
    {
        //logger.Debug("PostAsJsonAsync at {}",uri);
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
    
    public  async Task<string> Upload(string uri,List<CameraPayLoad> cpls)
    {
        using (var client =httpClient)
        {
            var imageList = new Dictionary<string,byte[]>();
           
            foreach (var cpl in cpls)
            {
                string imageName = $"{cpl.TriggerId}-{cpl.CamConfig.Columns[0]}-{cpl.CamConfig.Columns[1]}-{cpl.CamConfig.CameraPosition}";
                imageList.Add(imageName,cpl.PictureData);
                cpl.PictureData = null;
            }

            var cameraPayloadJson = JsonConvert.SerializeObject(cpls);
            using (
                var content =
                   new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                foreach (var (key, value) in imageList)
                {
                    content.Add(new StreamContent(new MemoryStream(value)), key, key);
                }
                
                content.Add(new StringContent(cameraPayloadJson), "cameraPayload");
                //logger.Debug($"Upload url {uri} content size{content.Headers.Count()}");
                using (
                    var message =
                    await client.PostAsync(uri, content))
                {
                    var input = await message.Content.ReadAsStringAsync();

                    return !string.IsNullOrWhiteSpace(input) ? Regex.Match(input, @"http://\w*\.directupload\.net/images/\d*/\w*\.[a-z]{3}").Value : null;
                }
            }
        }
    }
}