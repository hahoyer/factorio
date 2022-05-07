using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI.Common
{
    sealed class CommandManager : DumpableObject
    {
        readonly string TargetNamespace;
        readonly List<object> ActiveObjects = new List<object>();

        public CommandManager(string targetNamespace = null) => TargetNamespace = targetNamespace;

        internal bool this[object target]
        {
            get => ActiveObjects.Contains(target);
            set
            {
                if(value)
                    ActiveObjects.Insert(0, target);
                else
                    ActiveObjects.Remove(target);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand ByName(string identifier)
        {
            var types = TypeNameExtender
                .Types
                .Where(t => TargetNamespace == null || TargetNamespace == t.Namespace)
                .ToArray();

            var executes = types
                .SelectMany(t => t.GetMembers().Where(m => IsRelevant(m, identifier)))
                .OfType<MethodInfo>()
                .ToArray();

            var canExecute = executes
                .Select(x => x.DeclaringType)
                .Distinct()
                .Single()
                .GetProperties()
                .SingleOrDefault(p => IsRelevant(p, identifier));

            return new Command(this, executes, canExecute);
        }

        static bool IsRelevant(MemberInfo m, string identifier)
        {
            var commandAttribute =
                    m
                        .GetAttributes<CommandAttribute>(false)
                        .SingleOrDefault(a => a.Name == identifier)
                ;
            if(commandAttribute == null)
                return false;

            var mm = m as MethodInfo;
            return mm != null && mm.ReturnType == typeof(void) && mm.GetParameters().Length <= 1;
        }

        static bool IsRelevant(PropertyInfo p, string identifier)
        {
            var attribute = p
                    .GetAttributes<CommandAttribute>(false)
                    .SingleOrDefault(a => a.Name == identifier)
                ;
            return attribute != null && p.PropertyType == typeof(bool) && p.CanRead;
        }

        public bool CanExecute(MethodInfo execute, PropertyInfo canExecute)
        {
            var target = ActiveObjects.FirstOrDefault(o => o.GetType().Is(execute.DeclaringType));
            return target != null && (canExecute == null || (bool)canExecute.GetValue(target));
        }

        internal void Execute(MethodInfo method, object parameter)
        {
            var target = ActiveObjects.First(o => o.GetType().Is(method.DeclaringType));
            method.Invoke(target, parameter == null? null : new[] { parameter });
        }
    }
}