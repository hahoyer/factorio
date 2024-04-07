using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using ManageModsAndSaveFiles.Saves;
using MmasfUI.Common;

namespace MmasfUI;

sealed class SaveFileClusterProxy : INotifyPropertyChanged
{
    internal static class Command
    {
        internal const string ViewConflicts = "SaveFileClusterProxy.ViewConflicts";
    }

    readonly FileCluster Data;
    readonly string ConfigurationName;
    FileCluster DataIfRead => Data.IsValidData? Data : null;
    IEnumerable<ModConflict> Conflicts => DataIfRead?.RelevantConflicts;

    [UsedImplicitly]
    public DateTime Created => Data.Created;

    [UsedImplicitly]
    public string Name => Data.Name;

    [UsedImplicitly]
    public TimeSpanProxy Duration;

    [UsedImplicitly]
    public Version Version => DataIfRead?.Version;

    [UsedImplicitly]
    public string GamePart => DataIfRead?.ScenarioName + "/" + DataIfRead?.MapName + "/" + DataIfRead?.CampaignName;

    [UsedImplicitly]
    public bool IsKnownConflict => Conflicts?.Any(c => c.IsKnown) ?? false;

    [UsedImplicitly]
    public string FirstUnknown
    {
        get
        {
            var conflicts = Conflicts?.ToArray();
            if(conflicts == null)
                return null;
            if(conflicts.Any(c => c.IsKnown))
                return null;
            return conflicts.FirstOrDefault(c => !c.IsKnown)?.Mod.FullName;
        }
    }

    [Command(Command.ViewConflicts)]
    public bool IsConflict => Conflicts?.Any() ?? false;

    public SaveFileClusterProxy(FileCluster data, string configurationName)
    {
        Data = data;
        ConfigurationName = configurationName;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Refresh()
    {
        Data.Refresh();
        Duration = new(Data.Duration);
        OnPropertyChanged();
    }

    void OnPropertyChanged()
        => PropertyChanged?.Invoke(this, new(null));

    [Command(Command.ViewConflicts)]
    public void ViewConflicts()
        => MainContainer
            .Instance
            .GetViewConfiguration("ModConflicts", Data.Name, ConfigurationName)
            .ShowAndActivate();
}