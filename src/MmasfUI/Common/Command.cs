using System;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI.Common;

sealed class Command : DumpableObject, ICommand
{
    readonly CommandManager Parent;
    readonly MethodInfo[] Executes;
    readonly PropertyInfo CanExecute;

    internal Command
    (
        CommandManager parent,
        MethodInfo[] executes,
        PropertyInfo canExecute
    )
    {
            Parent = parent;
            Executes = executes;
            CanExecute = canExecute;
        }

    bool ICommand.CanExecute(object parameter)
    {
            var execute = FindExecutor(parameter);
            return Parent.CanExecute(execute, CanExecute);
        }

    event EventHandler ICommand.CanExecuteChanged
    {
        add => System.Windows.Input.CommandManager.RequerySuggested += value;
        remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
    }

    void ICommand.Execute(object parameter)
    {
            var execute = FindExecutor(parameter);
            Parent.Execute(execute, parameter);
        }

    MethodInfo FindExecutor(object parameter)
        => Executes.Single(x => IsMatch(parameter, x.GetParameters()));

    static bool IsMatch(object parameter, ParameterInfo[] parameterInfos)
    {
            switch(parameterInfos.Length)
            {
                case 0:
                    return parameter == null;
                case 1:
                    return parameter != null && parameter.GetType().Is(parameterInfos[0].ParameterType);
                default:
                    true.ConditionalBreak();
                    return false;
            }
        }
}