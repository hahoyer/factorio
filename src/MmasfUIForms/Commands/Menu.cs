using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUIForms.Commands
{
    sealed class Menu : MenuItem
    {
        public Menu(string name)
            : base(name) { }

        internal MenuItem[] Entries;

        internal override System.Windows.Forms.MenuItem CreateMenuItem()
        {
            var result = new System.Windows.Forms.MenuItem(Name);

            var menuItems = Entries.Select
                (
                    item => new
                    {
                        item,
                        result = item.CreateMenuItem()
                    }
                )
                .ToArray();

            result.MenuItems.AddRange(menuItems.Select(item => item.result).ToArray());

            result.Popup += (s, e) =>
            {
                foreach(var item in menuItems)
                    item.result.Enabled = item.item.GetEnabled();
            };

            return result;
        }

        internal override bool GetEnabled() => true;
    }
}