
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommonLib.Lib.Sort.Util;
using CommonLib.Lib.vo;


namespace LibUnitTest.vo;
using NUnit.Framework;

public class JSONParserTest
{
    private const string jsonFilePath = @"./assets/vo/project_start_sample.json";
    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),jsonFilePath);

    private string porject_json_string;
    private JSONParser jparser;


   
    [SetUp]
    public void Setup()
    {   
        porject_json_string = File.ReadAllText(path);
        jparser = new JSONParser(porject_json_string);

    }

    [Test]
    public void createObjFromJson()
    {
        

        
        var fullCriteria = jparser.FullCriteria;
        var enabledCriteria = jparser.EnabledCriteria;
        var fullCriteriaLength = 11;
        var enabledCriteriaLength = 4;
        Assert.AreEqual(fullCriteriaLength,fullCriteria.Count);
        Assert.AreEqual(enabledCriteriaLength,enabledCriteria.Count);

    }
}