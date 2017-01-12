using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    static class SystemConfiguration
    {
        const string ConfigRoot = ".StudioConfig";

        internal static string UserSpecificConfigurationPath
            => OurFolder(Environment.SpecialFolder.ApplicationData);

        internal static string SystemConfigurationPath
            => OurFolder(Environment.SpecialFolder.CommonApplicationData);

        static string OurFolder(Environment.SpecialFolder folder)
            => OurFolder(Environment.GetFolderPath(folder));

        internal static void QueryFileAndOpenIt
            (this IFileOpenController fileOpenController, IStudioApplication application)
        {
            fileOpenController.OnFileOpen("Reni file", "Reni files|*.reni|All files|*.*", "reni");
            if(fileOpenController.FileName == null)
                return;

            //new EditorView(new FileConfiguration(fileOpenController.FileName), application).Run();
        }

        internal static void OnFileOpen
        (
            this IFileOpenController controller,
            string title,
            string filter,
            string defaultExtension,
            int filterIndex = 2,
            bool checkFileExists = false,
            bool restoreDirectory = true
        )
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                RestoreDirectory = restoreDirectory,
                InitialDirectory = InitialDirectory(controller),
                FileName = controller.FileName?.FileHandle().Name,
                Filter = filter,
                CheckFileExists = checkFileExists,
                FilterIndex = filterIndex,
                DefaultExt = defaultExtension
            };

            if(dialog.ShowDialog() != DialogResult.OK)
                return;

            var newFile = dialog.FileName.FileHandle();
            if(!newFile.Exists)
                newFile.String = controller.CreateEmptyFile;
            controller.FileName = newFile.FullName;
        }

        internal static FileConfiguration[] ActiveFileNames
            => AllKnownFileNames
                .Select(item => new FileConfiguration(item))
                .Where(item => item.Status != "Closed")
                .OrderBy(item => item.LastUsed)
                .ToArray();


        internal static string GetConfigurationPath(string editorFileName)
        {
            var projectPath = ".".FileHandle().FullName + "\\";
            var fullFileName = editorFileName.FileHandle().FullName;

            Tracer.Assert(fullFileName.StartsWith(projectPath));
            var fileName = fullFileName.Substring(projectPath.Length);

            return GetKnownConfigurationPath(fileName, fullFileName)
                ?? GetNewConfigurationPath(fileName);
        }

        static string GetKnownConfigurationPath(string fileName, string fullFileName)
        {
            var fileHandle = EditorFilesPath.PathCombine(fileName).FileHandle();
            fileHandle.EnsureDirectoryOfFileExists();

            var result = ConfigurationPathsForAllKnownFiles
                .SingleOrDefault
                (item => GetEditorFileName(item).FileHandle().FullName == fullFileName);
            return result;
        }

        static string EditorFilesPath => ConfigRoot.PathCombine( "EditorFiles");

        static string OurFolder(string head) => head.PathCombine("HoyerWare", "ReniStudio");

        static string GetNewConfigurationPath(string fileName)
        {
            var configurationFileName = fileName.Replace("\\", "_");

            while(true)
            {
                var duplicates = EditorFilesPath
                    .FileHandle()
                    .Items
                    .Select(item => item.Name)
                    .Count(item => item.StartsWith(configurationFileName));

                if(duplicates == 0)
                {
                    var result = EditorFilesPath.PathCombine(configurationFileName);
                    var nameFile = result.PathCombine("Name").FileHandle();
                    nameFile.EnsureDirectoryOfFileExists();
                    nameFile.String = fileName;
                    return result;
                }

                configurationFileName += "_" + duplicates;
            }
        }

        static IEnumerable<string> AllKnownFileNames
            => ConfigurationPathsForAllKnownFiles
                .Select(GetEditorFileName);

        static IEnumerable<string> ConfigurationPathsForAllKnownFiles
        {
            get
            {
                var fileHandle = EditorFilesPath.FileHandle();
                if(fileHandle.Exists)
                    return fileHandle
                        .Items
                        .Select(item => item.FullName);

                return Enumerable.Empty<string>();
            }
        }

        static string GetEditorFileName(string configurationPath)
            => configurationPath.PathCombine("Name").FileHandle().String;

        static string InitialDirectory(IFileOpenController controller)
            => controller.FileName == null
                ? controller.DefaultDirectory.FileHandle().FullName
                : controller.FileName.FileHandle().DirectoryName;
    }

    interface IFileOpenController
    {
        string FileName { get; set; }
        string CreateEmptyFile { get; }
        string DefaultDirectory { get; }
    }
}