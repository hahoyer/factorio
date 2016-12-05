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

        public sealed class UpdatedMod : ModConflict
        {
            public ModDescription SaveMod;
            public Version ModVersion;
        }

        public sealed class AddedMod : ModConflict
        {
            public Mods.FileCluster Mod;
        }
    }
}