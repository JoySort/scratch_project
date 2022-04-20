using System.IO;
using System.Linq;
using System.Reflection;
using CommonLib.Lib.Util;
using CommonLib.Lib.vo;


namespace LibUnitTest.Parser;
using NUnit.Framework;

public class JsonParserTestV1 {
    private const string JsonFilePath = @"./fixtures/project_start_v1.json";
    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,JsonFilePath);

    private string? porjectJsonString;
    private ProjectParser? jparser;

    private Project project;
   
    [SetUp]
    public void Setup()
    {   
        porjectJsonString = File.ReadAllText(path);
        jparser = new ProjectParser(porjectJsonString,ProjectParser.V1);
        project = jparser.getProject();
    }

    [Test]
    public void CheckProject()
    {
         Assert.AreEqual(project.Id,"5_T75D61G");
         Assert.AreEqual(project.Name,"Final TestRun Khalas(VIP,Retail,Bulk) with Peeling and defect detection dryness");
         Assert.AreEqual(project.Genre.Name,GenreName.palmDate);
         Assert.AreEqual(project.Category.CatId,1);
    }

    [Test]
    public void CheckCriteria()
    {
        
        
        
         var fullCriteria = jparser?.FullCriteria;
         var enabledCriteria = jparser?.EnabledCriteria;
         var fullCriteriaLength = 4;
         var enabledCriteriaLength = 4;
         Assert.AreEqual(fullCriteriaLength,fullCriteria?.Count);
         Assert.AreEqual(enabledCriteriaLength,enabledCriteria?.Count);
        //
        
    }

    [Test]
    public void CheckOutlet()
    {
         var outlets = jparser?.Outlets;
         var outletNO = 8;
         Assert.AreEqual(outlets?.Count,outletNO);

         var expectedFilters = new string[][] {new string[]{"dd","wt"},
                                               new string[]{"color"},
                                               new string[]{"dd","wt","sf","color"},
                                               new string[]{"dd","wt","sf","color"},
                                               new string[]{"dd","wt","sf","color"},
                                               new string[]{"dd","wt","sf","color"},
                                               new string[]{"dd","wt","sf","color"},
                                               new string[]{"dd","wt","sf","color"}
         };

         foreach (var outlet in outlets)
         {
           Assert.True(outlet.Filters.First().Select((filter=>filter.Criteria.Code)).ToArray().OrderBy(value=>value).SequenceEqual(expectedFilters[int.Parse(outlet.ChannelNo)-1].OrderBy(value=>value)));  
         }
    }
}