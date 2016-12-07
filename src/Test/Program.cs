﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Saves;

namespace Test
{
    class Program
    {
        public static void Main(string[] args)
        {
            var context = MmasfContext.Instance;
            Tracer.Line(context.FactorioInformation);
            Tracer.Line(context.SystemConfiguration.ConfigurationPath);
            Tracer.Line(context.UserConfigurations.Select(item => item.Path).Stringify("\n"));

            var userConfiguration = context
                .UserConfigurations
                .Single(item => item.Name == "HardCrafting");

            var saveFiles =
                    userConfiguration
                        .SaveFiles
                //          .Where(item => item.Name.StartsWith("Campaign"))
                //.ToArray()
                ;

            Tracer.Line
                (saveFiles.Select(i => i.ToString()).Stringify("\n").Format(100.StringAligner()));

            //FindDifference(saveFiles);

            var conflicts = userConfiguration.SaveFileConflicts.ToArray();

            Tracer.Line(userConfiguration.Path);
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
                    Tracer.LinePart("\n" + r1.Position + ": ");
                }

                var b = r.Select(item => item.GetNext<byte>()).ToArray();
                if(b.Distinct().Count() > 1)
                {
                    if(!differs)
                        Tracer.LinePart(r1.Position % 1000 + ": ");
                    Tracer.LinePart(b.Stringify("/") + " ");
                }
                differs = b.Distinct().Count() > 1;
            }

            Tracer.Line("");
        }
    }
}