local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local UI = require("core.UI")
local class = require("core.class")

local Technology = class:new("Technology", Common)
local ignore

Technology.property = {
    Amount = {
        cache = true,
        get = function(self) --
            local formula = self.Prototype.research_unit_count_formula
            if formula then
                local level = self.Prototype.level
                local result = game.evaluate_expression(formula, {L = level, l = level})
                return result
            else
                return self.Prototype.research_unit_count
            end
        end,
    },

    Ingredients = {
        get = function(self) --
            return Array:new(self.Prototype.research_unit_ingredients) --
            :Select(
                function(tag, index)
                    local result = self.Database:GetStackOfGoods(tag)
                    result.Goods.UsedBy:AppendForKey(" researching", self)
                    result.Source = {Technology = self, IngredientIndex = index}
                    return result
                end
            ) --
        end,

    },

    Input = {
        get = function(self) --
            return self.Ingredients:Select(
                function(stack)
                    return stack:Clone(
                        function(amounts)
                            amounts.value = amounts.value * self.Amount
                        end
                    )
                end
            ) --
        end,
    },

    NumberOnSprite = {
        get = function(self) --
            if self.Prototype.level and self.Prototype.max_level > 1 then
                return self.Prototype.level
            end
        end,
    },

    SpriteStyle = {
        get = function(self)
            if self.IsResearchedOrResearching then return end
            return self.IsReady
        end,
    },
    IsResearched = {
        get = function(self)
            return UI.Player.force.technologies[self.Prototype.name].researched == true
        end,
    },
    IsResearchedOrResearching = {
        get = function(self) return self.IsResearched or self.IsResearching end,
    },

    IsNextGeneration = {
        get = function(self) return not (self.IsResearched or self.IsReady or self.IsResearching) end,
    },

    IsResearching = {
        get = function(self)
            local queue = UI.Player.force.research_queue
            for index = 1, #queue do
                if queue[index].name == self.Prototype.name then return true end
            end
        end,
    },
    IsReady = {
        get = function(self)
            return not self.IsResearchedOrResearching and self.Prerequisites:All(
                function(technology)
                    return technology.IsResearchedOrResearching
                end
            )
        end,
    },

    Prerequisites = {
        get = function(self)
            return Dictionary:new(self.Prototype.prerequisites) --
            :ToArray() --
            :Select(
                function(technology)
                    return self.Database:GetTechnology(nil, technology)
                end
            )
        end,
    },

    TopReadyPrerequisite = {
        get = function(self)
            if self.IsResearchedOrResearching then return end
            if self.IsReady then return self end
            for _, technology in pairs(self.Prerequisites) do
                local result = technology.TopReadyPrerequisite
                if result then return result end
            end
        end,
    },

    Enables = {
        cache = true,
        get = function(self)
            local enabledTechnologies =
                self.Database.EnabledTechnologiesForTechnology[self.Prototype.name]
            if enabledTechnologies then
                return enabledTechnologies --
                :Select(
                    function(technology)
                        return self.Database:GetTechnology(nil, technology)
                    end
                )
            else
                return Array:new{}
            end
        end,
    },

    EnabledRecipes = {
        cache = true,
        get = function(self)
            return Dictionary:new(self.Prototype.effects) --
            :Where(function(effect) return effect.type == "unlock-recipe" end) --
            :Select(
                function(effect)
                    return self.Database:GetRecipe(effect.recipe)
                end
            )
        end,
    },

    Effects = {
        cache = true,
        get = function(self)
            return Dictionary:new(self.Prototype.effects) --
            :Select(
                function(effect)
                    if effect.type == "unlock-recipe" then
                        return self.Database:GetRecipe(effect.recipe)
                    end
                    return self.Database:GetBonusFromEffect(effect)
                end
            )
        end,
    },

    SpecialFunctions = {
        get = function(self) --
            return Array:new{
                {
                    UICode = "-C- l",
                    HelpText = "gui-technology-preview.start-research",
                    IsAvailable = function() return self.IsReady end,
                    Action = function() return {Research = self} end,
                },
                {
                    UICode = "AC- l",
                    HelpText = "ingteb-utility.multiple-research",
                    IsAvailable = function() return self.IsNextGeneration end,
                    Action = function() return {Research = self, Multiple = true} end,
                },
            }
        end,
    },
}

function Technology:new(name, prototype, database)
    local self = self:adopt(self.base:new(prototype or game.technology_prototypes[name], database))
    
    assert(self.Prototype.object_name == "LuaTechnologyPrototype")

    self.TypeOrder = 3
    self.SpriteType = "technology"
    self.Time = self.Prototype.research_unit_energy
    self.IsRefreshRequired = {Research = true}

    function self:Refresh() self.EnabledRecipes:Select(function(recipe) recipe:Refresh() end) end

    function self:IsBefore(other)
        if self == other then return false end
        if self.TypeOrder ~= other.TypeOrder then return self.TypeOrder < other.TypeOrder end
        if self.IsResearched ~= other.IsResearched then return self.IsResearched end
        if self.IsReady ~= other.IsReady then return self.IsReady end
        return self.Prototype.order < other.Prototype.order
    end

    function self:SortAll()
        if not self.CreatedBy then self.CreatedBy = self.OriginalCreatedBy end
        if not self.UsedBy then self.UsedBy = self.OriginalUsedBy end
    end

    return self

end

return Technology
