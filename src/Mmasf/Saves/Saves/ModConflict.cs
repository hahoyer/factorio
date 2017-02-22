using System;
using System.Collections.Generic;
using System.Linq;
using ManageModsAndSavefiles.Mods;

namespace ManageModsAndSavefiles.Saves
{
    public sealed class ModConflict
    {
        public FileCluster Save;
        public ModDescription SaveMod;
        public ModDescription GameMod;
        public ModDescription Mod => GameMod ?? SaveMod;

    }
}