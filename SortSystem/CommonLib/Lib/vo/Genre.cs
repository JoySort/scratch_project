namespace CommonLib.Lib.vo;

/**
 * <summary>类别，比如枣，苹果，包装袋等等</summary>
 */
public class Genre
{
    private string name;
    private Category category;

    public Genre(string name, Category category)
    {
        this.name = name;
        this.category = category;
    }

    public string Name => name;

    /**
     * <summary>本次分拣需要用到的子类别</summary>
     */
    public Category Category => category;
}