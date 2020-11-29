local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")

local Category = Common:class("Category")

local function GetPrototype(domain, category)
    if domain == "crafting" then
        return game.recipe_category_prototypes[category]
    elseif domain == "mining" or domain == "fluid-mining" then
        return game.resource_category_prototypes[category]
    elseif domain == "hand-mining" and category == "steel-axe" then
        return game.technology_prototypes["steel-axe"]
    else
        assert(todo)
    end
end

function Category:new(name, prototype, database)
    assert(name)

    local _, _, domain, category = name:find("^(.+)%.(.*)$")

    local self = Common:new(prototype or GetPrototype(domain, category), database)
    self.object_name = Category.object_name
    self.Domain = domain
    self.Key = self.Domain .. "." .. self.Name

    assert(
        self.Prototype.object_name == "LuaResourceCategoryPrototype" --
        or self.Prototype.object_name == "LuaRecipeCategoryPrototype" --
        or self.Prototype.object_name == "LuaTechnologyPrototype" --
    )

    self.Workers = Array:new()

    self:properties{
        Recipes = {
            get = function()
                local recipes = self.Database.RecipesForCategory[self.Domain .. "." .. self.Name] --
                if not recipes then return Array:new{} end
                return recipes:Select(
                    function(recipeName)
                        return self.Database:GetRecipe(recipeName)
                    end
                )
            end,
        },
    }

    if self.Domain == "mining" or self.Domain == "hand-mining" then
        self.Workers:Append(
            self.Database:GetEntity("(hand-miner)", game.entity_prototypes["character"])
        )
    end

    function self:SortAll() end

    return self

end

return Category

