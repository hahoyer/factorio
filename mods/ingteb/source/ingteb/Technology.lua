local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local UI = require("core.UI")

local Technology = Common:class("Technology")
local ignore

function Technology:new(name, prototype, database)

    local self = Common:new(prototype or game.technology_prototypes[name], database)
    self.object_name = Technology.object_name

    assert(self.Prototype.object_name == "LuaTechnologyPrototype")

    self.TypeOrder = 3
    self.SpriteType = "technology"
    self.IsDynamic = true
    self.Time = self.Prototype.research_unit_energy

    self:properties{
        Amount = {
            cache = true,
            get = function() --
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
            get = function() --
                return Array:new(self.Prototype.research_unit_ingredients) --
                :Select(
                    function(tag)
                        local result = database:GetStackOfGoods(tag)
                        result.Goods.UsedBy:AppendForKey(" researching", self)
                        return result
                    end
                ) --
            end,

        },

        Input = {
            get = function() --
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
            get = function() --
                if self.Prototype.level and self.Prototype.max_level > 1 then
                    return self.Prototype.level
                end
            end,
        },

        FunctionalHelp = {
            get = function() --
                if self.IsResearchedOrResearching then
                    return
                elseif self.IsReady then
                    return UI.GetHelpTextForButtonsACS12("ingteb-utility.research")
                else
                    return UI.GetHelpTextForButtonsACS12("ingteb-utility.multiple-research")
                end
            end,
        },

        SpriteStyle = {
            get = function()
                if self.IsResearchedOrResearching then return end
                return self.IsReady
            end,
        },
        IsResearched = {
            get = function()
                return UI.Player.force.technologies[self.Prototype.name].researched == true
            end,
        },
        IsResearchedOrResearching = {
            get = function() return self.IsResearched or self.IsResearching end,
        },
        IsResearching = {
            get = function()
                local queue = UI.Player.force.research_queue
                for index = 1, #queue do
                    if queue[index].name == self.Prototype.name then return true end
                end
            end,
        },
        IsReady = {
            get = function()
                return self.Prerequisites:All(
                    function(technology)
                        return technology.IsResearchedOrResearching
                    end
                )
            end,
        },

        Prerequisites = {
            get = function()
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
            get = function()
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
            get = function()
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
            get = function()
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
            get = function()
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

    }

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

    function self:GetAction(event)
        if UI.IsMouseCode(event, "-C- l") --
        and self.IsReady --
        then return {Research = self} end

        if UI.IsMouseCode(event, "AC- l") --
        and not self.IsResearchedOrResearching --
        then return {Research = self, Multiple = true} end
    end

    return self

end

return Technology
