using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    sealed class FileConfiguration : DumpableObject
    {
        internal readonly string FileName;
        readonly ValueCache<Persister> FilePersister;

        public FileConfiguration(string fileName)
        {
            FileName = fileName;
            FilePersister = new ValueCache<Persister>
                (() => new Persister(ItemFile("EditorConfiguration")));
        }

        internal string Status
        {
            get { return ItemFile("Status").String; }
            private set { ItemFile("Status").String = value; }
        }

        internal DateTime? LastUsed
        {
            get { return FromDateTime(ItemFile("LastUsed").String); }
            private set { ItemFile("LastUsed").String = value?.ToString("O"); }
        }

        static DateTime? FromDateTime(string value)
        {
            if(value == null)
                return null;

            DateTime result;
            if(DateTime.TryParse(value, out result))
                return result;

            return null;
        }

        internal void ConnectToFrame(Form frame)
        {
            frame.FormClosing += (a, s) => OnClosing();
            frame.Activated += (a, s) => LastUsed = DateTime.Now;
        }

        hw.Helper.File ItemFile(string itemName) => ItemFileName(itemName).FileHandle();

        string ItemFileName(string itemName)
            => SystemConfiguration
                .GetConfigurationPath(FileName)
                .PathCombine(itemName);

        void OnClosing() { Status = "Closed"; }
        void OnActivated() { LastUsed = DateTime.Now; }

        internal string PositionPath => ItemFileName("Position");
    }
}