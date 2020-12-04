local mod_gui = require("mod-gui")
local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local UI = require("core.UI")
local Presentator = require("ingteb.Presentator")
local Selector = require("ingteb.Selector")
local Database = require("ingteb.Database")

local Gui = {Active = {}}

function Gui:EnsureDatabase() self.Database = Database:Ensure() end

function Gui:GetRecipeData(recipePrototype, result)
    if recipePrototype then
        local recipe = self.Database:GetRecipe(nil, recipePrototype)
        result.recipe[recipePrototype.name] = true
        local inoutItems = recipe.Input:Concat(recipe.Output) --
        inoutItems:Select(function(stack) result.items[stack.Goods.Name] = true end)
    end
end

function Gui:GetInventoryData(inventory, result)
    if inventory then
        for index = 1, #inventory do
            local stack = inventory[index]
            if stack.valid_for_read then result.items[stack.prototype.name] = true end
        end
    end
end

function Gui:FindTargets(player)
    self:EnsureDatabase()
    assert(release or self.Active.ingteb)
    assert(release or not self.Active.Selector)
    assert(release or not self.Active.Presentator)

    local cursor = player.cursor_stack
    if cursor and cursor.valid and cursor.valid_for_read then
        return {self.Database:GetItem(cursor.name)}
    end

    local cursor = player.cursor_ghost
    if cursor then return {self.Database:GetItem(cursor.name)} end

    local cursor = player.selected
    if cursor then
        local result = self.Database:GetEntity(cursor.name)
        if result.IsResource then
            return {result}
        else
            return {result.Item}
        end
    end

    local cursor = player.opened
    if cursor then

        local t = player.opened_gui_type
        if t == defines.gui_type.custom then assert(release) end
        if t == defines.gui_type.entity then
            assert(release or cursor.object_name == "LuaEntity")

            local inventories = Dictionary:new(defines.inventory) --
            :Select(
                function(_, name)
                    local inventory = cursor.get_inventory(defines.inventory[name])
                    return inventory and #inventory or 0
                end
            ) --
            :Where(function(count) return count > 0 end)

            local results = {
                items = Dictionary:new{},
                recipes = Dictionary:new{},
                enities = Dictionary:new{},
                fuelCategory = Dictionary:new{},
            }

            results.items[cursor.name] = true
            if cursor.type == "container" then
                Gui:GetInventoryData(cursor.get_inventory(defines.inventory.item_main), results)
            elseif cursor.type == "assembling-machine" then
                Gui:GetRecipeData(cursor.get_recipe(), results)
            elseif cursor.type == "lab" then
                Gui:GetInventoryData(cursor.get_inventory(defines.inventory.lab_input), results)
                Gui:GetInventoryData(cursor.get_inventory(defines.inventory.lab_modules), results)
                Gui:GetRecipeData(player.force.current_research, results)
            elseif cursor.type == "mining-drill" then
                results.enities[cursor.mining_target.name] = true
                if cursor.burner and cursor.burner.fuel_categories then
                    for category, _ in pairs(cursor.burner.fuel_categories) do
                        results.fuelCategory[category] = true
                    end
                end
                Gui:GetInventoryData(cursor.get_inventory(defines.inventory.fuel))
                Gui:GetInventoryData(cursor.get_inventory(defines.inventory.item_main))
                Gui:GetInventoryData(cursor.get_inventory(defines.inventory.mining_drill_modules))
            else
                assert(release)
            end

            local result = Array:new{}
            results.fuelCategory:Select(
                function(_, name) result:Append(self.Database:GetFuelCategory(name)) end
            )
            results.enities:Select(
                function(_, name) result:Append(self.Database:GetEntity(name)) end
            )
            results.recipes:Select(
                function(_, name) result:Append(self.Database:GetRecipe(name)) end
            )
            results.items:Select(
                function(_, name) result:Append(self.Database:GetItem(name)) end
            )
            return result
        end

        assert(release)
    end

    return {}
end

function Gui:ScanActiveGui(player)
    self.Active.ingteb = mod_gui.get_button_flow(player).ingteb
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
    global.Current.Gui = {}
    player.gui.screen.Presentator.destroy()
    self.Active.Presentator = nil
end

function Gui:SelectTarget(player, targets)
    Helper.ShowFrame(player, "Selector", function(frame) return Selector:new(frame, targets) end)
    self:ScanActiveGui(player)
end

function Gui:PresentTarget(player, target)
    assert(release or target.Prototype)
    Helper.ShowFrame(
        player, "Presentator", function(frame) return Presentator:new(frame, target) end
    )
    self:ScanActiveGui(player)
    return target
end

function Gui:OnMainButtonPressed(player)
    assert(release or self.Active.ingteb)
    assert(release or not self.Active.Selector or not self.Active.Presentator)

    if self.Active.Selector then
        self:CloseSelector(player)
    elseif self.Active.Presentator then
        self:ClosePresentator(player)
    else
        local targets = self:FindTargets(player)
        if #targets == 1 then
            return self:PresentTarget(player, targets[1])
        else
            self:SelectTarget(player, targets)
        end
    end
end

function Gui:EnsureMainButton()
    local player = global.Current.Player -- todo: multiplayer
    if player.gui.top.ingteb then
         player.gui.top.ingteb.destroy() 
        end
    if mod_gui.get_button_flow(player).ingteb == nil then
        assert(release or not self.Active.ingteb)
        mod_gui.get_button_flow(player).add {
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
end

function Gui:UpdateTabOrder(tabOrder, dropIndex)
    local dropTabIndex = tabOrder[tonumber(dropIndex)]
    tabOrder:Remove(dropIndex)
    tabOrder:Append(dropTabIndex)
end

function Gui:OnResearchFinished(research)
    if Database.IsInitialized then
        Gui:EnsureDatabase()
        Gui.Database:RefreshTechnology(research)
        Helper.RefreshResearchChanged(Database)
    end
end

function Gui:OnGuiClickForPresentator(player, event)
    self:EnsureDatabase()
    local target = self.Database:Get(global.Current.Links[event.element.index])
    if target and target.Prototype then
        if UI.IsMouseCode(event, "--- l") then return self:PresentTarget(player, target) end

        local order = target:GetHandCraftingRequest(event)
        if order then
            player.begin_crafting(order)
            return
        end

        local order = target:GetResearchRequest(event)
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
