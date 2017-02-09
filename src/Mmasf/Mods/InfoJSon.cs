using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ManageModsAndSavefiles.Mods
{
    public sealed class InfoJSon : IEquatable<InfoJSon>
    {
        public bool Equals(InfoJSon other)
        {
            if(ReferenceEquals(null, other))
                return false;
            if(ReferenceEquals(this, other))
                return true;

            if(!string.Equals(Author, other.Author))
                return false;
            if(!string.Equals(Contact, other.Contact))
                return false;

            if(Dependencies != other.Dependencies)
            {
                if (Dependencies.Length != other.Dependencies.Length)
                    return false;
                if (Dependencies.Any(d => !other.Dependencies.Contains(d)))
                    return false;

            }

            if (!string.Equals(Description, other.Description))
                return false;
            if(!string.Equals(FactorioVersion, other.FactorioVersion))
                return false;
            if(!string.Equals(Homepage, other.Homepage))
                return false;
            if(!string.Equals(Name, other.Name))
                return false;
            if(!string.Equals(Title, other.Title))
                return false;
            return string.Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;

            var a = obj as InfoJSon;
            return a != null && Equals(a);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Author?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Contact?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Dependencies?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (FactorioVersion?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Homepage?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Title?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Version?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(InfoJSon left, InfoJSon right) => Equals(left, right);
        public static bool operator !=(InfoJSon left, InfoJSon right) => !Equals(left, right);

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