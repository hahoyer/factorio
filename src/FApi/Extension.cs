using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Newtonsoft.Json.Linq;

namespace FactorioApi;

static class Extension
{
    public static WebSite ToWebSite(this string name) => new(name);

    public static object ToObject(this JToken target)
    {
        var jTokenType = target.Type;

        if(target is IDictionary<string, JToken> dictionary)
        {
            var result = new Dictionary<string, object>();
            foreach(var item in dictionary)
                result[item.Key] = ToObject(item.Value);
            return result;
        }

        if(jTokenType == JTokenType.String)
            return (string)target;
        if(jTokenType == JTokenType.Integer)
            return (int)target;

        if(target is IEnumerable<JToken> array)
            return array.Select(item => ToObject(item)).ToArray();

        Dumpable.NotImplementedFunction(target);
        return default;
    }
}