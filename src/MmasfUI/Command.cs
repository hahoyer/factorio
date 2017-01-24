using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using hw.DebugFormatter;

namespace MmasfUI
{
    sealed class Command : DumpableObject, ICommand
    {
        readonly CommandManager Parent;
        readonly MethodInfo Method;

        internal Command(CommandManager parent, MethodInfo method)
        {
            Parent = parent;
            Method = method;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if(parameter == null)
                return Parent.CanExecute(Method);

            NotImplementedMethod(parameter);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if(parameter == null)
            {
                Parent.Execute(Method);
                return;
            }

            NotImplementedMethod(parameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }
    }
}