using System.Text;
using CommonLib.Lib.ConfigVO;
using CommonLib.Lib.LowerMachine;
using CommonLib.Lib.vo;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CommonLib.Lib.Util;

/**
 * <summary>Usage see @UnitTest.vo.JSONParserTest</summary>
 */
public class ProjectParser
{
    private readonly JObject _jresult;
    
    public static Logger logger = LogManager.GetCurrentClassLogger();
    private string version;

    public  const string V1 = "v1";
    public  const string V2 = "v2";
    public ProjectParser(string projectJsonString,string version)
    {
        this.version = version ;
        _jresult = JObject.Parse(projectJsonString);
        ParseCriteria();
        ParseOutlet();
        ParseOtherProperties();
        afterParsingCheck();
    }
    
    public static Project ParseHttpRequest(HttpRequest Request,string version)
    {
        Project project = null;
        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        {  
            string content = reader.ReadToEndAsync().Result;
           
            if(!string.IsNullOrEmpty(content)){

                try
                {
                    ProjectParser parser = new ProjectParser(content,version);

                    project = parser.getProject();
                    if (project == null) throw new Exception("Project content is not valid " + content);
                    if (logger.IsEnabled(LogLevel.Debug))
                        logger.Debug("Parsing Project id: {} with name {} ", project.Id, project.Name);
                }
                catch (Exception e)
                {
                    throw new Exception("Project content is not valid: " + e.Message);
                }
            }
            else
            {
               throw new Exception( "Project configuration content is empty");
            }
        }
        return project;

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
            var isChecked = (bool) value.SelectToken("checked");
            
            int  criteriaIndex = ConfigUtil.getModuleConfig().CriteriaMapping[code].Index;;
            
            switch (version)
            {
                case V2 : 
                    criteriaIndex = (int) value.SelectToken("index");
                    break;
                case V1 : 
                    criteriaIndex = ConfigUtil.getModuleConfig().CriteriaMapping[code].Index;
                    break;
                default: 
                    criteriaIndex = ConfigUtil.getModuleConfig().CriteriaMapping[code].Index;
                    break;
            }
        
            
            var min = (float) value.SelectToken("data").SelectToken("min");
            var max = (float) value.SelectToken("data").SelectToken("max");
            var range = ((JArray) value.SelectToken("data").SelectToken("range")).Select(jv => (float) jv).ToArray();

            FullCriteria.Add(new Criteria(name, code, criteriaIndex, min, max, range,isChecked));
            if (value != null && (bool) value.SelectToken("checked"))
                EnabledCriteria.Add(new Criteria(name, code, criteriaIndex, min, max, range,isChecked));
        }
    }

    private Genre _genre;
    private Category _category;
    private string id;
    private string name;
    private void ParseOtherProperties()
    {
        _genre = new Genre(ConfigUtil.getModuleConfig().Genre);
        _category = new Category((string?)_jresult.SelectToken("category"));
        id = (string?) _jresult.SelectToken("id");
        name = (string?) _jresult.SelectToken("name");

    }

    private void ParseOutlet()
    {
        var outlets = (JArray) _jresult.SelectToken("channels");
        foreach (var outletDetail in outlets)
        {
            if (outletDetail == null) continue;
            var outlet_no = (string?) outletDetail.SelectToken("channel_no");
            var outlet_type = (string?) outletDetail.SelectToken("type");

            List<Filter[]> filters = null;
            switch (version)
            {
                case V2 : 
                    filters = ParseFilter( (JArray) outletDetail.SelectToken("filters"));
                    break;
                case V1 :
                    filters = praserFilterV1(outletDetail);
                    break;
                default: 
                    filters = praserFilterV1(outletDetail);
                    break;
            }
            

            var outlet = new Outlet(outlet_no, outlet_type, filters.ToArray());
            Outlets.Add(outlet);
        }
    }

    private List<Filter[]> praserFilterV1(JToken outletDetail)
    {
        List<Filter[]> filters = new List<Filter[]>();
        var criteriaMapping = ConfigUtil.getModuleConfig().CriteriaMapping;
        
        List<Filter> firstLevelFilter = new List<Filter>();
        List<string>  foundKeys = new List<string>();
        foreach ((var key, var critieraDetails) in criteriaMapping)
        {
            
            foreach (JProperty prop in outletDetail)
            {
                if (key.Equals(prop.Name))
                {
                    foundKeys.Add(prop.Name);
                   
                        //Console.WriteLine(prop.Name);
                        var criteria_key = prop.Name;
                        var criteria = findCriteria(criteria_key);
                        var boundrryIndeces = ((JArray) prop.Value).Select(jv => (int) jv).ToArray();
                        var filter = new Filter(boundrryIndeces, criteria,Filter.fillBoundries(criteria,boundrryIndeces).ToArray());
                        firstLevelFilter.Add( filter);
                }
            }
        }
        filters.Add(firstLevelFilter.ToArray());
        return filters;
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
                    var filter = new Filter(boundrryIndeces, criteria,Filter.fillBoundries(criteria,boundrryIndeces).ToArray());
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

    private void afterParsingCheck()
    {
        var existanceConfig = ConfigUtil.getModuleConfig().CriteriaMapping["existence"];
        var fmConfig = ConfigUtil.getModuleConfig().CriteriaMapping["fm"];
        
        Criteria existanceCriteria = new Criteria(existanceConfig.Key, existanceConfig.Key, existanceConfig.Index,
            0, 3, new float[]{0.5f,1.5f}, true);
        Criteria fmCriteria = new Criteria(fmConfig.Key, fmConfig.Key, fmConfig.Index, 
            0, 2, new float[] {0.5f}, true);

        
        //默认自由下落是经典模式，默认通道只选择双枣
        var doubleExistanceBoundrryIndeces = new int[] {2};// 1.5-3 {2} 
        var doubleExistanceFilter = new Filter(doubleExistanceBoundrryIndeces,existanceCriteria,Filter.fillBoundries(existanceCriteria,doubleExistanceBoundrryIndeces).ToArray());

        //吹出默认模式下，是分选FM的模式，，默认通道选取 吹出任何单枣和双枣。
        var doubleSingleExistanceBoundrryIndeces = new int[] {1,2};//[0-0.5|0.5-1.5|1.5-3] 1,2 are 0.5-1.5 {1}  and 1.5-3 {2} 
        var doubleSingleExistanceFilter = new Filter(doubleSingleExistanceBoundrryIndeces,existanceCriteria,Filter.fillBoundries(existanceCriteria,doubleSingleExistanceBoundrryIndeces).ToArray());

        //FM选择的filter
        var fmBoundryIndeces = new int[] {1 }; // 0.5-2 {1} 选择FM
        var fmFilter = new Filter(fmBoundryIndeces, fmCriteria,
            Filter.fillBoundries(existanceCriteria, doubleSingleExistanceBoundrryIndeces).ToArray());
        
        EnabledCriteria.Add(fmCriteria);
        EnabledCriteria.Add(existanceCriteria);
        
        FullCriteria.Add(fmCriteria);
        FullCriteria.Add(existanceCriteria);
        
        //如果自由下落的经典模式下，我们只需要确保双枣和FM都被自由下落通道（0）号选中。并且设置0号通道为默认default
        if(ConfigUtil.getModuleConfig().WorkingMode==WorkingMode.FreefallDefault)
        {
            //使用Or 关系让双枣或者FM都进入自由下落通道
            Filter[][] freefallDefaultFilter = new Filter[][]{new Filter[]{doubleExistanceFilter},new Filter[]{fmFilter}};

            Outlet zeroChannelOutlet = new Outlet("0","default",freefallDefaultFilter);
            Outlets.Add(zeroChannelOutlet);
        }
        else//如果是主动吹出模式下，我们需要让0号自由下落通道选中FM，8号通道选中单/双枣。并且设置8号通道为默认default
        {   
            
            //让FM被选中
            Filter[][] fmChooseByZeroWhenEjectdefaultFilter = new Filter[][] {new Filter[] {fmFilter}};
            Outlet zeroChannelOutlet = new Outlet("0","auto",fmChooseByZeroWhenEjectdefaultFilter);
            Outlets.Add(zeroChannelOutlet);
            
            //让单，双枣都进入吹出通道
            Filter[][] ejectDefaultFilter = new Filter[][]{new Filter[]{doubleSingleExistanceFilter}};
            Outlet eightChannelOutlet = Outlets.Where(value => value.ChannelNo == "8").SingleOrDefault();
            eightChannelOutlet.Type = "default";
            
            eightChannelOutlet.Filters = ejectDefaultFilter;
           
        }
        
        
        
    }
}

