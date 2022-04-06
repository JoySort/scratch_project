using System.IO;
using System.Reflection;
using CommonLib.Lib.Sort.Util;



namespace LibUnitTest.vo;
using NUnit.Framework;

public class JsonParserTest {
    private const string JsonFilePath = @"./assets/vo/project_start_sample.json";
    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);

    private string? porjectJsonString;
    private JsonParser? jparser;


   
    [SetUp]
    public void Setup()
    {   
        porjectJsonString = File.ReadAllText(path);
        jparser = new JsonParser(porjectJsonString);

    }

    [Test]
    public void CheckCriteria()
    {
        
        
        
        var fullCriteria = jparser?.FullCriteria;
        var enabledCriteria = jparser?.EnabledCriteria;
        var fullCriteriaLength = 11;
        var enabledCriteriaLength = 4;
        Assert.AreEqual(fullCriteriaLength,fullCriteria?.Count);
        Assert.AreEqual(enabledCriteriaLength,enabledCriteria?.Count);
        
        
    }

    [Test]
    public void CheckOutlet()
    {
        var outlets = jparser?.Outlets;
        var outletNO = 8;
        Assert.AreEqual(outlets?.Count,outletNO);
    }
}