local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Data = require("ingteb.Data")
local Helper = require("ingteb.Helper")
local Gui = require("ingteb.Gui")
local History = require("ingteb.History"):new()

local s = 1 --/w
local s = 1 --/w

State = {}
StateHandler = nil

local function EnsureGlobal()
    if not global.Current then
        global.Current = {}
    end

    if not global.Current.Links then
        global.Current.Links = {}
    end

    if not global.Current.Location then
        global.Current.Location = {}
    end
end

local function OpenMainGui(target, setHistory)
    if target then
        local data = Data.Get(target)
        if data then
            Helper.HideFrame()
            Gui.Main(data)
            StateHandler {mainPanel = true}
            if setHistory ~= false then History:HairCut(target) end
            return target
        end
    end
end

function ForeNavigation()
    OpenMainGui(History:Fore(), false)
end

function BackNavigation()
    OpenMainGui(History:Back(), false)
end

local function GuiClickForMain(event)
    global.Current.Player = game.players[event.player_index]
    local target = global.Current.Links and global.Current.Links[event.element.index]
    if target.name == "automation" then
        local a = b
    end

    OpenMainGui(target)
end

local function GuiElementChangedForSelect(event)
    global.Current.Player = game.players[event.player_index]
    StateHandler {selectPanel = false}
    OpenMainGui(event.element.elem_value)
end

local function GuiClose()
    Helper.HideFrame()
    StateHandler {mainPanel = false}
end

local function MainForClose()
    Helper.HideFrame()
    StateHandler {mainPanel = false, selectPanel = false}
    History:RemoveAll()
end

local function MainForOpen(event)
    EnsureGlobal()
    global.Current.Player = game.players[event.player_index]
    local target = Data.FindTarget()
    if not target then
        Gui.SelectTarget()
        StateHandler {selectPanel = true}
        return
    end

    OpenMainGui(target)
end

local function Load()
    History:Load(global.Current and global.Current.History)
end

StateHandler = function(state)
    state.mainPanel = state.mainPanel == true
    state.selectPanel = state.selectPanel == true

    local handlers = Dictionary:new {}

    handlers[Constants.Key.Fore] = {ForeNavigation, state.mainPanel}
    handlers[Constants.Key.Back] = {BackNavigation, state.mainPanel}

    handlers[Constants.Key.Main] =
        ((state.mainPanel or state.selectPane) and {MainForClose, "close mode"}) or {MainForOpen, "open mode"}

    handlers["on_gui_click"] = {GuiClickForMain, state.mainPanel}
    handlers["on_gui_elem_changed"] = {GuiElementChangedForSelect, state.selectPanel}
    handlers["on_gui_closed"] = {GuiClose, state.mainPanel or state.selectPanel}

    Helper.SetHandlers(handlers)
    global.Current.History = History:Save()
end

Helper.SetHandler(Constants.Key.Main, MainForOpen, "open mode")
Helper.SetHandler("on_load", Load)
