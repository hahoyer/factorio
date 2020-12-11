local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local class = require("core.class")

local Entity = class:new("Entity", Common)

Entity.property = {
    SpriteName = {
        get = function(self)
            -- special entity for handmining
            if self.Name == "(hand-miner)" then
                return "technology/steel-axe"
            end
            return self.inherited.SpriteName.get(self)
        end,
    },

    ClickTarget = {get = function(self) return self.Item and self.Item.ClickTarget end},
    Item = {
        cache = true,
        get = function(self)
            local place = self.Prototype.items_to_place_this
            if not place or #place == 0 then return end
            assert(release or #place == 1)
            return self.Database:GetItem(place[1].name)
        end,
    },
    IsResource = {
        cache = true,
        get = function(self)
            local prototype = self.Prototype
            if not prototype.mineable_properties --
            or not prototype.mineable_properties.minable --
                or not prototype.mineable_properties.products --
            then return end
            return not prototype.items_to_place_this
        end,
    },
    Categories = {
        cache = true,
        get = function(self)
            local xreturn = self.Database.Proxies.Category -- 
            :Where(
                function(category)
                    local domain = category.Domain
                    local list = {}
                    if domain == "mining" or domain == "fluid-mining" then
                        list = self.Prototype.resource_categories
                    elseif domain == "crafting" then
                        list = self.Prototype.crafting_categories
                    elseif domain == "hand-mining" then
                        return
                    elseif domain == "researching" then
                        return self.Prototype.lab_inputs
                    elseif domain == "boiling" then
                        return self.Prototype.lab_inputs
                    else
                        assert(release)
                    end
                    return list and list[category.SubName]
                end
            ) --
            return xreturn
        end,
    },

    RecipeList = {
        cache = true,
        get = function(self)
            return self.Categories --
            :Select(function(category) return category.RecipeList end) --
            :Where(function(recipes) return recipes:Any() end) --
        end,
    },
}

function Entity:SortAll() end

function Entity:new(name, prototype, database)
    local self = self:adopt(self.base:new(prototype or game.entity_prototypes[name], database))
    self.SpriteType = "entity"

    assert(release or self.Prototype.object_name == "LuaEntityPrototype")

    self.NumberOnSprite --
    = self.Prototype.mining_speed --
    or self.Prototype.crafting_speed -- 
    or self.Prototype.researching_speed -- 

    self.UsedBy = Dictionary:new{}
    self.CreatedBy = Dictionary:new{}


    return self

end

return Entity
