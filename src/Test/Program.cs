using System.Collections.Generic;
using System.IO;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ICSharpCode.SharpZipLib.Zip;
using ManageModsAndSaveFiles;
using ManageModsAndSaveFiles.Saves;

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local

namespace Test;

static class Program
{
    public static void Main(string[] args) => new ProfilerParser("develop").Format();
    // ManageModsAndSaveFiles.Game.Test();

    static int T(int[] A)
    {
        var p =
            A
                .Where(i1 => i1 > 0)
                .OrderBy(i1 => i1)
                .Distinct()
                .ToArray();
        var i = 1;
        for(; i < p.Length && i == p[i]; i++)
            continue;
        return i;
    }

    static void Error788()
    {
        var f = File.OpenRead(@"c:\Users\hoyer\AppData\Roaming\Factorio\plain\mods\autofill_1.4.6.zip");
        var z = new ZipFile(f);
        var ze = z.GetEntry("autofill_1.4.6/info.json");

        var r = z.GetInputStream(ze);
    }

    static void AllSorts()
    {
        var context = MmasfContext.Instance;
        context.FactorioInformation.Log();
        context.SystemConfigurationFile.ConfigurationPath.FullName.Log();
        context.UserConfigurations.Select(item => item.Path).Stringify("\n").Log();

        var userConfiguration = context
            .UserConfigurations
            .Single(item => item.Name == "Factorio");

        var saveFiles =
                userConfiguration
                    .SaveFiles
            //          .Where(item => item.Name.StartsWith("Campaign"))
            //.ToArray()
            ;

        var format = Profiler.Frame
        (
            () =>
                saveFiles
                    .Select(i => i.ToString())
                    .Stringify("\n")
                    .Format(100.StringAligner())
        );

        format.Log();

        //FindDifference(saveFiles);

        var conflicts = userConfiguration.SaveFileConflicts.ToArray();

        userConfiguration.Path.FullName.Log();
    }

    static void FindDifference(IEnumerable<FileCluster> saveFiles)
    {
        var r = saveFiles.Select(item => item.LevelDatReader).ToArray();
        var differs = false;
        var r1 = r.First();
        while(!r.Any(item => item.IsEnd))
        {
            if(r1.Position % 1000 == 0)
            {
                differs = false;
                ("\n" + r1.Position + ": ").LogLinePart();
            }

            var b = r.Select(item => item.GetNext<byte>()).ToArray();
            if(b.Distinct().Count() > 1)
            {
                if(!differs)
                    (r1.Position % 1000 + ": ").LogLinePart();
                (b.Stringify("/") + " ").LogLinePart();
            }

            differs = b.Distinct().Count() > 1;
        }

        "".Log();
    }
}