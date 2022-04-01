using System;
using Newtonsoft.Json;
using NUnit.Framework;
using CommonLib.lib.sort;
namespace TestSortingSystem;

public class Tests
{
    Condition cd1 = new Condition();
    Condition cd2 = new Condition();

    private Condition? cd3 = null;
    private string jsonString; 
    [SetUp]
    public void Setup()
    {
        
        cd1.AddSubCondition(3,new int[] {1,2,3,4,5});
        cd1.AddSubCondition(2,new int[] {1,2});


       
        cd2.AddSubCondition(1, new int[] { 1, 2 });
        cd2.AddSubCondition(3, new int[] { 6, 2, 3, 4, 5 });
        
        
        jsonString = JsonConvert.SerializeObject(cd1, Formatting.Indented);
        //Console.WriteLine(s);
        cd3 = Condition.GetConditionFromJson(jsonString);
    }

    [Test]
    public void TestCompareFunction()
    
    {
        Console.WriteLine(jsonString);
        Assert.NotZero(cd1.CompareTo(cd2));
    }

    [Test]
    public void TestSerilizationAndDeSerialization()
    {
        Assert.Zero(cd1.CompareTo(cd3));
    }
}