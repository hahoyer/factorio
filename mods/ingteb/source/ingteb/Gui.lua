local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Database = require("ingteb.Database")
local UI = require("core.UI")
local Presentator = require("ingteb.Presentator")
local Selector = require("ingteb.Selector")

local Gui = {Active = {}}


function Gui:FindTarget(player)
    assert(player)
    assert(self.Active.ingteb)
    assert(not self.Active.Selector)
    assert(not self.Active.Presentator)

    local function get()
        local cursor = player.cursor_stack
        if cursor and cursor.valid and cursor.valid_for_read then
            return Database:GetItem(cursor.name)
        end
        local cursor = player.cursor_ghost
        if cursor then return Database:GetItem(cursor.name) end

        local cursor = player.selected
        if cursor then
            local result = Database:GetEntity(cursor.name)
            if result.IsResource then
                return result
            else
                return result.Item
            end
        end

        local cursor = player.opened
        if cursor then

            local t = player.opened_gui_type
            if t == defines.gui_type.custom then return end
            if t == defines.gui_type.entity then return self.Entities[cursor.name] end

            assert()
        end
        -- local cursor = global.Current.Player.entity_copy_source
        -- assert(not cursor)

    end

    local result = get()
    return result
end

function Gui:ScanActiveGui(player)
    self.Active.ingteb = player.gui.top.ingteb
    self.Active.Selector = player.gui.screen.Selector
    self.Active.Presentator = player.gui.screen.Presentator
end

function Gui:CloseSelector(player)
    Helper.OnClose("Selector", self.Active.Selector)
    player.gui.screen.Selector.destroy()
    self.Active.Selector = nil
end

function Gui:ClosePresentator(player)
    Helper.OnClose("Presentator", self.Active.Presentator)
    player.gui.screen.Presentator.destroy()
    self.Active.Presentator = nil
end

function Gui:SelectTarget(player, targets)
    Helper.ShowFrame(player, "Selector", function(frame) return Selector:new(frame, targets) end)
    self:ScanActiveGui(player)
end

function Gui:PresentTarget(player, target)
    assert(target.Prototype)
    Helper.ShowFrame(
        player, "Presentator", function(frame) return Presentator:new(frame, target) end
    )
    self:ScanActiveGui(player)
    return target
end

function Gui:OnMainButtonPressed(player)
    assert(self.Active.ingteb)
    assert(not self.Active.Selector or not self.Active.Presentator)

    if self.Active.Selector then
        self:CloseSelector(player)
    elseif self.Active.Presentator then
        self:ClosePresentator(player)
    else
        local target = self:FindTarget(player)
        if target then
            return self:PresentTarget(player, target)
        else
            self:SelectTarget(player)
        end
    end
end

function Gui:EnsureMainButton()
    local player = global.Current.Player -- todo: multiplayer
    if player.gui.top.ingteb == nil then
        assert(not self.Active.ingteb)

        global.Current.Player.gui.top.add {
            type = "sprite-button",
            name = "ingteb",
            sprite = "ingteb",
        }
    end
    self:ScanActiveGui(player)
end

function Gui:OnGuiClick(player, event)
    local element = event.element
    if element == Gui.Active.ingteb then
        return self:OnMainButtonPressed(player)
    elseif element == Gui.Active.Selector then
        return
    elseif element == Gui.Active.Presentator then
        return
    end

    if self.Active.Presentator then return self:OnGuiClickForPresentator(player, event) end
    if self.Active.Presentator then return self:OnGuiClickForPresentator(player, event) end
end

function Gui:UpdateTabOrder(tabOrder, dropIndex)
    local dropTabIndex = tabOrder[tonumber(dropIndex)]
    tabOrder:Remove(dropIndex)
    tabOrder:Append(dropTabIndex)
end

function Gui:OnGuiClickForPresentator(player, event)
    local target = Database:Get(global.Current.Links[event.element.index])
    if target and target.Prototype then
        if UI.IsMouseCode(event, "--- l") then return self:PresentTarget(player, target) end

        local order = target:GetHandCraftingOrder(event)
        if order then
            player.begin_crafting(order)
            return
        end

        local order = target:GetResearchOrder(event)
        if order then
            player.force.add_research(order.Technology)
            return
        end
        return 
    end
    
    local target = global.Current.Links[self.Active.Presentator.index]
    if target then 
        self:UpdateTabOrder(target.TabOrder, event.element.name)
        return self:PresentTarget(player, target)
    end
end

return Gui
