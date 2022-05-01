namespace CommonLib.Lib.vo;

/**
 * <summary>类别，比如枣，苹果，包装袋等等</summary>
 */
public class Genre
{
    private GenreName name;

    public Genre(GenreName name)
    {
        this.name = name;
    }

    public GenreName Name => name;

}

//椰枣
//灰枣
//骏枣
//苹果
//栗子
//封袋
public enum GenreName{
    palmDate,
    greyDate=100,
    junJujube,
    apple,
    chestnut=200,
    packingsealing
}
