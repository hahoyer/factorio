local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local class = require("core.class")

local Category = class:new("Category", Common)

local function GetPrototype(domain, category)
    if domain == "crafting" then
        return game.recipe_category_prototypes[category]
    elseif category == "steel-axe" then
        return game.technology_prototypes["steel-axe"]
    elseif domain == "mining" or domain == "fluid-mining" then
        return game.resource_category_prototypes[category]
    elseif domain == "boiling" then
        return game.fluid_prototypes[category]
    else
        assert(release)
    end
end

Category.property = {
    Workers = {
        cache = true,
        get = function(self)
            local result = self.Database.WorkersForCategory[self.Name] --
            :Select(function(worker) return self.Database:GetEntity(nil, worker) end)

            if self.Domain == "mining" or self.Domain == "hand-mining" then
                result:Append(
                    self.Database:GetEntity("(hand-miner)", game.entity_prototypes["character"])
                )
            end

            return result
        end,
    },

    RecipeList = {
        cache = true,
        get = function(self)
            local recipeList = self.Database.RecipesForCategory[self.Name] or Array:new{} --
            local result = recipeList --
            :Select(
                function(recipeName)
                    if self.Domain == "crafting" then
                        return self.Database:GetRecipe(recipeName)
                    elseif self.Domain == "mining" or self.Domain == "fluid-mining"
                        or self.Domain == "hand-mining" then
                        return self.Database:GetMiningRecipe(recipeName)
                    elseif self.Domain == "boiling" then
                        return self.Database:GetBoilingRecipe(recipeName)
                    else
                        assert(release)
                    end
                end
            ) --
            :Where(function(recipe) return recipe end) --
            return result
        end,
    },
}

function Category:new(name, prototype, database)
    assert(release or name)

    local _, _, domain, category = name:find("^(.+)%.(.*)$")

    local p = GetPrototype(domain, category)
    if not p then __DebugAdapter.breakpoint() end
    local self = self:adopt(self.base:new(prototype or GetPrototype(domain, category), database))
    self.Domain = domain
    self.SubName = self.Prototype.name
    self.Name = self.Domain .. "." .. self.SubName


    self.cache.Category.Workers.IsValid = true

    function self:SortAll() end

    return self

end

return Category

