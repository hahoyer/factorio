local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")

function Entity(name, prototype, database)
    local self = CommonThing(name, prototype, database)
    self.class_name = "Entity"
    self.SpriteType = "entity"

    self:addCachedProperty(
        "Item", function()
            local place = self.Prototype.items_to_place_this
            if not place or #place == 0 then return end
            assert(#place == 1)
            return self.Database.Items[place[1].name]
        end
    )

    self:addCachedProperty(
        "IsResource", function()
            local prototype = self.Prototype
            if not prototype.mineable_properties --
            or not prototype.mineable_properties.minable --
                or not prototype.mineable_properties.products --
            then return end
            return not prototype.items_to_place_this
        end
    )

    self:addCachedProperty(
        "RecipeList", function()
            return self.Categories --
            :Select(function(category) return category.Recipes end) --
            :Where(function(recipes) return recipes:Any() end) --

        end
    )

    function self:Setup()
        if self.Name:find("mini") then --
            local x = y
        end
        if self.Name == "coal" then --
            local x = y
        end

        if self.IsResource then self.Database:CreateMiningRecipe(self) end

        self.Categories = self.Database.Categories -- 
        :Where(
            function(category)
                local domain = category.DomainName
                local list
                if domain == "mining" or domain == "fluid mining" then
                    list = self.Prototype.resource_categories
                elseif domain == "crafting" then
                    list = self.Prototype.crafting_categories
                elseif domain == "hand mining" then
                    return
                else
                    assert()
                end
                return list and list[category.Name]
            end
        ) --
        :Select(
            function(category)
                category.Workers:Append(self)
                return category
            end
        )

    end

    return self
end

