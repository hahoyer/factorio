using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;

namespace FactorioApi;

sealed class LuaHelperGenerator : DumpableObject
{
    readonly IFile Source;
    readonly Assessments Assessments;
    readonly GameApi GameApi;

    public LuaHelperGenerator(IFile source)
    {
        Source = source;
        GameApi = GetApi();
        Assessments = new(SmbFile.SourcePath(), GameApi);
    }

    Class[] Classes => GameApi.Classes;

    GameApi GetApi()
    {
        var text = Source.String;
        var objectForm = text.FromJson().ToObject();
        return text.FromJson<GameApi>();
    }

    public string GetAttributeList()
        => "return\n" + Classes.Where(IsRelevant).Select(GetAttributeList).Stringify(",\n").Indent();

    string GetAttributeList(Class luaClass)
    {
        var attributeData = luaClass
            .Attributes
            .Where(field => IsRelevant(luaClass, field))
            .Select(GetAttribute)
            .Stringify(", ");
        return @$"{luaClass.Name} = 
{{
    {attributeData}
}}";
    }

    bool IsRelevant(Class arg) => Assessments.IsRelevant(arg);

    bool IsRelevant(Class parent, Field field) => Assessments.IsRelevant(parent, field);

    string GetAttribute(Field field)
    {
        NotImplementedMethod(field);
        return default;
    }
}