﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ManageModsAndSaveFiles.Compression;
using ManageModsAndSaveFiles.Mods;
using ManageModsAndSaveFiles.Reader;

namespace ManageModsAndSaveFiles.Saves;

public sealed class FileCluster : DumpableObject
{
    const string LevelInitDat = "level-init.dat";
    const string LevelDat = "level.dat";

    readonly UserConfiguration Parent;
    readonly string Path;
    readonly ValueCache<BinaryData> DataCache;

    BinaryData Data => DataCache.Value;
    public string Name => Path.ToSmbFile().Name;
    public DateTime Created => Path.ToSmbFile().ModifiedDate;
    public Version Version => Data.Version;
    public string ScenarioName => Data.ScenarioName;
    public string MapName => Data.MapName;
    public string CampaignName => Data.CampaignName;
    public TimeSpan Duration => Data.Duration;
    public ModDescription[] Mods => Data.Mods;

    [DisableDump]
    public BinaryRead LevelDatReader => BinaryRead(LevelDat);

    [DisableDump]
    public IEnumerable<ModConflict> Conflicts
        => Mods.Where(m => m.Name != "base")
            .Merge
            (
                Parent.ModFiles.Where(m => m.IsEnabled == true),
                arg => arg.Name,
                arg => arg.Description.Name
            )
            .SelectMany(item => CreateConflict(item.Item2, item.Item3).NullableToArray());

    [DisableDump]
    public IEnumerable<ModConflict> RelevantConflicts
        => Conflicts.Where(c => c.IsRelevant);

    public bool IsValidData => DataCache.IsValid && Data.IsValid;

    public FileCluster(string path, UserConfiguration parent)
    {
        Path = path;
        Parent = parent;
        DataCache = new(GetData);
        Path.Log();
    }

    public override string ToString()
        => Name.Quote()
            + "  "
            + Version
            + "  "
            + Data.MapName.Quote()
            + "  "
            + Data.ScenarioName.Quote()
            + "  "
            + Data.CampaignName.Quote()
            + "  "
            + Data.Difficulty
            + "  "
            + Duration.Format3Digits();

    protected override string GetNodeDump() => Name;

    BinaryData GetData()
    {
        try
        {
            var reader = Profiler.Measure(() => LevelDatReader);
            reader.UserContext = new UserContext();
            return reader.GetNext<BinaryData>();
        }
        catch(Exception)
        {
            return new(false);
        }
    }

    ModConflict CreateConflict(ModDescription saveMod, Mods.FileCluster gameMod)
    {
        if(saveMod?.Version == gameMod?.Version)
            return null;

        return
            new()
            {
                Save = this, SaveMod = saveMod, GameMod = gameMod?.Description
            };
    }


    BinaryRead BinaryRead(string fileName)
    {
        var handle = GetFile(fileName);
        var reader = handle.Reader;
        var result = new BinaryRead(reader);
        return result;
    }

    IZipFileHandle GetFile(string name)
    {
        var fileHandle = Profiler.Measure(() => Path.ZipHandle());
        var zipFileHandles = Profiler.Measure(() => fileHandle.Items);
        var zipFileHandle = Profiler.Measure
            (() => zipFileHandles.Where(item => item.ItemName == name && item.Depth == 2));
        return Profiler.Measure(() => zipFileHandle.Single());
    }

    public void Refresh() => DataCache.IsValid = false;
}