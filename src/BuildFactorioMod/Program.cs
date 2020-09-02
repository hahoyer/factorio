using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;
using hw.DebugFormatter;
using hw.Helper;
using HWBase;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json.Linq;

namespace BuildFactorioMod
{
    static class Program
    {
        static void Main(string[] args)
        {
            var x
                = (Parser
                        .Default
                        .ParseArguments<Parameters>(args)
                    as Parsed<Parameters>)
                .AssertNotNull()
                .Value;


            var information = GetInformation(x.Source);
            var version = information["version"];
            var name = information["name"];
            var releaseFileName = name + "_" + version;
            var releaseFolder = x.Destination;
            var zipFile = releaseFolder.PathCombine(releaseFileName + ".zip");

            ("building release " + version).WriteLine();

            CreateZipFile(zipFile, x.Source.ToSmbFile(), name.ToString());
            "done".WriteLine();
        }

        static JObject GetInformation(string sourceDirectory)
            => sourceDirectory
                .PathCombine("info.json")
                .ToSmbFile()
                .String
                .FromJson();

        public static void CreateZipFile(string destinationPath, SmbFile source, string targetPath)
        {
            var zipStream = new ZipOutputStream(File.Create(destinationPath));
            zipStream.SetLevel(3); 
            CompressFolder(zipStream, source, targetPath);
            zipStream.IsStreamOwner = true; 
            zipStream.Close();
        }

        static void CompressFolder(ZipOutputStream zipStream, SmbFile handle, string targetPath)
        {
            foreach(var item in handle.Items)
            {
                var itemPath = targetPath.PathCombine(item.Name);
                if(item.IsDirectory)
                    CompressFolder(zipStream, item, itemPath);
                else
                    CompressFile(zipStream, item, itemPath);
            }
        }

        static void CompressFile(ZipOutputStream zipStream, SmbFile handle, string targetPath)
        {
            var newEntry = new ZipEntry(ZipEntry.CleanName(targetPath))
            {
                DateTime = handle.ModifiedDate,
                Size = handle.Size
            };

            zipStream.PutNextEntry(newEntry);

            var buffer = new byte[4096];
            using (var streamReader = handle.Reader)
                StreamUtils.Copy(streamReader, zipStream, buffer);
            zipStream.CloseEntry();

        }
    }

    class Parameters
    {
        [Option]
        public string Source {get; set;}

        [Option]
        public string Destination {get; set;}
    }
}