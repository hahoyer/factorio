local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Helper = require("ingteb.Helper")
local Gui = require("ingteb.Gui")
local History = require("ingteb.History"):new()
local Database = require("ingteb.Database"):new()
local UI = require("core.UI")

State = {}
StateHandler = nil

local function EnsureGlobal()
    Database:OnLoad()
    if not global.Current then global.Current = {} end
    if not global.Current.Links then global.Current.Links = {} end
    if not global.Current.Location then global.Current.Location = {} end
    if not global.Current.Gui then global.Current.Gui = Dictionary:new{} end
end

local function OpenMainGui(target, setHistory)
    if not target then return end

    Helper.HideFrame()
    Gui.Main(target)
    StateHandler {mainPanel = true}
    if setHistory ~= false then History:HairCut(target) end
    return target
end

local function UpdateGui(list, target)
    local helperText = target.HelperText
    local number = target.NumberOnSprite
    local style = target.SpriteStyle or "slot_button"
    list:Select(
        function(guiElement)
            guiElement.tooltip = helperText
            guiElement.number = number
            guiElement.style = style
        end
    )
end

local function RefreshMainInventoryChanged()
    global.Current.Gui --
    :Where(function(_, target) return target.class_name == "Recipe" end) --
    :Select(UpdateGui) --
end

local function RefreshStackChanged() end

local function RefreshMainResearchChanged()
    global.Current.Gui --
    :Where(function(_, target) return target.class_name == "Technology" end) --
    :Select(UpdateGui) --
end

local function RefreshMain() OpenMainGui(History:GetCurrent(), false) end

function ForeNavigation() OpenMainGui(History:Fore(), false) end

function BackNavigation() OpenMainGui(History:Back(), false) end

function GetHandCraftingOrder(event, target)
    if (UI.IsMouseCode(event, "A-- l") --
    or UI.IsMouseCode(event, "A-- r") --
    or UI.IsMouseCode(event, "--S l")) --
    and target and target.class_name == "Recipe" and target.HandCrafter and target.NumberOnSprite then
        local amount = 0
        if event.shift then
            amount = global.Current.Player.get_craftable_count(target.Prototype.name)
        elseif event.button == defines.mouse_button_type.left then
            amount = 1
        elseif event.button == defines.mouse_button_type.right then
            amount = 5
        else
            return
        end
        return {count = amount, recipe = target.Prototype.name}
    end
end

function GetResearchOrder(event, target)
    if UI.IsMouseCode(event, "-C- l") --
    and target and target.class_name == "Technology" and target.IsReady --
    then return {Technology = target.Prototype} end
end

local function GuiClickForMain(event)
    global.Current.Player = game.players[event.player_index]
    local target = global.Current.Links and global.Current.Links[event.element.index]

    if UI.IsMouseCode(event, "--- l") and target and target.Item --
    then
        OpenMainGui(target.Item)
        return
    end

    local order = GetHandCraftingOrder(event, target)
    if order then
        global.Current.Player.begin_crafting(order)
        return
    end

    local order = GetResearchOrder(event, target)
    if order then
        global.Current.Player.force.add_research(order.Technology)
        return
    end

    --        assert()

end

local function GuiElementChangedForSelect(event)
    global.Current.Player = game.players[event.player_index]
    StateHandler {selectPanel = false}
    OpenMainGui(Database:Get(event.element.elem_value))
end

local function GuiClose()
    Helper.HideFrame()
    StateHandler {mainPanel = false}
end

local function MainForClose()
    Helper.HideFrame()
    StateHandler {mainPanel = false, selectPanel = false}
end

local function MainForOpen(event)
    EnsureGlobal()
    global.Current.Player = game.players[event.player_index]
    local target = Database:FindTarget()
    if not target then
        Gui.SelectTarget()
        StateHandler {selectPanel = true}
        return
    end

    OpenMainGui(target)
end

local function OnLoad()
    History:RemoveAll()
    --    History:Load(global.Current and global.Current.History) 
end

local function OnInit() Database:OnLoad() end

StateHandler = function(state)
    state.mainPanel = state.mainPanel == true
    state.selectPanel = state.selectPanel == true

    local handlers = Dictionary:new{}

    handlers[Constants.Key.Fore] = {ForeNavigation, state.mainPanel}

    handlers[Constants.Key.Back] = (state.mainPanel and {BackNavigation, state.mainPanel}) or --
    {RefreshMain, "reopen current"}

    handlers[Constants.Key.Main] = ((state.mainPanel or state.selectPanel)
                                       and {MainForClose, "close mode"}) or --
    {MainForOpen, "open mode"}

    handlers[defines.events.on_gui_click] = {GuiClickForMain, state.mainPanel}
    handlers[defines.events.on_gui_elem_changed] = {GuiElementChangedForSelect, state.selectPanel}
    handlers[defines.events.on_gui_closed] = {GuiClose, state.mainPanel or state.selectPanel}

    handlers[defines.events.on_player_main_inventory_changed] =
        {RefreshMainInventoryChanged, state.mainPanel}
    handlers[defines.events.on_player_cursor_stack_changed] = {RefreshStackChanged, state.mainPanel}
    handlers[defines.events.on_research_finished] = {RefreshMainResearchChanged, state.mainPanel}

    Helper.SetHandlers(handlers)

    global.Current.History = History:Save()
end

Helper.SetHandler(Constants.Key.Main, MainForOpen, "open mode")
Helper.SetHandler("on_load", OnLoad)
Helper.SetHandler("on_init", OnInit)
