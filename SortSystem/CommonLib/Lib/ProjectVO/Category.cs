namespace CameraLib.Lib.vo;
/**
 * <summary>子类别，比如椰枣下面有medjoul, fard, lulu  等等</summary>
 */
public class Category
{
    private string name;
    private int catID;

    public Category(string name)
    {
        this.name = name;
        this.catID = cateogryCode(name);
    }
    private static int cateogryCode(string name)
    {
        string[] categorys = new[] {"khalas", "medjool", "suqey", "fardh", "sayer", "lulu", "barni", "ajwa", "safawi"};
        for (var i = 0; i < categorys.Length; i++)
        {
            if (categorys[i].Equals(name))
            {
                return i + 1;
            }
        }
        return 0;
    }
    public int CatId => catID;

    public string Name => name;
}
