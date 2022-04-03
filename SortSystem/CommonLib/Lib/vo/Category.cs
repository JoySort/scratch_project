namespace CommonLib.Lib.vo;
/**
 * <summary>子类别，比如椰枣下面有medjoul, fard, lulu  等等</summary>
 */
public class Category
{
    private string name;
    private int catID;

    public Category(string name, int catId)
    {
        this.name = name;
        catID = catId;
    }

    public int CatId => catID;

    public string Name => name;
}