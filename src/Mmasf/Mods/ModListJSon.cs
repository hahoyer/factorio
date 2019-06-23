using hw.DebugFormatter;
using Newtonsoft.Json;

namespace ManageModsAndSaveFiles.Mods
{
    sealed class ModListJSon : DumpableObject
    {
        internal sealed class Cell : DumpableObject
        {
            [JsonProperty(PropertyName = "name")]
            internal string Name;

            [JsonProperty(PropertyName = "enabled")]
            internal bool IsEnabled;
        }

        [JsonProperty(PropertyName = "mods")]
        internal Cell[] Cells;
    }
}