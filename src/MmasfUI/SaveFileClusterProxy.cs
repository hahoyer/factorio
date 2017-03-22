using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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
        readonly ModConflicts ModConflictsInstance;
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
            ModConflictsInstance = new ModConflicts(data.Name);
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
                .FindView(ConfigurationName + "." + Data.Name, ModConflictsInstance)
                .ShowAndActivate();

        internal sealed class ModConflicts : DumpableObject, ViewConfiguration.IData
        {
            readonly string FileClusterName;

            public ModConflicts(string fileClusterName)
            {
                Tracer.ConditionalBreak(fileClusterName.StartsWith("HardCrafting"));
                FileClusterName = fileClusterName;
            }

            Window ViewConfiguration.IData.CreateView(ViewConfiguration viewConfiguration)
                => new ModConflictsView(viewConfiguration, FileClusterName);

            string ViewConfiguration.IData.Name => "ModConflicts";
        }
    }
}