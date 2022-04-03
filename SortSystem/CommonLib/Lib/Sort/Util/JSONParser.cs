using System.Diagnostics;
using CommonLib.Lib.vo;
using Newtonsoft.Json.Linq;

namespace CommonLib.Lib.Sort.Util;
/**
 * <summary>Usage see @LibUnitTest.vo.JSONParserTest</summary>
 */
public class JSONParser
{
    public List<Criteria> FullCriteria => fullCriteria;

    public List<Criteria> EnabledCriteria => enabledCriteria;


    private List<Criteria> fullCriteria = new List<Criteria>();
    private List<Criteria> enabledCriteria = new List<Criteria>();

    private List<Outlet> outlets = new List<Outlet>();
    
    private JObject _jresult;
    public JSONParser(string project_json_string)
    {
        _jresult = JObject.Parse(project_json_string);
        ParseCriteria();
        ParseOutlet();
    }

    private void ParseCriteria()
    {   
        var criteria = (JObject)_jresult.SelectToken("criteria");
        foreach (var (key, value) in criteria)
        {
            if (value == null) continue;
            var code = (string?) key;
            var name = (string?) value.SelectToken("name");
            var criteriaIndex=(int) value.SelectToken("index");
            var min = (float) value.SelectToken("data").SelectToken("min");
            var max = (float) value.SelectToken("data").SelectToken("max");
            var range = ((JArray) value.SelectToken("data").SelectToken("range")).Select(jv => (float) jv).ToArray();
            
            fullCriteria.Add(new Criteria(name,code,criteriaIndex,min,max,range));
            if (value != null && (bool) value.SelectToken("checked")) {
                enabledCriteria.Add(new Criteria(name,code,criteriaIndex,min,max,range));
            }
        }
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

            Outlet outlet = new Outlet(outlet_no, outlet_type, filters.ToArray());
            this.outlets.Add(outlet);
        }

    }

    private List<Filter[]> ParseFilter(JArray filters)
    {
        List<Filter[]> filterList = new List<Filter[]>();
        if(filters!=null){
            foreach (var value in filters.Children<JObject>())
            {
                if (value == null) continue;
                var _filters = new Filter[value.Properties().Count()];
                var count = 0;
                foreach (JProperty prop in value.Properties())
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
        }
        return filterList;
    }

    private Criteria findCriteria(string name)
    {
        foreach (var item in enabledCriteria)
        {
            if (name.Equals(item.Code))
            {
                return item;
            }
        }

        return null;
    }
}