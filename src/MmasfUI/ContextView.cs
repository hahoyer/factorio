using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ManageModsAndSavefiles;
using MmasfUI.Commands;

namespace MmasfUI
{
    sealed class ContextView : ChildView
    {
        public readonly IStudioApplication Parent;
        readonly MmasfContext Context;

        public ContextView(IStudioApplication parent)
            : base(parent)
        {
            Parent = parent;
            Context = parent.Context;
            Client = Context.UserConfigurations.Select(CreateView).CreateRowView();
            Frame.Menu = this.CreateMenu();
            Title = "Factorio user configurations";
        }

        Control CreateView(UserConfiguration configuration)
            => true.CreateLineupView
            (
                CreateIndicatorView(configuration),
                configuration.Name.CreateView()
            );

        Panel CreateIndicatorView(UserConfiguration configuration)
            => new Panel
            {
                Size = new Size(10, 30),
                BackColor = IndicatorColor(configuration)
            };

        Color IndicatorColor(UserConfiguration configuration)
        {
            if(Context.DataConfiguration.RootUserConfigurationPath == configuration.Path)
            {
                if(Context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path)
                    return Color.DarkBlue;

                return Color.LightBlue;
            }

            if(Context.DataConfiguration.CurrentUserConfigurationPath == configuration.Path)
                return Color.Black;

            return Color.LightGray;
        }

        public void New() { throw new NotImplementedException(); }

        internal DataGridView CreateView()
        {
            var result = new DataGridView
            {
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.RowHeaderSelect,
                MultiSelect = true
            };
            result.Columns.AddRange(DataGridViewColumns().ToArray());
            result.Rows.AddRange(Context.UserConfigurations.Select(CreateRowForStep).ToArray());
            result.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            result.CellClick += (a, b) => OnSelect(b.RowIndex, b.ColumnIndex);
            return result;
        }

        void OnSelect(int rowIndex, int columnIndex) { throw new NotImplementedException(); }

        internal DataGridViewRow CreateRowForStep(UserConfiguration item)
        {
            var result = new DataGridViewRow();
            result.Cells.Add(new PanelCell(CreateIndicatorView(item)));
            result.Cells.Add
            (
                new DataGridViewTextBoxCell
                {
                    Value = item.Name
                });
            return result;
        }

        internal static IEnumerable<DataGridViewColumn> DataGridViewColumns()
        {
            yield return new DataGridViewColumn
            {
                Name = "",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            };
            yield return new DataGridViewTextBoxColumn
            {
                Name = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            };
        }
    }


    sealed class PanelCell : DataGridViewCell
    {
        public PanelCell(Panel panel)
        {
            Tracer.TraceBreak();
        }
    }
}