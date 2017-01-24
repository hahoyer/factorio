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
        readonly MethodInfo Execute;
        readonly PropertyInfo CanExecute;

        internal Command(CommandManager parent, MethodInfo execute, PropertyInfo canExecute)
        {
            Parent = parent;
            Execute = execute;
            CanExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if(parameter == null)
                return Parent.CanExecute(Execute, CanExecute);

            NotImplementedMethod(parameter);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if(parameter == null)
            {
                Parent.Execute(Execute);
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