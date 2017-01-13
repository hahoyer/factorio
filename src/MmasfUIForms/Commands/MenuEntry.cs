using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace MmasfUIForms.Commands
{
    sealed class MenuEntry<T> : MenuItem
    {
        readonly ICommandHandler<T> CommandHandler;
        readonly T Target;

        public MenuEntry(ICommandHandler<T> commandHandler, T target, string name = null)
            : base(name ?? commandHandler.Title)
        {
            CommandHandler = commandHandler;
            Target = target;
            Tracer.Assert(Target != null);
        }

        internal override System.Windows.Forms.MenuItem CreateMenuItem()
        {
            var menuItem
                = new System.Windows.Forms.MenuItem(Name, (s, e) => CommandHandler.Click(Target));
            menuItem.Popup += (s, e) => menuItem.Enabled = CommandHandler.IsValid(Target);
            return menuItem;
        }

        internal override bool GetEnabled() => CommandHandler.IsValid(Target);
    }

    interface ICommandHandler<in T>
    {
        void Click(T target);
        bool IsValid(T target);
        string Title { get; }
    }
}