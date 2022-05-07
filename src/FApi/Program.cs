using CommandLine;
using hw.DebugFormatter;

namespace FactorioApi;

static class Program
{
    static void Main(string[] args)
    {
        var luaHelper = new LuaHelperGenerator("https://lua-api.factorio.com/latest/runtime-api.json".ToWebSite());
        // var luaHelper = new LuaHelperGenerator("A:\\develop\\factorio-data\\runtime-api.json".ToSmbFile());

        Tracer.ConditionalBreak(luaHelper.HasNewEntries, () => "\n***Warning: new entries found. See previous log.");
        luaHelper.GetAttributeList().Log();

        Tracer.ConditionalBreak(true);
    }
}

class Parameters
{
    [Option]
    public string Source { get; set; }

    [Option]
    public string Destination { get; set; }
}