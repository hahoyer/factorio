using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
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

            var method = types
                .SelectMany(t => t.GetMembers().Where(m => IsRelevant(m, identifier)))
                .Single();

            return new Command(this, (MethodInfo) method);
        }

        static bool IsRelevant(MemberInfo m, string identifier)
        {
            var commandAttribute = m.GetAttribute<CommandAttribute>(false);
            if(commandAttribute == null)
                return false;
            if(commandAttribute.Name != identifier)
                return false;
            var mm = m as MethodInfo;
            return mm != null && mm.ReturnType == typeof(void) && !mm.GetParameters().Any();
        }

        public bool CanExecute(MemberInfo method)
        {
            var target = ActiveObjects.FirstOrDefault(o => o.GetType().Is(method.DeclaringType));
            if(target == null)
                return false;

            if(method.GetAttribute<CanExecute>(true) == null)
                return true;

            NotImplementedMethod(method.Name);
            return false;
        }

        internal void Execute(MethodInfo method)
        {
            var target = ActiveObjects.First(o => o.GetType().Is(method.DeclaringType));

            method.Invoke(target, null);
        }

        internal void Activate(object target , bool setIt = true)
        {
            if(setIt)
                ActiveObjects.Insert(0, target);
            else
                ActiveObjects.Remove(target);

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }

    class CanExecute : Attribute
    {}
}