using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ManageModsAndSavefiles
{
    sealed class ModInfo
    {
        [DataMember(Name = "author")]
        public string Author;
        [DataMember(Name = "contact")]
        public string Contact;
        [DataMember(Name = "dependencies")]
        public string[] Dependencies;
        [DataMember(Name = "description")]
        public string Description;
        [DataMember(Name = "factorio_version")]
        public string FactorioVersion;
        [DataMember(Name = "homepage")]
        public string Homepage;
        [DataMember(Name = "name")]
        public string Name;
        [DataMember(Name = "title")]
        public string Title;
        [DataMember(Name = "version")]
        public string Version;
    }
}