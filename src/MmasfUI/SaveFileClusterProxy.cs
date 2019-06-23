using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using ManageModsAndSaveFiles.Saves;
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
        FileCluster DataIfRead => Data.IsValidData ? Data : null;
        IEnumerable<ModConflict> Conflicts => DataIfRead?.RelevantConflicts;

        [UsedImplicitly]
        public DateTime Created => Data.Created;
        [UsedImplicitly]
        public string Name => Data.Name;

        [UsedImplicitly]
        public TimeSpanProxy Duration { get; set; }
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
        public bool IsConflict => Conflicts?.Any() ?? false;

        [Command(Command.ViewConflicts)]
        public void ViewConflicts()
            => MainContainer
                .Instance
                .GetViewConfiguration("ModConflicts", Data.Name, ConfigurationName)
                .ShowAndActivate();
    }
}