using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace MmasfUI.Commands
{
    abstract class MenuItem : DumpableObject
    {
        internal string Name { get; }

        protected MenuItem(string name) { Name = name; }

        internal abstract System.Windows.Forms.MenuItem CreateMenuItem();
        internal abstract bool GetEnabled();
    }

    static class Command
    {
        public static ICommandHandler<ContextView> New { get; }
            = new CommandHandler<ContextView>(s => s.New(), s => true, "New...");
        public static ICommandHandler<IStudioApplication> Exit { get; }
            = new CommandHandler<IStudioApplication>(s => s.Exit(), s => true, "Exit");
    }

    sealed class CommandHandler<T> : DumpableObject, ICommandHandler<T>
    {
        Action<T> Click { get; }
        Func<T, bool> IsValid { get; }
        string Title { get; }

        public CommandHandler(Action<T> click, Func<T, bool> isValid, string title)
        {
            Click = click;
            IsValid = isValid;
            Title = title;
        }

        void ICommandHandler<T>.Click(T target) => Click(target);
        bool ICommandHandler<T>.IsValid(T target) => IsValid(target);

        string ICommandHandler<T>.Title => Title;
    }
}