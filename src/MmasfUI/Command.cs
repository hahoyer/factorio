using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using hw.DebugFormatter;

namespace MmasfUI
{
    public sealed class Command : DumpableObject, ICommand
    {
        readonly Action Execute;
        readonly Func<bool> CanExecute;

        public Command(Action execute, Func<bool> canExecute = null)
        {
            Execute = execute;
            CanExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if(parameter == null)
                return CanExecute == null || CanExecute();

            NotImplementedMethod(parameter);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if(parameter == null)
            {
                Execute();
                return;
            }

            NotImplementedMethod(parameter);
        }

        event EventHandler ICommand.CanExecuteChanged { add { } remove { } }
    }
}