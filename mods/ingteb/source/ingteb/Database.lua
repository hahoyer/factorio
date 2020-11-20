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
require("ingteb.Category")

local Database = PropertyProvider:new{cache = {}}

function Database:new() return Database end

function Database:addCachedProperty(name, getter)
    self.cache[name] = ValueCache(getter)
    self.property[name] = {get = function(self) return self.cache[name].Value end}
end

function Database:CreateMiningRecipe(resource)
    self.Recipes[resource.Name] = MiningRecipe(resource, self)
end

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
    if item then return ItemSet(item, amounts, self) end
end

function Measure(target)
    local profiler = game.create_profiler()
    profiler.reset()
    target()
    profiler.stop()
    log(profiler)
    return ""
end

function Database:Scan()
    if self.Entities then return end

    self.Entities = {}
    self.Items = {}
    self.Fluids = {}
    self.Recipes = {}
    self.Technologies = {}
    self.Categories = Dictionary:new{}
    self.Bonuses = {}

    Dictionary:new(game.entity_prototypes) --
    :Where(function(value) return not (value.flags and value.flags.hidden) end) --
    :Select(function(value, key) self.Entities[key] = Entity(key, value, self) end)

    Dictionary:new(game.item_prototypes) --
    :Where(function(value) return not (value.flags and value.flags.hidden) end) --
    :Select(function(value, key) self.Items[key] = Item(key, value, self) end)

    Dictionary:new(game.fluid_prototypes) --
    :Where(function(value) return not (value.hidden) end) --
    :Select(function(value, key) self.Fluids[key] = Fluid(key, value, self) end)

    Dictionary:new(game.resource_category_prototypes) --
    :Select(
        function(value, key)
            self.Categories[key .. " mining"] = Category("mining", value, self)
            self.Categories[key .. " fluid mining"] = Category("fluid mining", value, self)
        end
    )

    Dictionary:new(game.recipe_category_prototypes) --
    :Select(
        function(value, key)
            self.Categories[key .. " crafting"] = Category("crafting", value, self)
        end
    )

    self.Categories[" hand mining"] = Category(
        "hand mining", game.technology_prototypes["steel-axe"], self
    )

    self.Categories[" hand mining"].Workers:Append(self.Entities["character"])
    self.Categories["basic-solid mining"].Workers:Append(self.Entities["character"])

    Dictionary:new(game.recipe_prototypes) --
    :Select(function(value, key) self.Recipes[key] = Recipe(key, value, self) end)

    Dictionary:new(game.technology_prototypes) --
    :Where(function(value) return not value.hidden end) --
    :Select(function(value, key) self.Technologies[key] = Technology(key, value, self) end)
    
    Dictionary:new(self.Entities):Select(function(entity) entity:Setup() end)
    Dictionary:new(self.Recipes):Select(function(entity) entity:Setup() end)
    Dictionary:new(self.Technologies):Select(function(entity) entity:Setup() end)
    Dictionary:new(self.Categories):Select(function(entity) entity:Setup() end)
    Dictionary:new(self.Fluids):Select(function(entity) entity:Setup() end)
    Dictionary:new(self.Items):Select(function(entity) entity:Setup() end)
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
        if cursor and cursor.valid and cursor.valid_for_read then
            return self.Items[cursor.name]
        end
        local cursor = global.Current.Player.cursor_ghost
        if cursor then return self.Items[cursor.name] end

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

            assert()
        end
        -- local cursor = global.Current.Player.entity_copy_source
        -- assert(not cursor)

    end

    local result = get()
    return result
end

function Database:Get(target)
    if target.type == "item" then return self.Items[target.name] end
    -- assert()
end

return Database
