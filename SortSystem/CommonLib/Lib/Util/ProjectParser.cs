using CommonLib.Lib.vo;
using Newtonsoft.Json.Linq;

namespace CommonLib.Lib.Util;

/**
 * <summary>Usage see @LibUnitTest.vo.JSONParserTest</summary>
 */
public class ProjectParser
{
    private readonly JObject _jresult;
    

    public ProjectParser(string projectJsonString)
    {
        _jresult = JObject.Parse(projectJsonString);
        ParseCriteria();
        ParseOutlet();
        ParseOtherProperties();
    }

    public Project getProject()
    {
        return new Project(id,name,_genre,_category,EnabledCriteria.ToArray(),Outlets.ToArray());
    }

    public List<Criteria> FullCriteria { get; } = new();

    public List<Criteria> EnabledCriteria { get; } = new();

    public List<Outlet> Outlets { get; } = new();


    private void ParseCriteria()
    {
        var criteria = (JObject) _jresult.SelectToken("criteria")!;
        foreach (var (key, value) in criteria)
        {
            if (value == null) continue;
            var code = (string?) key;
            var name = (string?) value.SelectToken("name");
            var criteriaIndex = (int) value.SelectToken("index");
            var min = (float) value.SelectToken("data").SelectToken("min");
            var max = (float) value.SelectToken("data").SelectToken("max");
            var range = ((JArray) value.SelectToken("data").SelectToken("range")).Select(jv => (float) jv).ToArray();

            FullCriteria.Add(new Criteria(name, code, criteriaIndex, min, max, range));
            if (value != null && (bool) value.SelectToken("checked"))
                EnabledCriteria.Add(new Criteria(name, code, criteriaIndex, min, max, range));
        }
    }

    private Genre _genre;
    private Category _category;
    private string id;
    private string name;
    private void ParseOtherProperties()
    {
        _genre = new Genre(ConfigUtil.loadModuleConfig().Genre);
        _category = new Category((string?)_jresult.SelectToken("category"));
        id = (string?) _jresult.SelectToken("id");
        name = (string?) _jresult.SelectToken("name");

    }

    private void ParseOutlet()
    {
        var outlets = (JArray) _jresult.SelectToken("channels");
        foreach (var value in outlets)
        {
            if (value == null) continue;
            var outlet_no = (string?) value.SelectToken("channel_no");
            var outlet_type = (string?) value.SelectToken("type");
            var _jobj_filters = (JArray) value.SelectToken("filters");
            var filters = ParseFilter(_jobj_filters);

            var outlet = new Outlet(outlet_no, outlet_type, filters.ToArray());
            Outlets.Add(outlet);
        }
    }

    private List<Filter[]> ParseFilter(JArray filters)
    {
        var filterList = new List<Filter[]>();
        if (filters != null)
            foreach (var value in filters.Children<JObject>())
            {
                if (value == null) continue;
                var _filters = new Filter[value.Properties().Count()];
                var count = 0;
                foreach (var prop in value.Properties())
                {
                    //Console.WriteLine(prop.Name);
                    var criteria_key = prop.Name;
                    var criteria = findCriteria(criteria_key);
                    var boundrryIndeces = ((JArray) prop.Value).Select(jv => (int) jv).ToArray();
                    var filter = new Filter(boundrryIndeces, criteria);
                    _filters[count++] = filter;
                }

                filterList.Add(_filters);
            }

        return filterList;
    }

    private Criteria findCriteria(string name)
    {
        foreach (var item in EnabledCriteria)
            if (name.Equals(item.Code))
                return item;

        return null;
    }
}