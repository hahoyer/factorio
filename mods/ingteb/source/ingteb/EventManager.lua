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
local core = {EventManager = require("core.EventManager")}

-- __DebugAdapter.breakpoint(mesg:LocalisedString)
-----------------------------------------------------------------------
EventManager = class:new("EventManager", core.EventManager)

function EventManager:EnsureGlobal()
    if not global.Current then global.Current = {} end
    if not global.Current.Links then global.Current.Links = {} end
    if not global.Current.Location then global.Current.Location = {} end
    if not global.Current.Gui or not global.Current.Gui.AppendForKey then global.Current.Gui = {} end
    if not global.Current.PendingTranslation then
        global.Current.PendingTranslation = Dictionary:new{}
    end
end

function EventManager:OnSelectorForeOrBackClick(event)
    self.Player = event.player_index
    Gui:PresentTarget(self.Player, History.Current)
end

function EventManager:OnPresentatorForeClick(event)
    self.Player = event.player_index
    History:Fore()
    Gui:PresentTarget(self.Player, History.Current)
end

function EventManager:OnPresentatorBackClick(event)
    self.Player = event.player_index
    History:Back()
    Gui:PresentTarget(self.Player, History.Current)
end

function EventManager:OnSelectorElementChanged(event)
    self.Player = event.player_index
    local target = Database:Get(event.element.name)
    if target then
        Gui:CloseSelector(self.Player)
        Gui:PresentTarget(self.Player, target)
        History:ResetTo(target)
    end
end

function EventManager:OnSelectorClose(event)
    self.Player = event.player_index
    Gui:CloseSelector(self.Player)
end

function EventManager:OnPresentatorClose(event)
    self.Player = event.player_index
    Gui:ClosePresentator(self.Player)
end

function EventManager:IsIngtebControl(element)
    if not element then return false end
    if element == Gui.Active.Selector then return true end
    if element == Gui.Active.Presentator then return true end
    if element == Gui.Active.ingteb then return true end
    return self:IsIngtebControl(element.parent)
end

function EventManager:DoNothing(event) self.Player = event.player_index end

function EventManager:OnGuiClick(event)
    self.Player = event.player_index
    if self:IsIngtebControl(event.element) then
        if Gui.Active.Selector then
            assert(release or event.element)
            self:OnSelectorElementChanged(event)
        elseif Gui.Active.ingteb and event.element == Gui.Active.ingteb then
            local target = Gui:OnMainButtonPressed(self.Player)
            if target then History:AdvanceWith(target) end
        else
            local target = Gui:OnGuiClick(self.Player, event)
            if target then History:AdvanceWith(target) end
        end
    end
end

function EventManager:OnTickInitial()
    self:EnsureGlobal()
    self:SetHandler(defines.events.on_tick)
end

function EventManager:OnMainKey(event)
    self:EnsureGlobal()
    self.Player = event.player_index
    local target = Gui:OnMainButtonPressed(self.Player)
    if target then History:AdvanceWith(target) end
end

function EventManager:OnLoad()
    Gui:EnsureMainButton()
    History = History:new()
    --    History = History:new(global.Current and global.Current.History) 
end

function EventManager:OnPlayerJoined(event) self.Player = event.player_index end

function EventManager:OnMainInventoryChanged() Helper.RefreshMainInventoryChanged(Database) end

function EventManager:OnStackChanged() Helper.RefreshStackChanged(Database) end

function EventManager:OnResearchFinished(event)
    Gui:OnResearchFinished(event.research)
end

function EventManager:OnForeClicked(event)
    if Gui.Active.Presentator then
        return History.IsForePossible and self:OnPresentatorForeClick(event)
    else
        return History.Current and self:OnSelectorForeOrBackClick(event)
    end
end

function EventManager:OnBackClicked(event)
    if Gui.Active.Presentator then
        return History.IsBackPossible and self:OnPresentatorBackClick(event)
    else
        return History.Current and self:OnSelectorForeOrBackClick(event)
    end
end

function EventManager:OnClose(event)
    if Gui.Active.Selector then
        self:OnSelectorClose(event)
    elseif Gui.Active.Presentator then
        self:OnPresentatorClose(event)
    end
end

function EventManager:new(instance)
    if not instance then instance = {} end
    self:adopt(instance)

    self:properties{
        Player = {
            get = function() return global.Current.Player end,
            set = function(_, value)
                self:EnsureGlobal()
                if value then
                    local acutalValue = --
                    type(value) == "number" and game.players[value] --
                    or type(value) == "table" and value.object_name == "LuaPlayer" and value --
                        or assert(release)
                    if acutalValue == global.Current.Player then return end
                    global.Current.Player = acutalValue
                    Gui:EnsureMainButton()
                else
                    global.Current.Player = nil
                end
            end,
        },
    }

    self = instance
    self:SetHandler("on_load", self.OnLoad)
    self:SetHandler(defines.events.on_player_joined_game, self.OnPlayerJoined)
    self:SetHandler(defines.events.on_player_created, self.OnPlayerJoined)
    self:SetHandler(defines.events.on_tick, self.OnTickInitial, "initial")
    self:SetHandler(Constants.Key.Main, self.OnMainKey)

    self:SetHandler(defines.events.on_player_main_inventory_changed, self.OnMainInventoryChanged)
    self:SetHandler(defines.events.on_player_cursor_stack_changed, self.OnStackChanged)
    self:SetHandler(defines.events.on_research_finished, self.OnResearchFinished)
    --self:SetHandler(defines.events.on_string_translated, Helper.CompleteTranslation)
    self:SetHandler(defines.events.on_gui_click, self.OnGuiClick)
    self:SetHandler(defines.events.on_gui_closed, self.OnClose)
    self:SetHandler(Constants.Key.Fore, self.OnForeClicked)
    self:SetHandler(Constants.Key.Back, self.OnBackClicked)

    return self
end

return EventManager

