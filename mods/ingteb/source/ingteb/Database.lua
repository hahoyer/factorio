local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
local PropertyProvider = require("core.PropertyProvider")
require("ingteb.Entity")
require("ingteb.Item")
require("ingteb.Fluid")
require("ingteb.Recipe")
require("ingteb.MiningRecipe")
require("ingteb.Technology")
require("ingteb.ItemSet")

local Database = PropertyProvider:new{cache = {}}

function Database:new() return Database end

function Database:addCachedProperty(name, getter)
    self.cache[name] = ValueCache(getter)
    self.property[name] = {get = function(self) return self.cache[name].Value end}
end

function Database:CreateMiningRecipe(resource) self.Recipes[resource.Name] = MiningRecipe(resource, self) end

function Database:GetItemSet(target)
    local amounts = {
        value = target.amount,
        probability = target.probability,
        min = target.amount_min,
        max = target.amount_max,
    }
    local item --
    = target.type == "item" and self.Items[target.name] --
    or target.type == "fluid" and self.Fluids[target.name] --
    or assert()

    if false then
        assert(
            not Dictionary:new(target) --
            :Where(
                function(_, key)
                    return not Array:new{
                        "type",
                        "name",
                        "amount",
                        "probability",
                        "fluidbox_index",
                        "catalyst_amount",
                        "amount_min",
                        "amount_max",
                    }:Contains(key)
                end
            ) --
            :Any()
        )
    end

    return ItemSet(item, amounts, self)
end

function Database:Scan()
    if self.Entities then return end

    self.Entities = {}
    self.Items = {}
    self.Fluids = {}
    self.Recipes = {}
    self.Technologies = {}
    self.WorkingEntities = {}
    self.Bonuses = {}

    Dictionary:new(game.entity_prototypes) --
    :Select(function(value, key) self.Entities[key] = Entity(key, value, self) end)

    Dictionary:new(game.item_prototypes) --
    :Select(function(value, key) self.Items[key] = Item(key, value, self) end)
    Dictionary:new(game.fluid_prototypes) --
    :Select(function(value, key) self.Fluids[key] = Fluid(key, value, self) end)

    Dictionary:new(game.resource_category_prototypes) --
    :Select(
        function(value, key)
            self.WorkingEntities[key .. " mining"] = Array:new()
            self.WorkingEntities[key .. " fluid mining"] = Array:new()
        end
    )

    Dictionary:new(game.recipe_category_prototypes) --
    :Select(function(value, key) self.WorkingEntities[key .. " crafting"] = Array:new() end)

    self.WorkingEntities[" hand mining"] = Array:new{self.Entities["character"]}

    self.WorkingEntities["basic-solid mining"]:Append(self.Entities["character"])

    Dictionary:new(game.recipe_prototypes) --
    :Select(function(value, key) self.Recipes[key] = Recipe(key, value, self) end)

    Dictionary:new(game.technology_prototypes) --
    :Select(function(value, key) self.Technologies[key] = Technology(key, value, self) end)

    Dictionary:new(self.Entities) --
    :Select(function(entity) entity:Setup() end)

    Dictionary:new(self.Items) --
    :Select(function(entity) entity:Setup() end)

    Dictionary:new(self.Fluids) --
    :Select(function(entity) entity:Setup() end)

    Dictionary:new(self.Recipes) --
    :Select(function(entity) entity:Setup() end)

    Dictionary:new(self.Technologies) --
    :Select(function(entity) entity:Setup() end)

end

function Database:AddBonus(target, technology)
    local bonus = self.Bonuses[target.type]
    if not bonus then
        bonus = Array:new()
        self.Bonuses[target.type] = bonus
    end
    bonus:Append{Technology = technology, Modifier = target.modifier}

end

function Database:OnLoad() self:Scan() end

function Database:FindTarget()
    local function get()
        local cursor = global.Current.Player.cursor_stack
        if cursor and cursor.valid and cursor.valid_for_read then return self.Items[cursor.name] end
        local cursor = global.Current.Player.cursor_ghost
        if cursor then --
            return {type = cursor.type, name = cursor.name}
        end
        local cursor = global.Current.Player.selected
        if cursor then
            local result = self.Entities[cursor.name]
            if result.IsResource then
                return result
            else
                return result.Item
            end
        end

        local cursor = global.Current.Player.opened
        if cursor then

            local t = global.Current.Player.opened_gui_type
            if t == defines.gui_type.custom then return end
            if t == defines.gui_type.entity then return self.Entities[cursor.name] end

            if global.Current.Links and global.Current.Links[cursor.index] then
                local target = global.Current.Links[cursor.index]
                return target
            end
            if cursor.burner then return {fuel_categories = cursor.burner.fuel_categories} end
            if cursor.type == "mining-drill" and cursor.mining_target then
                return {type = cursor.mining_target.type, name = cursor.mining_target.name}
            end

            if cursor.type == "furnace" and cursor.previous_recipe then
                return {type = "recipe", name = cursor.previous_recipe.name}
            end
            if cursor.type == "assembling-machine" and cursor.get_recipe() then
                return {type = "recipe", name = cursor.get_recipe().name}
            end
        end
    end

    local result = get()
    return result
end

function Database:Get(target)
    if target.type == "item" then return self.Items[target.name] end
    -- assert()
end

function Database.IsBefore(this, other)
    if this == other then return false end

    if this.class_name ~= other.class_name then return this.class_name == "MiningRecipe" end
    if this.class_name ~= "MiningRecipe" then

        if (not this.Technology) ~= (not other.Technology) then return not this.Technology end
        if this.IsResearched ~= other.IsResearched then return this.IsResearched end
        if this.Technology then
            if this.Technology.IsReady ~= other.Technology.IsReady then return this.Technology.IsReady end
        end
    end
    if this.Prototype.group ~= other.Prototype.group then
        return this.Prototype.group.order < other.Prototype.group.order
    end
    if this.Prototype.subgroup ~= other.Prototype.subgroup then
        return this.Prototype.subgroup.order < other.Prototype.subgroup.order
    end

    return this.Prototype.order < other.Prototype.order
end
return Database
