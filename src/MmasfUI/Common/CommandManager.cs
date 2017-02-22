using System;
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

        public CommandManager(string targetNamespace = null) { TargetNamespace = targetNamespace; }

        public ICommand ByName(string identifier)
        {
            var types = TypeNameExtender
                .Types
                .Where(t => TargetNamespace == null || TargetNamespace == t.Namespace)
                .ToArray();

            var flatExecute = types
                .SelectMany(t => t.GetMembers().Where(m => IsRelevant(m, identifier, 0)))
                .SingleOrDefault();

            var paramterizedExecute = types
                .SelectMany
                (t => t.GetMembers().Where(m => IsRelevant(m, identifier, 1)))
                .SingleOrDefault();

            var canExecute = (flatExecute ?? paramterizedExecute)?
                .DeclaringType?
                .GetProperties()
                .SingleOrDefault(p => IsRelevant(p, identifier));

            return new Command
                (this, (MethodInfo) flatExecute, (MethodInfo) paramterizedExecute, canExecute);
        }

        static bool IsRelevant(MemberInfo m, string identifier, int parameterCount)
        {
            var commandAttribute = m.GetAttribute<CommandAttribute>(false);
            if(commandAttribute == null)
                return false;
            if(commandAttribute.Name != identifier)
                return false;

            var mm = m as MethodInfo;
            return mm != null
                && mm.ReturnType == typeof(void)
                && mm.GetParameters().Length == parameterCount;
        }

        static bool IsRelevant(PropertyInfo p, string identifier)
        {
            var attribute = p.GetAttribute<CommandAttribute>(false);
            if(attribute == null)
                return false;
            if(attribute.Name != identifier)
                return false;

            return p.PropertyType == typeof(bool) && p.CanRead;
        }

        public bool CanExecute(MethodInfo execute, PropertyInfo canExecute)
        {
            var target = ActiveObjects.FirstOrDefault(o => o.GetType().Is(execute.DeclaringType));
            return target != null && (canExecute == null || (bool) canExecute.GetValue(target));
        }

        internal void Execute(MethodInfo method)
        {
            var target = ActiveObjects.First(o => o.GetType().Is(method.DeclaringType));
            method.Invoke(target, null);
        }

        internal void Execute(MethodInfo method, object parameter)
        {
            var target = ActiveObjects.First(o => o.GetType().Is(method.DeclaringType));
            method.Invoke(target, new[] {parameter});
        }

        internal void Activate(object target, bool setIt = true)
        {
            if(setIt)
                ActiveObjects.Insert(0, target);
            else
                ActiveObjects.Remove(target);

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}