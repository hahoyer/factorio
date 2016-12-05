using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSavefiles.Mods;

namespace ManageModsAndSavefiles.Saves
{
    public sealed class FileCluster : DumpableObject
    {
        const string LevelInitDat = "level-init.dat";

        readonly string Path;
        readonly MmasfContext Parent;

        Version VersionValue;
        ModDescription[] ModsValue;
        string GameType;
        string ScenarioName;
        string GameKind;

        public FileCluster(string path, MmasfContext parent)
        {
            Path = path;
            Parent = parent;
        }

        public string Name => Path.FileHandle().Name;
        protected override string GetNodeDump() => Name;
        public override string ToString() => Name.Quote();

        public Version Version
        {
            get
            {
                if(VersionValue == null)
                    ReadLevelInitDatFile();
                return VersionValue;
            }
        }

        public ModDescription[] Mods
        {
            get
            {
                if(ModsValue == null)
                    ReadLevelInitDatFile();
                return ModsValue;
            }
        }

        void ReadLevelInitDatFile()
        {
            var reader = GetFile(LevelInitDat).BinaryReader;
            var version = new Version
            (
                reader.GetNext<short>(),
                reader.GetNext<short>(),
                reader.GetNext<short>(),
                reader.GetNext<short>()
            );

            VersionValue = version;

            var lookAhead2 = reader.GetBytes(150);
            var gameKind = reader.GetNextString<int>();
            GameKind = gameKind;
            var gameType = reader.GetNextString<int>();
            GameType = gameType;
            var scenarioName = reader.GetNextString<int>();
            ScenarioName = scenarioName;

            var version1 = new Version(0, 14, 14);
            var unknownSize =
                Version < new Version(0, 13)
                    ? 23
                    : Version < version1
                        ? 19
                        : 16;


            var unknown = reader.GetNextBytes(unknownSize);
            var lookAhead1 = reader.GetBytes(150);
            var modCount = reader.GetNext<int>();
            var lookAhead = reader.GetBytes(150);
            Tracer.Assert(modCount < 100);

            if(Version < version1)
            {
                ModsValue =
                    modCount
                        .Select(i => Parent.CreateModReferenceBefore0_14(i, reader))
                        .ToArray();
                return;
            }

            ModsValue =
                modCount
                    .Select(i => Parent.CreateModReference(i, reader))
                    .ToArray();
        }

        ZipFileHandle GetFile(string name)
        {
            return Path.ZipFileHandle().Items.Single(item => item.ItemName == name);
        }

        public ModConflict GetConflict(ModDescription saveMod, Mods.FileCluster mod)
        {
            if(mod == null)
                return
                    new ModConflict.RemovedMod
                    {
                        Save = this,
                        SaveMod = saveMod
                    };

            if(saveMod == null)
                return new ModConflict.AddedMod
                {
                    Save = this,
                    Mod = mod
                };

            if(saveMod.Version == mod.Description.Version)
                return null;

            return
                new ModConflict.UpdatedMod
                {
                    Save = this,
                    SaveMod = saveMod,
                    ModVersion = mod.Description.Version
                };
        }
    }

    // d4rkpl4y3r

    //int main()
    //{
    //    string file = "C:\\Users\\USER\\Desktop\\level-init.dat";
    //    char* buffer = new char[257];
    //    ifstream is(file, ifstream::binary);
    //is.read(buffer, 48);
    //is.read(buffer, 4);
    //    int modCount = buffer[0];
    //    while (modCount > 0)
    //    {
    //    is.read(buffer, 1);
    //        int length = buffer[0];
    //    is.read(buffer, length);
    //        buffer[length] = 0;
    //        cout << buffer;
    //    is.read(buffer, 3);
    //        cout << " v" << ((int)buffer[0]) << '.' << ((int)buffer[1]) << '.' << ((int)buffer[2]) << endl;
    //        modCount--;
    //    }
    //}
}