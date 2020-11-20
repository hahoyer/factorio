local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")

function Item(name, prototype, database)
    local self = CommonThing(name, prototype, database)
    self.class_name = "Item"
    self.SpriteType = "item"

    self.property.RecipeList = {
        get = function() return self.Entity and self.Entity.RecipeList or Array:new{} end,
    }

    if self.Name:find("mini") then --
        local x = y
    end

    function Sort(target)
        local targetArray = target:ToArray(
            function(value, key) return {Value = value, Key = key} end
        )
        targetArray:Sort(
            function(a, b)
                if a == b then return false end
                local aOrder = a.Value:Select(function(recipe) return recipe.Order end):Sum()
                local bOrder = b.Value:Select(function(recipe) return recipe.Order end):Sum()
                if aOrder ~= bOrder then return aOrder > bOrder end

                local aSubOrder = a.Value:Select(
                    function(recipe) return recipe.SubOrder end
                ):Sum()
                local bSubOrder = b.Value:Select(
                    function(recipe) return recipe.SubOrder end
                ):Sum()
                return aSubOrder > bSubOrder

            end
        )

        return targetArray:ToDictionary(
            function(value)
                value.Value:Sort(function(a, b) return a:IsBefore(b) end)
                return value
            end
        )

    end

    function self:Setup()
        if self.Prototype.place_result then
            self.Entity = self.Database.Entities[self.Prototype.place_result.name]
        end

        self.In = Sort(self.In)
        self.Out = Sort(self.Out)

    end

    return self
end

