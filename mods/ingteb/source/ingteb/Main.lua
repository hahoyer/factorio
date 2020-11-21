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
    if not global.Current then global.Current = {} end
    if not global.Current.Links then global.Current.Links = {} end
    if not global.Current.Location then global.Current.Location = {} end
    if not global.Current.Gui then global.Current.Gui = Dictionary:new{} end
    if not global.Current.PendingTranslation then
        global.Current.PendingTranslation = Dictionary:new{}
    end
end

local function EnsureMainButton()
    EnsureGlobal()
    if global.Current.Player.gui.top.ingteb == nil then
        local mainButton = global.Current.Player.gui.top.add {
            type = "sprite-button",
            name = "ingteb",
            sprite = "ingteb",
        }
        global.Current.MainButtonIndex = mainButton.index
    end
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

local function RefreshDescription(this)
    global.Current.Gui --
    :Where(function(_, target) return target == this end) --
    :Select(UpdateGui) --
end

local function OnStringTranslated(event)
    local target = event.localised_string
    local pendingList = global.Current.PendingTranslation[target[1]]
    Array:new(pendingList) --
    :Select(
        function(pending, index)
            if Helper.DeepEqual(pending.Key, target) then
                table.remove(pendingList, index)
                if #pendingList == 0 then
                    global.Current.PendingTranslation[target[1]] = nil
                end

                if event.translated then
                    local thing = pending.Value
                    thing.HasLocalisedDescriptionPending = false
                    RefreshDescription(thing)
                end
                return
            end
        end
    ) --
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

local function OpenMainGuiForNewItem()
    local target = Database:FindTarget()
    if not target then
        Gui.SelectTarget()
        StateHandler {selectPanel = true}
        return
    end

    OpenMainGui(target)
end

local function MainForClose()
    Helper.HideFrame()
    StateHandler {mainPanel = false, selectPanel = false}
end

local function MainForOpen()
    EnsureGlobal()
    Database:OnLoad()
    OpenMainGuiForNewItem()
end

local function Main()
    if global.Current.Frame then
        MainForClose()
    else
        MainForOpen()
    end
end

local function GuiClick(event)
    global.Current.Player = game.players[event.player_index]
    if global.Current.MainButtonIndex == event.element.index then return Main() end

    --        assert()

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

    GuiClick(event)
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

local function OnMainKey(event)
    global.Current.Player = game.players[event.player_index]
    Main()
end

local function OnLoad()
    History:RemoveAll()
    --    History:Load(global.Current and global.Current.History) 
end

local function OnInit() Database:OnLoad() end

local function OnTick()
    EnsureMainButton()
    StateHandler {mainButton = true}
end

StateHandler = function(state)
    state.mainPanel = state.mainPanel == true
    state.selectPanel = state.selectPanel == true
    state.mainButton = state.mainButton == true

    local handlers = Dictionary:new{}

    handlers[Constants.Key.Fore] = {ForeNavigation, state.mainPanel}

    handlers[Constants.Key.Back] = (state.mainPanel and {BackNavigation, state.mainPanel}) or --
    {RefreshMain, "reopen current"}

    handlers[defines.events.on_gui_click] = --
    (state.mainPanel and {GuiClickForMain, state.mainPanel}) or --
    {GuiClick, "outside"}

    handlers[defines.events.on_gui_elem_changed] = {GuiElementChangedForSelect, state.selectPanel}
    handlers[defines.events.on_gui_closed] = {GuiClose, state.mainPanel or state.selectPanel}

    handlers[defines.events.on_player_main_inventory_changed] =
        {RefreshMainInventoryChanged, state.mainPanel}
    handlers[defines.events.on_player_cursor_stack_changed] = {RefreshStackChanged, state.mainPanel}
    handlers[defines.events.on_research_finished] = {RefreshMainResearchChanged, state.mainPanel}
    handlers[defines.events.on_tick] = {}

    Helper.SetHandlers(handlers)

    global.Current.History = History:Save()
end

Helper.SetHandler(Constants.Key.Main, MainForOpen, "open mode")
Helper.SetHandler("on_load", OnLoad)
Helper.SetHandler("on_init", OnInit)
Helper.SetHandler(defines.events.on_tick, OnTick)
Helper.SetHandler(Constants.Key.Main, OnMainKey)
Helper.SetHandler(defines.events.on_string_translated, OnStringTranslated)
