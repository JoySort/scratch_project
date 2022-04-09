using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LibUnitTest.network;

public class HTTPClientTest
{
    private HttpClient client = new HttpClient();
    //[SetUp]
    public async Task  setup()
    {
        var url = "http://code-server.lan:5133/Discover";
        Console.WriteLine("started");
        //var response = await client.GetAsync(url);
        //response.EnsureSuccessStatusCode();
        //string responseBody = await response.Content.ReadAsStringAsync();
        // Above three lines can be replaced with new helper method below
        string responseBody = await client.GetStringAsync(url);

        Console.WriteLine(responseBody);
    }

    //[Test]
    public void test1()
    {
     
    }
}