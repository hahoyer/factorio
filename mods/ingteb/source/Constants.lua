local modName = "ingteb"
local result = {
    Key = {
        Main = modName .. "-main-key",
        Back = modName .. "-back-key",
        Fore = modName .. "-fore-key"
    },
    GuiStyle = {
        CenteredFlow = modName .. "-centered-flow"
    },
    ModName = modName,
    GlobalPrefix = modName .. "_" .. "GlobalPrefix",
    GraphicsPath = "__" .. modName .. "__/graphics/"
}

return result
