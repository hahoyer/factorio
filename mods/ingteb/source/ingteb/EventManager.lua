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
    if not global.Current.Gui or not global.Current.Gui.AppendForKey then
        global.Current.Gui = Dictionary:new{}
    end
    if not global.Current.PendingTranslation then
        global.Current.PendingTranslation = Dictionary:new{}
    end
end

function EventManager:ConfigureEvents()
    assert(Gui.Active.ingteb)
    if Gui.Active.Selector then

        self:SetHandler(
            Constants.Key.Fore, History.Current and self.OnSelectorForeOrBackClick or self.DoNothing
        )
        self:SetHandler(
            Constants.Key.Back, History.Current and self.OnSelectorForeOrBackClick or self.DoNothing
        )
        self:SetHandler(defines.events.on_gui_elem_changed, self.OnSelectorElementChanged)
        self:SetHandler(defines.events.on_gui_closed, self.OnSelectorClose)
        self:SetHandler(defines.events.on_tick)

    elseif Gui.Active.Presentator then

        self:SetHandler(
            Constants.Key.Fore,
                History.IsForePossible and self.OnPresentatorForeClick or self.DoNothing
        )
        self:SetHandler(
            Constants.Key.Back,
                History.IsBackPossible and self.OnPresentatorBackClick or self.DoNothing
        )
        self:SetHandler(defines.events.on_gui_elem_changed)
        self:SetHandler(defines.events.on_gui_closed, self.OnPresentatorClose)
        self:SetHandler(defines.events.on_tick)

    else

        self:SetHandler(
            Constants.Key.Fore, History.Current and self.OnSelectorForeOrBackClick or self.DoNothing
        )
        self:SetHandler(
            Constants.Key.Back, History.Current and self.OnSelectorForeOrBackClick or self.DoNothing
        )
        self:SetHandler(defines.events.on_gui_closed, self.DoNothing)

    end
    self:SetHandler(defines.events.on_gui_click, self.OnGuiClick)
end

function EventManager:OnSelectorForeOrBackClick(event)
    self.Player = event.player_index
    Gui:PresentTarget(self.Player, History.Current)
    self:ConfigureEvents()
end

function EventManager:OnPresentatorForeClick(event)
    self.Player = event.player_index
    History:Fore()
    Gui:PresentTarget(self.Player, History.Current)
    self:ConfigureEvents()
end

function EventManager:OnPresentatorBackClick(event)
    self.Player = event.player_index
    History:Back()
    Gui:PresentTarget(self.Player, History.Current)
    self:ConfigureEvents()
end

function EventManager:OnSelectorElementChanged(event)
    self.Player = event.player_index
    local target = Database:Get(event.element.elem_value)
    Gui:CloseSelector(self.Player)
    Gui:PresentTarget(self.Player, target)
    History:ResetTo(target)
    self:ConfigureEvents()
end

function EventManager:OnSelectorClose(event)
    self.Player = event.player_index
    Gui:CloseSelector(self.Player)
    self:ConfigureEvents()
end

function EventManager:OnPresentatorClose(event)
    self.Player = event.player_index
    Gui:ClosePresentator(self.Player)
    self:ConfigureEvents()
end

function EventManager:DoNothing(event)
    self.Player = event.player_index
    self:ConfigureEvents()
end

function EventManager:OnGuiClick(event)
    self.Player = event.player_index
    local target = Gui:OnGuiClick(self.Player, event)
    if target then History:AdvanceWith(target) end
    self:ConfigureEvents()
end

function EventManager:OnTickInitial()
    self:EnsureGlobal()
    Gui:EnsureMainButton()
    self:ConfigureEvents()
end

function EventManager:OnInit() Database:OnLoad() end

function EventManager:OnMainKey(event)
    self.Player = event.player_index
    local target = Gui:OnMainButtonPressed(self.Player)
    if target then History:AdvanceWith(target) end
    self:ConfigureEvents()
end

function EventManager:OnLoad()
    History = History:new()
    --    History = History:new(global.Current and global.Current.History) 
end

function EventManager:OnPlayerJoined(event) 
    self.Player = event.player_index 
    self:EnsureGlobal()
end

function EventManager:new(instance)
    if not instance then instance = {} end
    self:adopt(instance)

    self:properties{
        Player = {
            get = function() return global.Current.Player end,
            set = function(_, value)
                if value then
                    if not global.Current then global.Current = {} end
                    global.Current.Player --
                    = type(value) == "number" and game.players[value] --
                    or type(value) == "table" and value.object_name == "LuaPlayer" and value --
                          or assert()
                else
                    global.Current.Player = nil
                end
            end,
        },
    }

    instance:SetHandler("on_load", self.OnLoad)
    instance:SetHandler("on_init", self.OnInit)
    instance:SetHandler(defines.events.on_player_joined_game, self.OnPlayerJoined)
    instance:SetHandler(defines.events.on_tick, self.OnTickInitial, "initial")
    instance:SetHandler(Constants.Key.Main, self.OnMainKey)
    instance:SetHandler(defines.events.on_string_translated, Helper.CompleteTranslation)
    return instance
end

return EventManager

