using CommandLine;
using hw.DebugFormatter;
using hw.Helper;

namespace FactorioApi;

static class Program
{
    static void Main(string[] args)
    {
        var luaHelper = new LuaHelperGenerator("https://lua-api.factorio.com/latest/runtime-api.json".ToWebSite());
        // var luaHelper = new LuaHelperGenerator("A:\\develop\\factorio-data\\runtime-api.json".ToSmbFile());

        Tracer.ConditionalBreak(luaHelper.HasNewEntries, () => "\n***Warning: new entries found. See previous log.");
        var target = "d:\\data\\Games\\factorio\\develop\\mods\\ingteb\\lib\\MetaData.lua".ToSmbFile();
        Tracer.FilePosition(target.FullName, 0, 1, FilePositionTag.Debug).Log();
        var content = luaHelper.GetAttributeList();
        target.String = content;
        $"***Information: Relevant metadata saved to {target.FullName.Quote()}.".Log();
    }
}

class Parameters
{
    [Option]
    public string Source { get; set; }

    [Option]
    public string Destination { get; set; }
}