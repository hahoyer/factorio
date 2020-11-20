local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")

function Technology(name, prototype, database)
    local self = Common(name, prototype, database)
    self.class_name = "Technology"
    self.SpriteType = "technology"

    self.property.IsReady = {
        get = function(self)
            return self.Prerequisites:All(
                function(technology) return technology.IsResearched end
            )
        end,
    }

    self.HelperText = nil
    self.property.HelperText = {
        get = function(self) --
            if not self.IsResearched and self.IsReady then
                return {
                    "ingteb_utility.Lines2",
                    self.LocalisedName,
                    {
                        "ingteb_utility.research",
                        {"control-keys.alt"},
                        {"control-keys.control"},
                        {"control-keys.shift"},
                        {"control-keys.mouse-button-1-alt-1"},
                        {"control-keys.mouse-button-2-alt-1"},
                    },
                }
            else
                return self.LocalisedName
            end
        end,
    }

    self:addCachedProperty(
        "NumberOnSprite", function()
            if self.Prototype.level and self.Prototype.max_level > 1 then
                return self.Prototype.level
            end
        end
    )

    self.property.SpriteStyle = {
        get = function(self)
            if self.IsResearched then return end
            if self.IsReady then return Constants.GuiStyle.LightButton end
            return "red_slot_button"
        end,
    }

    self.property.IsResearched = {
        get = function(self)
            return global.Current.Player.force.technologies[self.Name].researched
        end,
    }

    self.IsDynamic = true
    self.Enables = Array:new()

    function self:Setup()
        self.Prerequisites = Dictionary:new(self.Prototype.prerequisites) --
        :ToArray() --
        :Select(
            function(technology)
                local result = self.Database.Technologies[technology.name]
                result.Enables:Append(self)
                return result
            end
        )

        self.In = Array:new(self.Prototype.research_unit_ingredients) --
        :Select(
            function(tag)
                local result = database:GetItemSet(tag)
                result.Item.TechnologyIngredients:Append(self)
                return result
            end
        )

        self.Out = Array:new(self.Prototype.effects) --
        :Select(
            function(effect)
                if effect.type == "unlock-recipe" then
                    local result = database.Recipes[effect.recipe]
                    result.Technologies:Append(self)
                    return result
                else
                    database:AddBonus(effect, self)
                    return
                end
            end
        ) --
        :Where(function(item) return item end)

    end

    return self
end

