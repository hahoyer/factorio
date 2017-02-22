using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

namespace MmasfUI
{
    sealed class TimeSpanProxy : DumpableObject, INotifyPropertyChanged
    {
        [UsedImplicitly]
        public TimeSpan Value { get; }

        public TimeSpanProxy(TimeSpan value)
        {
            Value = value;
            OnPropertyChanged();
        }

        [UsedImplicitly]
        public string DisplayValue => Value.Format3Digits();

        public override string ToString() => Value.Format3Digits();

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        internal static void Register(DataGrid dataGrid)
        {
            dataGrid.AutoGeneratingColumn += (s, e) => OnAutoGeneratingColumns(e);
        }

        static void OnAutoGeneratingColumns(DataGridAutoGeneratingColumnEventArgs args)
        {
            if(args.PropertyType != typeof(TimeSpanProxy))
                return;

            var column = (DataGridTextColumn) args.Column;
            var binding = (Binding) column.Binding;
            binding.Path.Path += ".DisplayValue";
            args.Column.CellStyle = new Style
            {
                Setters =
                {
                    new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right)
                }
            };
            column.SortMemberPath += ".Value";
            column.CanUserSort = true;
        }
    }
}