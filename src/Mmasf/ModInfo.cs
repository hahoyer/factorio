using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ManageModsAndSavefiles
{
    sealed class ModInfo
    {
        [JsonProperty(PropertyName = "author")]
        internal string Author;
        [JsonProperty(PropertyName = "contact")]
        internal string Contact;
        [JsonProperty(PropertyName = "dependencies")]
        internal string[] Dependencies;
        [JsonProperty(PropertyName = "description")]
        internal string Description;
        [JsonProperty(PropertyName = "factorio_version")]
        internal string FactorioVersion;
        [JsonProperty(PropertyName = "homepage")]
        internal string Homepage;
        [JsonProperty(PropertyName = "name")]
        internal string Name;
        [JsonProperty(PropertyName = "title")]
        internal string Title;
        [JsonProperty(PropertyName = "version")]
        internal string Version;
    }
}