using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;


namespace ManageModsAndSavefiles
{
    public class Game : DumpableObject
    {
        internal class Item : EnumEx
        {
            public static readonly Item YellowScience = new Item();
            public static readonly Item CopperCable = new Item();
            public static readonly Item Battery = new Item();
            public static readonly Item ProcessingUnit = new Item();
            public static readonly Item SpeedModule = new Item();
            public static readonly Item CopperPlate = new Item();
            public static readonly Item IronPlate = new Item();
            public static readonly Item PlasticBar = new Item();
            public static readonly Item ElectronicCircuit = new Item();
            public static readonly Item AdvancedCircuit = new Item();
            public static readonly Item PetroleumGas = new Item();
            public static readonly Item Coal = new Item();
            public static readonly Item SulfuricAcid = new Item();
        }

        internal class Stack<TTarget> : DumpableObject
        {
            public Rational Count;
            public TTarget Target;

            protected override string GetNodeDump() => Count.Ceiling + "(" + Count + ") " + Target;
        }

        internal class Receipe : EnumEx
        {
            public static readonly Receipe YelloScience =
                new Receipe(14)
                    .Add(Item.YellowScience, -2)
                    .Add(Item.CopperCable, 30)
                    .Add(Item.Battery, 1)
                    .Add(Item.ProcessingUnit, 3)
                    .Add(Item.SpeedModule, 1);

            public static readonly Receipe CopperCable =
                new Receipe(new Rational(1, 2))
                    .Add(Item.CopperCable, -2)
                    .Add(Item.CopperPlate, 1);

            public static readonly Receipe ProcessingUnit =
                new Receipe(10)
                    .Add(Item.ProcessingUnit, -1)
                    .Add(Item.AdvancedCircuit, 2)
                    .Add(Item.ElectronicCircuit, 20)
                    .Add(Item.SulfuricAcid, 6);

            public static readonly Receipe SpeedModule =
                new Receipe(15)
                    .Add(Item.SpeedModule, -1)
                    .Add(Item.AdvancedCircuit, 5)
                    .Add(Item.ElectronicCircuit, 5);

            public static readonly Receipe ElectronicCircuit =
                new Receipe(new Rational(1, 2))
                    .Add(Item.ElectronicCircuit, -1)
                    .Add(Item.CopperCable, 3)
                    .Add(Item.IronPlate, 1);

            public static readonly Receipe AdvancedCircuit =
                new Receipe(6)
                    .Add(Item.AdvancedCircuit, -1)
                    .Add(Item.CopperCable, 4)
                    .Add(Item.ElectronicCircuit, 2)
                    .Add(Item.PlasticBar, 2);

            public static readonly Receipe PlasticBar =
                new Receipe(1)
                    .Add(Item.PetroleumGas, 20)
                    .Add(Item.Coal, 1)
                    .Add(Item.PlasticBar, -2);

            public static readonly Receipe Battery =
                new Receipe(5)
                    .Add(Item.Battery, -1)
                    .Add(Item.SulfuricAcid, 20)
                    .Add(Item.IronPlate, 1)
                    .Add(Item.CopperPlate, 1);

            readonly Rational SecondsPerCycle;
            internal Stack<Item>[] Items = { };

            Receipe(int secondsPerCycle)
                : this(new Rational(secondsPerCycle)) { }

            Receipe(Rational secondsPerCycle) { SecondsPerCycle = secondsPerCycle; }

            public static IEnumerable<Receipe> All => AllInstances<Receipe>();

            Receipe Add(Item item, int count)
            {
                var itemStack = new Stack<Item>
                {
                    Count = count / SecondsPerCycle,
                    Target = item
                };
                Items = Items.Concat(new[] {itemStack}).ToArray();
                return this;
            }

            public bool Creates(Item target) => Items.Any(i => i.Target == target && i.Count < 0);

            public Rational GetCountPerSecond(Item target)
                => Items.Top(i => i.Target == target, enableEmpty: false, enableMultiple: false).Count;
        }

        static Receipe[] GetCreatingReceipes(Item target)
            => Receipe
                .All
                .Where(r => r.Creates(target))
                .ToArray();


        public static void Test()
        {
            TestLuaConnection();
        }

        static void TestLuaConnection()
        {
            var instance = MmasfContext.Instance;
            instance.SystemConfiguration.Path.Directory.Run();
        }

        static void TestRationalCeiling()
        {
            Tracer.Assert(new Rational(4,3).Ceiling == 2 );
            Tracer.Assert(new Rational(-4,3).Ceiling == -1 );

        }

        static void TestSingle()
        {
            var factory = new Factory(Item.CopperCable);
            Tracer.Assert(factory.Receipes.Length == 1);
            Tracer.Assert(factory.Receipes.Top().Target == Receipe.CopperCable);
            Tracer.Assert(factory.Receipes.Top().Count == new Rational(1));
        }

        class Factory : DumpableObject
        {
            internal Stack<Receipe>[] Receipes = { };

            Stack<Item>[] Creating => ReceipeItems
                .Where(i => i.Count < 0).ToArray();

            Stack<Item>[] Consuming => ReceipeItems
                .Where(i => i.Count > 0).ToArray();

            Stack<Item>[] Bilance
                => Creating.Concat(Consuming)
                    .GroupBy(i => i.Target)
                    .Select(g =>
                        new Stack<Item>
                        {
                            Target = g.Key,
                            Count = g.Select(i => i.Count).Aggregate()
                        })
                    .Where(i => i.Count != 0)
                    .ToArray();

            public Factory(Item target, int targetCountPerSecond = 0)
            {
                var receipe = GetCreatingReceipes(target).Top(enableEmpty: false, enableMultiple: false);
                var count = targetCountPerSecond == 0
                    ? new Rational(1)
                    : targetCountPerSecond / receipe.GetCountPerSecond(target) * -1;
                Add(receipe, count);
                Replendish();
            }

            void Add(Receipe receipe, Rational count)
            {
                Receipes = Receipes
                    .Concat(new[] {new Stack<Receipe> {Target = receipe, Count = count}})
                    .GroupBy(i => i.Target)
                    .Select(g => new Stack<Receipe> {Target = g.Key, Count = g.Select(i => i.Count).Aggregate()})
                    .ToArray();
            }

            static IEnumerable<Stack<Item>> GetReceipeItems(Stack<Receipe> receipeStack)
            {
                return receipeStack
                    .Target
                    .Items
                    .Select(i => new Stack<Item> {Count = i.Count * receipeStack.Count, Target = i.Target});
            }

            IEnumerable<Stack<Item>> ReceipeItems => GetReceipeItems();

            IEnumerable<Stack<Item>> GetReceipeItems()
            {
                return Receipes
                    .SelectMany(GetReceipeItems)
                    .GroupBy(i => i.Target)
                    .SelectMany(g => GetItems(g, i => i < 0).Concat(GetItems(g, i => i > 0)));
            }

            static IEnumerable<Stack<Item>> GetItems(IGrouping<Item, Stack<Item>> target,
                Func<Rational, bool> predicate)
            {
                var targetKey = target.Key;
                var count = target.Select(i => i.Count).Where(predicate).Aggregate();
                if(count == null || count == 0)
                    yield break;

                yield return new Stack<Item>
                {
                    Target = targetKey,
                    Count = count
                };
            }

            void Replendish()
            {
                while(true)
                {
                    var next = Bilance
                        .Where(i => i.Count > 0)
                        .Where(i => GetCreatingReceipes(i.Target).Any())
                        .Top();
                    if(next == null)
                        return;

                    var receipe = GetCreatingReceipes(next.Target).Top();
                    Add(receipe, next.Count / receipe.GetCountPerSecond(next.Target) * -1);
                }
            }
        }
    }
}