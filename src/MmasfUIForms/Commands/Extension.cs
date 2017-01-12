using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MmasfUI.Commands
{
    static class Extension
    {
        internal static MainMenu CreateMenu(this ContextView target)
        {
            var result = new MainMenu();
            result.MenuItems.AddRange
                (target.Menus().Select(item => item.CreateMenuItem()).ToArray());
            return result;
        }

        static MenuItem MenuItem<T>
            (this T target, ICommandHandler<T> commandHandler, string title = null)
            => new MenuEntry<T>(commandHandler, target, title);

        static IEnumerable<Menu> Menus(this ContextView target)
            => new[]
            {
                new Menu("File")
                {
                    Entries = new[]
                    {
                        target.MenuItem(Command.New),
                        target.Parent.MenuItem(Command.Exit),
                    }
                }
            };

        public static void InvokeAsynchron(this Control target, Action action)
        {
            if(target.InvokeRequired)
                target.Invoke(action);
            else
                action();
        }
    }
}