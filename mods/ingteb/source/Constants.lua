local modName = "ingteb"
local result = {
    Key = {
        Main = modName .. "-main-key",
        Back = modName .. "-back-key",
        Fore = modName .. "-fore-key"
    },
    GuiStyle = {
        CenteredFlow = modName .. "-centered-flow",
        ProperiesFlow = modName .. "-properties-flow",
        LightButton = modName .. "-light-button",
        UnButton = modName .. "-un-button"
    },
    ModName = modName,
    GlobalPrefix = modName .. "_" .. "GlobalPrefix",
    GraphicsPath = "__" .. modName .. "__/graphics/"
}

return result
