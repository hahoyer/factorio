local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Helper = require("ingteb.Helper")
local Gui = require("ingteb.Gui")
local History = require("ingteb.History")
local class = require("core.class")
local core = {EventManager = require("core.EventManager")}

-- __DebugAdapter.breakpoint(mesg:LocalisedString)
-----------------------------------------------------------------------

local EventManager = class:new("EventManager", core.EventManager)

function EventManager:OnSelectorForeOrBackClick(event)
    self.Player = event.player_index
    Gui:PresentTargetFromCommonKey(self.Player, global.History.Current)
end

function EventManager:OnPresentatorForeClick(event)
    self.Player = event.player_index
    global.History:Fore()
    Gui:PresentTargetFromCommonKey(self.Player, global.History.Current)
end

function EventManager:OnPresentatorBackClick(event)
    self.Player = event.player_index
    global.History:Back()
    Gui:PresentTargetFromCommonKey(self.Player, global.History.Current)
end

function EventManager:OnSelectorElementChanged(event)
    self.Player = event.player_index
    log("event.element.name = " .. tostring(event.element.name))
    local target = Gui:PresentSelected(self.Player, event.element.name)
    if target then global.History:ResetTo(target) end
end

function EventManager:OnSelectorClose(event)
    self.Player = event.player_index
    Gui:CloseSelector(self.Player)
end

function EventManager:OnPresentatorClose(event)
    self.Player = event.player_index
    Gui:ClosePresentator(self.Player)
end

function EventManager:GetIngtebControl(element)
    if not element then return element end
    if element == Gui.Active.Selector then return element end
    if element == Gui.Active.Presentator then return element end
    if element == Gui.Active.ingteb then return element end
    return self:GetIngtebControl(element.parent)
end

function EventManager:DoNothing(event) self.Player = event.player_index end

function EventManager:OnGuiClick(event)
    self.Player = event.player_index
    local active = self:GetIngtebControl(event.element)
    if active then
        if active == Gui.Active.ingteb then
            local target = Gui:OnMainButtonPressed(self.Player)
            if target then global.History:ResetTo(target) end
        elseif active == Gui.Active.Selector then
            if event.element == active then return end
            assert(release or event.element)
            self:OnSelectorElementChanged(event)
        elseif active == Gui.Active.Presentator then
            local target = Gui:OnGuiClick(self.Player, event)
            if target then global.History:AdvanceWith(target) end
        end
    end
end

function EventManager:OnGuiMoved(event)
    self.Player = event.player_index
    local active = self:GetIngtebControl(event.element)
    if active and active == event.element then
        if active == Gui.Active.Selector then
            global.Location.Selector = active.location
        elseif active == Gui.Active.Presentator then
            global.Location.Presentator = active.location
        end
    end

end

function EventManager:OnTickInitial()
    Gui:EnsureMainButton()
    self:SetHandler(defines.events.on_tick)
end

function EventManager:OnMainKey(event)
    self.Player = event.player_index
    local target = Gui:OnMainButtonPressed(self.Player)
    if target then global.History:ResetTo(target) end
end

function EventManager:OnPlayerJoined(event) 
    self.Player = event.player_index 
    Gui:EnsureMainButton(self.Player)
end

function EventManager:OnMainInventoryChanged() Gui:OnMainInventoryChanged() end

function EventManager:OnStackChanged() Gui:OnStackChanged() end

function EventManager:OnResearchFinished(event) Gui:OnResearchFinished(event.research) end

function EventManager:OnForeClicked(event)
    if Gui.Active.Presentator then
        if global.History.IsForePossible then self:OnPresentatorForeClick(event) end
    else
        if global.History.Current then self:OnSelectorForeOrBackClick(event) end
    end
end

function EventManager:OnBackClicked(event)
    if Gui.Active.Presentator then
        if global.History.IsBackPossible then self:OnPresentatorBackClick(event) end
    else
        if global.History.Current then self:OnSelectorForeOrBackClick(event) end
    end
end

function EventManager:OnClose(event)
    if Gui.Active.Selector then
        self:OnSelectorClose(event)
    elseif Gui.Active.Presentator then
        self:OnPresentatorClose(event)
    end
end

function EventManager:OnLoad()
    History:adopt(global.History)
    global.History:Log("OnLoad")
end

function EventManager:OnInitialise()
    global = {Links = {}, Location = {}, History = History:new()}
    Gui:EnsureMainButton()
end

function EventManager:new()
    local instance = core.EventManager:new()
    self:adopt(instance)

    self = instance
    self:SetHandler("on_init", self.OnInitialise)
    self:SetHandler("on_load", self.OnLoad)
    self:SetHandler(defines.events.on_player_joined_game, self.OnPlayerJoined)
    self:SetHandler(defines.events.on_player_created, self.OnPlayerJoined)
    self:SetHandler(defines.events.on_tick, self.OnTickInitial, "initial")
    self:SetHandler(Constants.Key.Main, self.OnMainKey)

    self:SetHandler(defines.events.on_player_main_inventory_changed, self.OnMainInventoryChanged)
    self:SetHandler(defines.events.on_player_cursor_stack_changed, self.OnStackChanged)
    self:SetHandler(defines.events.on_research_finished, self.OnResearchFinished)
    self:SetHandler(defines.events.on_gui_location_changed, self.OnGuiMoved)
    self:SetHandler(defines.events.on_gui_click, self.OnGuiClick)
    self:SetHandler(defines.events.on_gui_closed, self.OnClose)
    self:SetHandler(Constants.Key.Fore, self.OnForeClicked)
    self:SetHandler(Constants.Key.Back, self.OnBackClicked)

    return self
end

return EventManager

