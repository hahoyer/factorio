local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCacheContainer = require("core.ValueCacheContainer")
local class = require("core.class")
local Proxy = {
    Category = require("ingteb.Category"),
    Entity = require("ingteb.Entity"),
    FuelCategory = require("ingteb.FuelCategory"),
    Fluid = require("ingteb.Fluid"),
    Item = require("ingteb.Item"),
    ImplicitRecipe = require("ingteb.ImplicitRecipe"),
    Recipe = require("ingteb.Recipe"),
    Technology = require("ingteb.Technology"),
}

local StackOfGoods = require("ingteb.StackOfGoods")

local DatabaseClass = class:new{"DatabaseClass"}
local Database = DatabaseClass:adopt{}
Database.object_name = "Database"

local function EnsureKey(data, key, value)
    local result = data[key]
    if not result then
        result = value or {}
        data[key] = result
    end
    return result
end

function Database:Ensure()
    if self.IsInitialized then return self end
    self.IsInitialized = "pending"

    log("database initialize start...")
    self.RecipesForItems = {}
    self.RecipesForCategory = {}
    for _, prototype in pairs(game.recipe_prototypes) do self:ScanRecipe(prototype) end

    self.TechnologiesForRecipe = {}
    self.EnabledTechnologiesForTechnology = {}
    for _, prototype in pairs(game.technology_prototypes) do self:ScanTechnology(prototype) end

    self.ItemsForFuelCategory = {}
    for _, prototype in pairs(game.item_prototypes) do self:ScanItem(prototype) end

    self.EntitiesForBurnersFuel = {}
    self.WorkersForCategory = {}
    self.Resources = {}
    for _, prototype in pairs(game.entity_prototypes) do self:ScanEntity(prototype) end

    log("database initialize proxies...")
    self.Proxies = {
        ImplicitRecipe = Dictionary:new{},
        Category = Dictionary:new{},
        Fluid = Dictionary:new{},
        Recipe = Dictionary:new{},
        Technology = Dictionary:new{},
        Entity = Dictionary:new{},
    }

    for categoryName in pairs(self.RecipesForCategory) do
        EnsureKey(self.WorkersForCategory, categoryName, Array:new())
    end

    log("database initialize categories...")
    self:CreateBoilerRecipe()
    self:CreateHandMiningCategory()
    for categoryName in pairs(self.WorkersForCategory) do self:GetCategory(categoryName) end

    log("database initialize recipes...")
    self.Proxies.Item = Dictionary:new{}
    self.Proxies.FuelCategory = Dictionary:new{}
    self.Proxies.Category:Select(function(category) return category.RecipeList end)

    log("database initialize complete.")
    self.IsInitialized = true
    return self
end

function Database:GetProxyFromCommonKey(targetKey)
    Database:Ensure()
    local _, _, className, prototypeName = targetKey:find("^(.+)%.(.*)$")
    return Database:GetProxy(className, prototypeName)
end

function Database:GetProxy(className, name, prototype)
    local data = self.Proxies[className]
    local key = name or prototype.name

    local result = data[key]
    if not result then
        result = Proxy[className]:new(name, prototype, self):SealUp()
        data[key] = result
    end

    return result
end

function Database:GetFluid(name, prototype) return self:GetProxy("Fluid", name, prototype) end
function Database:GetItem(name, prototype) return self:GetProxy("Item", name, prototype) end
function Database:GetEntity(name, prototype) return self:GetProxy("Entity", name, prototype) end
function Database:GetCategory(name, prototype) return self:GetProxy("Category", name, prototype) end
function Database:GetRecipe(name, prototype) return self:GetProxy("Recipe", name, prototype) end
function Database:GetTechnology(name, prototype) return self:GetProxy("Technology", name, prototype) end
function Database:GetFuelCategory(name, prototype)
    return self:GetProxy("FuelCategory", name, prototype)
end

function Database:GetImplicitRecipeForDomain(domain, name, prototype) --
    return self:GetProxy("ImplicitRecipe", domain .. "." .. (name or prototype.name), prototype)
end

---@param domain string
---@param category string
---@param prototype LuaEntityPrototype
function Database:AddWorkerForCategory(domain, category, prototype)
    EnsureKey(self.WorkersForCategory, domain .. "." .. category, Array:new{}):Append(prototype)
end

local function EnsureRecipeCategory(result, side, name, category)
    local itemData = EnsureKey(result, name)
    local sideData = EnsureKey(itemData, side, Dictionary:new())
    local categoryData = EnsureKey(sideData, "crafting." .. category, Array:new())
    return categoryData
end

function Database:ScanEntity(prototype)
    for category, _ in pairs(prototype.crafting_categories or {}) do
        self:AddWorkerForCategory("crafting", category, prototype)
    end

    for category, _ in pairs(prototype.resource_categories or {}) do
        if #prototype.fluidbox_prototypes > 0 then
            self:AddWorkerForCategory("fluid-mining", category, prototype)
        end
        self:AddWorkerForCategory("mining", category, prototype)
    end

    if prototype.burner_prototype then
        for category, _ in pairs(prototype.burner_prototype.fuel_categories or {}) do
            EnsureKey(self.EntitiesForBurnersFuel, category, Array:new()):Append(prototype.name)
        end
    end

    if prototype.mineable_properties --
    and prototype.mineable_properties.minable --
    and prototype.mineable_properties.products --
    and not prototype.items_to_place_this --
    then
        local isFluidMining = prototype.mineable_properties.required_fluid --
                                  or Array:new(prototype.mineable_properties.products) --
            :Any(function(product) return product.type == "fluid" end) --

        local category = not prototype.resource_category and "hand-mining.steel-axe" --
                             or (isFluidMining and "fluid-mining." or "mining.")
                             .. prototype.resource_category

        EnsureKey(self.RecipesForCategory, category, Array:new()):Append(prototype.name)
    end

    if prototype.type == "character" and prototype.name == "character" then
        self:AddWorkerForCategory("hand-mining", "steel-axe", prototype)
    end
end

function Database:CreateHandMiningCategory() self:GetCategory("hand-mining.steel-axe") end

function Database:CreateBoilerRecipe()
    local prototype = game.entity_prototypes.boiler
    self:AddWorkerForCategory("boiling", "steam", prototype)
    self:GetImplicitRecipeForDomain("boiling", "steam", game.fluid_prototypes["steam"])
end

function Database:ScanTechnology(prototype)
    for _, value in pairs(prototype.effects or {}) do
        if value.type == "unlock-recipe" then
            EnsureKey(self.TechnologiesForRecipe, value.recipe, Array:new()):Append(prototype)
        end
    end
    for key, _ in pairs(prototype.prerequisites or {}) do
        EnsureKey(self.EnabledTechnologiesForTechnology, key, Array:new()):Append(prototype)
    end

end

function Database:ScanItem(prototype)
    if prototype.fuel_category then
        EnsureKey(self.ItemsForFuelCategory, prototype.fuel_category, Array:new()):Append(prototype)
    end
end

function Database:ScanRecipe(prototype)

    if prototype.hidden then return end

    for _, itemSet in pairs(prototype.ingredients) do
        EnsureRecipeCategory(self.RecipesForItems, "UsedBy", itemSet.name, prototype.category) --
        :Append(prototype.name)
    end

    for _, itemSet in pairs(prototype.products) do
        EnsureRecipeCategory(self.RecipesForItems, "CreatedBy", itemSet.name, prototype.category) --
        :Append(prototype.name)
    end

    EnsureKey(self.RecipesForCategory, "crafting." .. prototype.category, Array:new()) --
    :Append(prototype.name)

end

function Database:GetStackOfGoods(target)
    local amounts = {
        value = target.amount,
        probability = target.probability,
        min = target.amount_min,
        max = target.amount_max,
    }
    local goods --
    = target.type == "item" and self:GetItem(target.name) --
    or target.type == "fluid" and self:GetFluid(target.name) --
    if goods then return StackOfGoods:new(goods, amounts, self) end
end

function Database:Get(target)
    local object_name, Name
    if not target or target == "" then
        return
    elseif type(target) == "string" then
        _, _, object_name, Name = target:find("^(.-)%.(.*)$")
    elseif target.type then
        if target.type == "item" then
            object_name = "Item"
        elseif target.type == "fluid" then
            object_name = "Fluid"
        else
            assert(release)
        end
        Name = target.name
    else
        object_name = target.object_name
        Name = target.Name
        Prototype = target.Prototype
    end
    self:Ensure()
    assert(release or object_name)
    assert(release or Name or Prototype)
    return self:GetProxy(object_name, Name, Prototype)
end

function Database:RefreshTechnology(target) self:GetTechnology(target.name):Refresh() end

return Database

