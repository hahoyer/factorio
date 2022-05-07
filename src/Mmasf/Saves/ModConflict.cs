using ManageModsAndSaveFiles.Mods;

namespace ManageModsAndSaveFiles.Saves;

public sealed class ModConflict
{
    public FileCluster Save;
    public ModDescription SaveMod;
    public ModDescription GameMod;
    public ModDescription Mod => GameMod ?? SaveMod;

    internal bool IsRelevant
    {
        get
        {
            if(GameMod == null)
                return SaveMod.IsSaveOnlyPossible != true;
            if(SaveMod == null)
                return GameMod.IsGameOnlyPossible != true;

            return !GameMod.IsCompatible(SaveMod.Version);
        }
    }

    public bool IsKnown
    {
        get
        {
            if(!Mod.HasConfiguration)
                return false;
            if(GameMod == null)
                return Mod.IsSaveOnlyPossible != null;
            if(SaveMod == null)
                return Mod.IsGameOnlyPossible != null;
            return false;
        }
    }
}