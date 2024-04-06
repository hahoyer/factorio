using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using ManageModsAndSaveFiles;

namespace MmasfUI;

static class SystemConfiguration
{
    const string ViewConfigurationFileName = "UI";
    const string ViewIdentifierName = "Identifier";

    static string ViewConfigurationPath
        => MmasfContext.Instance
            .SystemConfiguration
            .ProgramFolder
            .FullName
            .PathCombine(ViewConfigurationFileName);

    internal static string GetConfigurationPath(string[] viewIdentifier)
        => GetConfigurationPath(viewIdentifier.Stringify("\n"));

    static string GetConfigurationPath(string viewIdentifier)
        => GetKnownConfigurationPath(viewIdentifier) ?? GetNewConfigurationPath(viewIdentifier);

    static string GetKnownConfigurationPath(string viewIdentifier)
    {
            var result = ConfigurationPathsForAllKnownFiles
                .SingleOrDefault
                    (item => GetViewIdentifierString(item) == viewIdentifier);
            return result;
        }

    static string[] GetViewIdentifier(string viewIdentifier)
        => GetViewIdentifierString(viewIdentifier)
            .Split('\n')
            .Select(key => key.Trim('\n', '\r', '\t', ' '))
            .ToArray();

    static string GetViewIdentifierString(string viewIdentifier)
        => viewIdentifier
            .PathCombine(ViewIdentifierName)
            .ToSmbFile()
            .String;

    static string GetNewConfigurationPath(string viewIdentifier)
    {
            var configurationFileName = viewIdentifier.Replace("\n", ".");

            while(true)
            {
                var duplicates = ViewConfigurationPath
                    .ToSmbFile()
                    .Items
                    .Select(item => item.Name)
                    .Count(item => item.StartsWith(configurationFileName));

                if(duplicates == 0)
                {
                    var result = ViewConfigurationPath.PathCombine(configurationFileName);
                    var nameFile = result.PathCombine(ViewIdentifierName).ToSmbFile();
                    nameFile.String = viewIdentifier;
                    return result;
                }

                configurationFileName += "_" + duplicates;
            }
        }

    static IEnumerable<string[]> AllKnownViewIdentifiers
        => ConfigurationPathsForAllKnownFiles
            .Select(GetViewIdentifier);

    static IEnumerable<string> ConfigurationPathsForAllKnownFiles
    {
        get
        {
                var fileHandle = ViewConfigurationPath.ToSmbFile();
                if(fileHandle.Exists)
                    return fileHandle
                        .Items
                        .Select(item => item.FullName);

                return Enumerable.Empty<string>();
            }
    }

    internal static void OpenActiveViews()
    {
            var views = AllKnownViewIdentifiers
                .Select(identifier => new ViewConfiguration(identifier))
                .Where(f => f.Status == "Open")
                .OrderBy(f => f.LastUsed);

            foreach(var view in views)
                view.ShowAndActivate();
        }

    public static void Cleanup()
    {
            var enumerable = ConfigurationPathsForAllKnownFiles
                .Select
                (
                    path => new
                    {
                        path,
                        isValid = GetViewIdentifierString(path) != null
                    })
                .ToArray();

            var filesToDelete = enumerable
                .Where(i => !i.isValid);

            foreach(var item in filesToDelete)
                item.path.ToSmbFile().Delete(true);
        }
}