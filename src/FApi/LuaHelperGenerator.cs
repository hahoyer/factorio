using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace FactorioApi;

sealed class LuaHelperGenerator : DumpableObject
{
    readonly WebSite Source;
    readonly Assessments Assessments;
    readonly GameApi GameApi;

    public LuaHelperGenerator(WebSite source)
    {
        Source = source;
        GameApi = GetApi();
        Assessments = new(SmbFile.SourcePath(), GameApi);
    }

    Class[] Classes => GameApi.Classes;

    public bool HasNewEntries => Assessments.HasNewEntries;

    GameApi GetApi()
    {
        var text = Source.String;
        var objectForm = text.FromJson().ToObject();
        return text.FromJson<GameApi>();
    }

    public string GetAttributeList()
        => @$"-- Generated {DateTime.Now:O} by FactorioApi {GetType().Assembly.GetName().Version} 
-- see https://github.com/hahoyer/factorio/tree/master/src/FApi
return {Classes.Where(IsRelevant).Select(GetAttributeList).Stringify(",\n").Indent().Surround("{")}";

    string GetAttributeList(Class luaClass)
    {
        var attributeData = Assessments
            .GetRelevantAttributes(luaClass)
            .Select(name => $"{name} = true")
            .Stringify(",\n");
        return @$"{luaClass.Name} = 
{{
    {attributeData.Indent()}
}}";
    }

    bool IsRelevant(Class arg) => Assessments.IsRelevant(arg);

    bool IsRelevant(Class parent, Field field) => Assessments.IsRelevant(parent, field);
}