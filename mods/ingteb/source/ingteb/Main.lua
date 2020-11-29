local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Helper = require("ingteb.Helper")
local Gui = require("ingteb.Gui")
local History = require("ingteb.History"):new()
local Database = require("ingteb.Database")
local UI = require("core.UI")
local class = require("core.class")

State = {
    EventDefinesByIndex = Dictionary:new(defines.events):ToDictionary(
        function(value, key) return {Key = value, Value = key} end
    ):ToArray(),

}

function State:Watch(handler, eventId)
    return function(...)
        self:Enter(eventId)
        local result = handler(...)
        self:Leave(eventId)
        return result
    end
end

function State:SetHandler(eventId, handler, register)
    if not handler then register = false end
    if register == nil then register = true end

    local name = type(eventId) == "number" and self.EventDefinesByIndex[eventId] or eventId

    State[name] = "activating..." .. tostring(register)

    if register == false then handler = nil end
    local watchedEvent = handler and self:Watch(handler, name) or nil

    local eventRegistrar = event[eventId]
    if eventRegistrar then
        eventRegistrar(watchedEvent)
    else
        event.register(eventId, watchedEvent)
    end

    State[name] = register
end

function State:SetHandlers(list)
    list:Select(function(command, key) self:SetHandler(key, command[1], command[2]) end)
end

function State:Enter(name)
    assert(
        not self.Active --
        or name == "on_gui_closed" --
    )
    self.Active = {name, self.Active}
end

function State:Leave(name)
    assert(self.Active[1] == name)
    self.Active = self.Active[2]
end

local function EnsureGlobal()
    if not global.Current then global.Current = {} end
    if not global.Current.Links then global.Current.Links = {} end
    if not global.Current.Location then global.Current.Location = {} end
    if not global.Current.Gui or not global.Current.Gui.AppendForKey then
        global.Current.Gui = Dictionary:new{}
    end
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

local function HideGuiAndResetData()
    if global.Current.Frame then
        log("\n>>>>--- HideGuiAndResetData")
        global.Current.Location[global.Current.Frame.name] = global.Current.Frame.location
        global.Current.Frame.destroy()
        game.tick_paused = false
        global.Current.Frame = nil
        global.Current.Links = {}
        log("!!!!--- Links dropped")
        global.Current.Gui = Dictionary:new{}
        global.Current.Player.opened = nil
        log("<<<<------ HideGuiAndResetData\n")
    end
end

local function OpenMainGui(target, setHistory)
    if not target then return end
    log(
        "\n>>>>--- OpenMainGui target = " .. target.object_name .. ":" .. target.Name
            .. " setHistory = " .. tostring(setHistory)
    )
    assert(target.Prototype)
    assert(type(setHistory or false) == "boolean")

    ConfigureEmptyCloseEventState()

    log(">>>>--- Link collection")
    Gui.Main(target)
    log("<<<<--- Link collection")
    ConfigureMainPanelOpenState()
    if setHistory ~= false then History:HairCut(target) end
    log("<<<<------ OpenMainGui\n")
    return target
end

local function RefreshMain()
    log("\n>>>>--- RefreshMain")
    OpenMainGui(History:GetCurrent(), false)
    log("<<<<------ RefreshMain\n")
end

local function ForeNavigation()
    log("\n>>>>--- ForeNavigation")
    OpenMainGui(History:Fore(), false)
    log("<<<<------ ForeNavigation\n")
end

local function BackNavigation()
    log("\n>>>>--- BackNavigation")
    OpenMainGui(History:Back(), false)
    log("<<<<------ BackNavigation\n")
end

local function GetHandCraftingOrder(event, target)
    if (UI.IsMouseCode(event, "A-- l") --
    or UI.IsMouseCode(event, "A-- r") --
    or UI.IsMouseCode(event, "--S l")) --
    and target and target.object_name == "Recipe" and target.HandCrafter and target.NumberOnSprite then
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

local function GetResearchOrder(event, target)
    if UI.IsMouseCode(event, "-C- l") --
    and target and target.object_name == "Technology" and target.IsReady --
    then return {Technology = target.Prototype} end
end

local function OpenOrCloseMainGui()
    if global.Current.Frame then
        HideGuiAndResetData()
        ConfigureNoneOpenState()
    else
        EnsureGlobal()
        local target = Database:FindTarget()
        if not target then
            Gui.SelectTarget()
            ConfigureSelectionPanelOpenState()
            return
        end

        OpenMainGui(target)
    end
end

local function CheckForOpenOrClose(event)
    if global.Current.MainButtonIndex == event.element.index then
        OpenOrCloseMainGui()
        return true
    end
end

local function OnGuiClick(event)
    log("\n>>>>--- OnGuiClick " .. serpent.block(event))
    global.Current.Player = game.players[event.player_index]
    if (CheckForOpenOrClose(event)) then return end
    assert()
    log("<<<<--- OnGuiClick\n")
end

local function OnGuiClickForSelect(event)
    log("\n>>>>--- OnGuiClickForSelect")
    global.Current.Player = game.players[event.player_index]
    log("<<<<--- OnGuiClickForSelect\n")
end

local function OnGuiClickForMain(event)
    log("\n>>>>--- OnGuiClickForMain")
    global.Current.Player = game.players[event.player_index]
    local target = global.Current.Links and global.Current.Links[event.element.index]

    if (CheckForOpenOrClose(event)) then return end

    if target and UI.IsMouseCode(event, "--- l") then
        if target.Prototype then
            OpenMainGui(target)
            log("<<<<--- OnGuiClickForMain IsMouseCode\n")
            return
        end
        __DebugAdapter.breakpoint()
    end

    local order = GetHandCraftingOrder(event, target)
    if order then
        global.Current.Player.begin_crafting(order)
        log("<<<<--- OnGuiClickForMain GetHandCraftingOrder\n")
        return
    end

    local order = GetResearchOrder(event, target)
    if order then
        global.Current.Player.force.add_research(order.Technology)
        log("<<<<--- OnGuiClickForMain GetResearchOrder\n")
        return
    end

    log("<<<<--- OnGuiClickForMain\n")
end

local function sCloseSelectionGui()
    log("\n>>>>--- CloseSelectionGui")
    ConfigureNoneOpenState()
    global.Current.Player.opened = nil
    log("<<<<--- CloseSelectionGui\n")
end

local function debugElementDump(element) return element and element.name or element end

local function OnGuiElementChangedForSelect(event)
    log("\n>>>>--- OnGuiElementChangedForSelect " .. debugElementDump(event.element))
    global.Current.Player = game.players[event.player_index]
    local target = event.element.elem_value
    OpenMainGui(Database:Get(target))
    log("<<<<--- OnGuiElementChangedForSelect\n")
end

local function OnGuiClose(event)
    log("\n>>>>--- OnGuiClose " .. debugElementDump(event.element))
    if event.element then
        if event.element == global.Current.Frame then
            log("!!!--- OnGuiClose: element match")
            HideGuiAndResetData()
            ConfigureNoneOpenState()
        end
    end
    log("<<<<--- OnGuiClose\n")
end

local function DoDestroyIt(event)
    log("\n>>>>--- DoDestroyIt " .. debugElementDump(event.element))
    event.element.destroy()
    log("<<<<--- DoDestroyIt\n")
end

local function OnResearchFinished(event)
    log("\n>>>>--- OnResearchFinished " .. debugElementDump(event.element))
    Database:RefreshTechnology(event.research)
    Helper.RefreshMainResearchChanged()
    log("<<<<--- OnResearchFinished\n")
end

local function OnMainKey(event)
    log("\n>>>>--- OnMainKey")
    global.Current.Player = game.players[event.player_index]
    OpenOrCloseMainGui()
    log("<<<<--- OnMainKey\n")
end

local function OnLoad()
    log("\n>>>>--- OnLoad")
    History:RemoveAll()
    --    History:Load(global.Current and global.Current.History) 
    log("<<<<--- OnLoad\n")
end

local function OnInit()
    log("\n>>>>--- OnInit")
    Database:OnLoad()
    log("<<<<--- OnInit\n")
end

local function OnTick()
    log("\n>>>>--- OnTick")

    EnsureMainButton()
    ConfigureMainPanelOpenState()
    log("<<<<--- OnTick\n")
end

function ConfigureEmptyCloseEventState()
    log("!!!!--- ConfigureNoCloseEventState")
    local handlers = Dictionary:new{}

    handlers[defines.events.on_gui_closed] = {DoDestroyIt}

    State:SetHandlers(handlers)

    global.Current.History = History:Save()
    -- log("State = " .. serpent.block(State))
end

ConfigureNoneOpenState = function()
    log("!!!!--- ConfigureNoneOpenState")
    local handlers = Dictionary:new{}
    handlers[Constants.Key.Fore] = {RefreshMain}
    handlers[Constants.Key.Back] = {RefreshMain}
    handlers[defines.events.on_gui_click] = {OnGuiClick}
    handlers[defines.events.on_gui_elem_changed] = {}
    handlers[defines.events.on_gui_closed] = {}
    handlers[defines.events.on_player_main_inventory_changed] = {}
    handlers[defines.events.on_player_cursor_stack_changed] = {}
    handlers[defines.events.on_research_finished] = {}
    handlers[defines.events.on_tick] = {}

    State:SetHandlers(handlers)

    global.Current.History = History:Save()
    -- log("State = " .. serpent.block(State))
end

ConfigureSelectionPanelOpenState = function()
    log("!!!!--- ConfigureSelectionPanelOpenState")
    local handlers = Dictionary:new{}
    handlers[Constants.Key.Fore] = {RefreshMain}
    handlers[Constants.Key.Back] = {RefreshMain}
    handlers[defines.events.on_gui_click] = {OnGuiClickForSelect}
    handlers[defines.events.on_gui_elem_changed] = {OnGuiElementChangedForSelect}
    handlers[defines.events.on_gui_closed] = {OnGuiClose}
    handlers[defines.events.on_player_main_inventory_changed] = {}
    handlers[defines.events.on_player_cursor_stack_changed] = {}
    handlers[defines.events.on_research_finished] = {}
    handlers[defines.events.on_tick] = {}

    State:SetHandlers(handlers)

    global.Current.History = History:Save()
    --    log("State = " .. serpent.block(State))
end

ConfigureMainPanelOpenState = function()
    log("!!!!--- ConfigureMainPanelOpenState")
    local handlers = Dictionary:new{}
    handlers[Constants.Key.Fore] = {ForeNavigation}
    handlers[Constants.Key.Back] = {BackNavigation}
    handlers[defines.events.on_gui_click] = {OnGuiClickForMain}
    handlers[defines.events.on_gui_elem_changed] = {}
    handlers[defines.events.on_gui_closed] = {OnGuiClose}
    handlers[defines.events.on_player_main_inventory_changed] = {Helper.RefreshMainInventoryChanged}
    handlers[defines.events.on_player_cursor_stack_changed] = {Helper.RefreshStackChanged}
    handlers[defines.events.on_research_finished] = {OnResearchFinished}
    handlers[defines.events.on_tick] = {}

    State:SetHandlers(handlers)

    global.Current.History = History:Save()
    -- log("State = " .. serpent.block(State))
end

State:SetHandler("on_load", OnLoad)
State:SetHandler("on_init", OnInit)
State:SetHandler(defines.events.on_tick, OnTick)
State:SetHandler(Constants.Key.Main, OnMainKey)
State:SetHandler(defines.events.on_string_translated, Helper.CompleteTranslation)

-- __DebugAdapter.breakpoint(mesg:LocalisedString)
