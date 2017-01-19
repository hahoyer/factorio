using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace MmasfUI
{
    sealed class Selection<T>
        where T :class
    {
        sealed class Item
        {
            internal readonly T Target;
            internal readonly ISelectionItemView ItemView;

            public Item(T target, ISelectionItemView itemView)
            {
                Target = target;
                ItemView = itemView;
            }
        }

        readonly List<Item> Items = new List<Item>();
        Item CurrentItem;

        public void Add(T target, int index, ISelectionItemView itemView)
        {
            while(Items.Count <= index)
                Items.Add(null);
            Items[index] = new Item(target, itemView);
        }

        internal T Current
        {
            get { return CurrentItem?.Target; }
            set
            {
                if(Current == value)
                    return;

                if(CurrentItem != null)
                    CurrentItem.ItemView.Selection = false;

                CurrentItem = value == null ? null : Items.Single(i => i.Target.Equals(value));

                if (CurrentItem != null)
                    CurrentItem.ItemView.Selection = true;
            }
        }

    }

    interface ISelectionItemView
    {
        bool Selection { set; }
    }
}