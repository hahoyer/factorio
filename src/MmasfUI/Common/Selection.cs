using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using hw.DebugFormatter;
using ManageModsAndSavefiles;

namespace MmasfUI.Common
{
    abstract partial class Selection : DumpableObject
    {
        internal interface IAcceptor
        {
            bool IsSelected { set; }
        }

        internal interface IController
        {
            void RegisterSelectionTrigger(Action value);
        }

        sealed class Item
        {
            [DisableDump]
            internal readonly object Target;
            [DisableDump]
            internal readonly IAcceptor ItemView;

            public Item(object target, IAcceptor itemView)
            {
                Target = target;
                ItemView = itemView;
            }
        }

        readonly List<Item> Items = new List<Item>();
        Item CurrentItem;

        protected void Add(int index, object target, IAcceptor itemView)
        {
            while(Items.Count <= index)
                Items.Add(null);
            var item = new Item(target, itemView);
            Items[index] = item;
            (itemView as IController)?.RegisterSelectionTrigger(() => CurrentTarget = target);
        }

        protected void Add(object target, IController itemView)
        {
            itemView.RegisterSelectionTrigger(() => CurrentTarget = target);
        }

        internal object CurrentTarget
        {
            get { return CurrentItem?.Target; }
            set
            {
                if(CurrentTarget == value)
                    return;

                if(CurrentItem != null)
                    CurrentItem.ItemView.IsSelected = false;

                CurrentItem = value == null ? null : Items.Single(i => IsMatchingTarget(value, i));

                if(CurrentItem != null)
                    CurrentItem.ItemView.IsSelected = true;
            }
        }

        static bool IsMatchingTarget(object value, Item i)
        {
            var targetTarget = i.Target as IIdentified<string>;
            var valueTarget = value as IIdentified<string>;
            if(targetTarget == null || valueTarget == null)
                return i.Target.Equals(value);

            return valueTarget.Identifier == targetTarget.Identifier;
        }

        internal void RegisterKeyboardHandler(Window window) { window.KeyUp += GetKey; }

        void GetKey(object sender, KeyEventArgs e)
        {
            var index = GetIndex(e.Key);

            if(index == null)
                return;

            SetCurrentTarget(index.Value);
            e.Handled = true;
        }

        int? GetIndex(Key key)
        {
            switch(key)
            {
                case Key.Up:
                    return (CurrentItem == null ? Items.Count : Items.IndexOf(CurrentItem)) - 1;
                case Key.Down:
                    return (CurrentItem == null ? -1 : Items.IndexOf(CurrentItem)) + 1;
                case Key.Home:
                    return 0;
                case Key.End:
                    return Items.Count;
            }
            return null;
        }

        void SetCurrentTarget(int i)
        {
            CurrentTarget = Items.Any() ? Items[Math.Max(0, Math.Min(i, Items.Count - 1))].Target : null;
        }
    }

    sealed class Selection<T> : Selection
        where T : class
    {
        internal T Current { get { return (T) CurrentTarget; } set { CurrentTarget = value; } }
        internal void Add(int index, T target, IAcceptor itemView) => base.Add(index, target, itemView);
        internal void Add(T target, IController itemView) => base.Add(target, itemView);
    }
}