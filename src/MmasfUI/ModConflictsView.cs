using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using hw.Helper;
using JetBrains.Annotations;
using ManageModsAndSavefiles;
using ManageModsAndSavefiles.Mods;
using ManageModsAndSavefiles.Saves;
using MmasfUI.Common;

namespace MmasfUI
{
	sealed class ModConflictsView : Window
	{
		sealed class Proxy : INotifyPropertyChanged
		{
			internal static class Command
			{
				internal const string ViewModDescriptions = "ModConflictsView.ViewModDescriptions";
			}

			readonly ModConflict Data;
			ModDescription Mod => Data.Mod;

			public Proxy(ModConflict data) { Data = data; }
			public event PropertyChangedEventHandler PropertyChanged;

			[UsedImplicitly]
			[NotifyPropertyChangedInvocator]
			void OnPropertyChanged([CallerMemberName] string propertyName = null)
				=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

			[UsedImplicitly]
			public string ModName => Mod.Name;
			[UsedImplicitly]
			public string SaveVersion => Data.SaveMod?.Version.ToString();
			[UsedImplicitly]
			public string GameVersion => Data.GameMod?.Version.ToString();

			[Command(Command.ViewModDescriptions)]
			public void ViewModDescriptions()
				=> MainContainer
					.Instance
					.CommandManager
					.ByName(MainContainer.Command.ViewModDictionary)
					.Execute(Mod);
		}

		readonly StatusBar StatusBar = new StatusBar();

		public ModConflictsView(ViewConfiguration viewConfiguration, string fileClusterName)
		{
			var parts = viewConfiguration.Name.Split('.');
			var configurationName = parts[0];
			var fileName = parts.Skip(1).Take(parts.Length - 2).Stringify(".");
			var parent = MmasfContext
				.Instance
				.UserConfigurations.Single(u => u.Name == configurationName)
				.SaveFiles.Single(u => u.Name == fileClusterName);

			var data = parent.RelevantConflicts
				.Select(s => new Proxy(s))
				.ToArray();

			ContextMenu = CreateContextMenu();
			var dataGrid = CreateGrid(data);
			Content = dataGrid;

			Title = viewConfiguration.Data.Name
			        + " of "
			        + parent.Name.Quote()
			        + " of "
			        + parent.Parent.Name.Quote();
			this.InstallPositionPersister(viewConfiguration.PositionPath);
			this.InstallMainMenu(CreateMenu());
			this.InstallStatusLine(StatusBar);
		}

		static ContextMenu CreateContextMenu()
			=> new ContextMenu
			{
				Items =
				{
					"View _Mod descriptions".MenuItem(Proxy.Command.ViewModDescriptions)
				}
			};

		static DataGrid CreateGrid(IEnumerable<Proxy> data)
		{
			var result = new DataGrid
			{
				IsReadOnly = true,
				SelectionMode = DataGridSelectionMode.Single
			};

			result.ConfigurateDefaultColumns();
			result.ActivateSelectedItems();
			result.ItemsSource = data;
			return result;
		}

		static Menu CreateMenu()
			=> new Menu
			{
				Items =
				{
					new MenuItem
					{
						Header = "_File",
						Items =
						{
							"_Exit".MenuItem("Exit")
						}
					},
					new MenuItem
					{
						Header = "_View",
						Items =
						{
							"View _Mod descriptions".MenuItem(Proxy.Command.ViewModDescriptions)
						}
					}
				}
			};
	}
}