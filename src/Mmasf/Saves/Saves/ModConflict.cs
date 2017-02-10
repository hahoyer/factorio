using System;
using System.Collections.Generic;
using System.Linq;
using ManageModsAndSavefiles.Mods;

namespace ManageModsAndSavefiles.Saves
{
    public abstract class ModConflict
    {
        public FileCluster Save;

        public sealed class RemovedMod : ModConflict
        {
            public ModDescription SaveMod;
        }

        public sealed class LegacyMod : ModConflict
        {
            public ModDescription SaveMod;
            public Version CurrentModVersion;
        }

        public sealed class AddedMod : ModConflict
        {
            public Mods.FileCluster CurrentMod;
        }
    }
}