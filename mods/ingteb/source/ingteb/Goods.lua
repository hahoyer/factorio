local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")

local Goods = Common:class("Goods")

function Goods:new(prototype, database)
    local self = Common:new(prototype, database)
    self.object_name = Goods.object_name

    self:properties{
        OriginalRecipeList = {
            get = function()
                return self.Entity and self.Entity.RecipeList or Array:new{} 
            end,
        },

        RecipesForItem = {
            cache = true,
            get = function()
                return self.Database.RecipesForItems[self.Prototype.name] or {}
            end,
        },

        OriginalUsedBy = {
            get = function()
                local names = self.RecipesForItem.UsedBy
                if not names then return Dictionary:new{} end

                return names --
                :Select(
                    function(value)
                        return value --
                        :Select(
                            function(value)
                                return self.Database:GetRecipe(value)
                            end
                        )
                    end
                )
            end,
        },

        OriginalCreatedBy = {
            get = function()
                local names = self.RecipesForItem.CreatedBy
                if not names then return Dictionary:new{} end

                return names --
                :Select(
                    function(value)
                        return value --
                        :Select(
                            function(value)
                                return self.Database:GetRecipe(value)
                            end
                        )
                    end
                )

            end,
        },
    }

    local function Sort(target)
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

    function self:SortAll()
        if not self.RecipeList then self.RecipeList = self.OriginalRecipeList end
        if not self.CreatedBy then self.CreatedBy = self.OriginalCreatedBy end
        if not self.UsedBy then self.UsedBy = self.OriginalUsedBy end
        
        self.RecipeList = Sort(self.RecipeList)
        self.CreatedBy = Sort(self.CreatedBy)
        self.UsedBy = Sort(self.UsedBy)
    end

    return self

end

return Goods
