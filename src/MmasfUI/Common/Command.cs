using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using hw.DebugFormatter;

namespace MmasfUI.Common
{
    sealed class Command : DumpableObject, ICommand
    {
        readonly CommandManager Parent;
        readonly MethodInfo FlatExecute;
        readonly MethodInfo ParamterizedExecute;
        readonly PropertyInfo CanExecute;

        internal Command
        (
            CommandManager parent,
            MethodInfo flatExecute,
            MethodInfo paramterizedExecute,
            PropertyInfo canExecute)
        {
            Parent = parent;
            FlatExecute = flatExecute;
            ParamterizedExecute = paramterizedExecute;
            CanExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            var execute = parameter == null ? FlatExecute : ParamterizedExecute;
            return Parent.CanExecute(execute, CanExecute);
        }

        void ICommand.Execute(object parameter)
        {
            if(parameter == null)
                Parent.Execute(FlatExecute);
            else
                Parent.Execute(ParamterizedExecute, parameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }
    }
}