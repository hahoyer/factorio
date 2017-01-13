using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;

namespace MmasfUIForms
{
    sealed class PanelCell : DataGridViewCell
    {
        internal sealed class Handler : DumpableObject
        {
            [DisableDump]
            internal Panel Panel;
            [DisableDump]
            readonly PanelCell Parent;

            public Handler(PanelCell parent) { Parent = parent; }

            public void InitializeEditingControl
                (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
            {
                NotImplementedMethod(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            }

            internal object GetFormattedValue
            (
                object value,
                int rowIndex,
                ref DataGridViewCellStyle cellStyle,
                TypeConverter valueTypeConverter,
                TypeConverter formattedValueTypeConverter,
                DataGridViewDataErrorContexts context)
            {
                return Panel;
                NotImplementedMethod
                    (value, rowIndex, cellStyle.ToString(), valueTypeConverter, formattedValueTypeConverter, context);
                return null;
            }
        }

        readonly Handler HandlerInstance;

        public PanelCell() { HandlerInstance = new Handler(this); }

        public override void InitializeEditingControl
        (
            int rowIndex,
            object initialFormattedValue,
            DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

            HandlerInstance.InitializeEditingControl
            (
                rowIndex,
                initialFormattedValue,
                dataGridViewCellStyle);
        }

        public override Type EditType => typeof(Panel);

        public override Type ValueType => typeof(Panel);

        public override object DefaultNewRowValue => null;
        public Panel Panel { get { return HandlerInstance.Panel; } set { HandlerInstance.Panel = value; } }
        protected override object GetFormattedValue
        (
            object value,
            int rowIndex,
            ref DataGridViewCellStyle cellStyle,
            TypeConverter valueTypeConverter,
            TypeConverter formattedValueTypeConverter,
            DataGridViewDataErrorContexts context)
        {
            return HandlerInstance.GetFormattedValue
                (value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
        }
    }
}