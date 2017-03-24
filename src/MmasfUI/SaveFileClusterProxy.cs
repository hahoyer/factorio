using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using ManageModsAndSavefiles.Saves;
using MmasfUI.Common;

namespace MmasfUI
{
    sealed class SaveFileClusterProxy : INotifyPropertyChanged
    {
        internal static class Command
        {
            internal const string ViewConflicts = "SaveFileClusterProxy.ViewConflicts";
        }

        readonly FileCluster Data;
        readonly string ConfigurationName;
        FileCluster DataIfRead => Data.IsDataRead ? Data : null;
        IEnumerable<ModConflict> Conflicts => DataIfRead?.RelevantConflicts;

        [UsedImplicitly]
        public DateTime Created => Data.Created;
        [UsedImplicitly]
        public string Name => Data.Name;
        [UsedImplicitly]
        public string FirstConflict => Conflicts?.FirstOrDefault()?.Mod.FullName;

        [UsedImplicitly]
        public TimeSpanProxy Duration { get; set; }
        [UsedImplicitly]
        public Version Version => DataIfRead?.Version;
        [UsedImplicitly]
        public string ScenarioName => DataIfRead?.ScenarioName;
        [UsedImplicitly]
        public string MapName => DataIfRead?.MapName;
        [UsedImplicitly]
        public string CampaignName => DataIfRead?.CampaignName;

        public SaveFileClusterProxy(FileCluster data, string configurationName)
        {
            Data = data;
            ConfigurationName = configurationName;
        }

        public void Refresh()
        {
            Data.IsDataRead = true;
            Duration = new TimeSpanProxy(Data.Duration);
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        [Command(Command.ViewConflicts)]
        public void ViewConflicts()
            => MainContainer
                .Instance
                .FindViewConfiguration("ModConflicts", Data.Name, ConfigurationName)
                .ShowAndActivate();
    }
}