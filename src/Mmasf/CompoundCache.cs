using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;

namespace ManageModsAndSaveFiles
{
    sealed class CompoundCache<TValue> : CompoundCache
    {
        readonly ValueCache<TValue> Cache;

        public CompoundCache(Func<TValue> getValue, params CompoundCache[] dependsOn)
            : base(dependsOn) => Cache = new ValueCache<TValue>(getValue);

        public TValue Value => Cache.Value;

        public override bool IsValid {get => Cache.IsValid; set => Cache.IsValid = value;}

        public void OnChange()
        {
            foreach(var item in AllDependers)
                item.IsValid = false;
        }
    }

    abstract class CompoundCache : DumpableObject
    {
        readonly CompoundCache[] DependsOn;

        protected CompoundCache(CompoundCache[] dependsOn) => DependsOn = dependsOn;

        protected CompoundCache[] AllDependers
        {
            get
            {
                var result = DependsOn;
                while(true)
                {
                    var newResult = result
                        .SelectMany(item => item.DependsOn)
                        .Concat(result)
                        .Distinct()
                        .ToArray();
                    if(result.Length == newResult.Length)
                        return newResult;

                    result = newResult;
                }
            }
        }

        public abstract bool IsValid {get; set;}
    }
}