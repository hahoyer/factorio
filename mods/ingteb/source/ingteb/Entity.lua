local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")

local Entity = Common:class("Entity")

function Entity:new(name, prototype, database)
    local self = Common:new(prototype or game.entity_prototypes[name], database)
    self.object_name = Entity.object_name
    self.SpriteType = "entity"

    assert(self.Prototype.object_name == "LuaEntityPrototype")

    -- special entity for handmining
    if name == "(hand-miner)" then
        assert(prototype.name == "character")
        self:properties{SpriteName = {get = function() return "technology/steel-axe" end}}
    end

    self.NumberOnSprite --
    = self.Prototype.mining_speed --
    or self.Prototype.crafting_speed -- 
    or self.Prototype.researching_speed -- 

    self.UsedBy = Dictionary:new{}
    self.CreatedBy = Dictionary:new{}

    self:properties{
        ClickHandler = {get = function() return self.Item end},
        Item = {
            cache = true,
            get = function()
                local place = self.Prototype.items_to_place_this
                if not place or #place == 0 then return end
                assert(#place == 1)
                return self.Database:GetItem(place[1].name)
            end,
        },
        IsResource = {
            cache = true,
            get = function()
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
            get = function()
                local xreturn = self.Database.Proxies.Category -- 
                :Where(
                    function(category)
                        local domain = category.Domain
                        local list
                        if domain == "mining" or domain == "fluid-mining" then
                            list = self.Prototype.resource_category
                        elseif domain == "crafting" then
                            list = self.Prototype.crafting_categories
                        elseif domain == "hand-mining" then
                            return
                        elseif domain == "researching" then
                            return self.Prototype.lab_inputs
                        else
                            assert()
                        end
                        return list and list[category.Name]
                    end
                ) --
                return xreturn
            end,
        },

        RecipeList = {
            cache = true,
            get = function()
                return self.Categories --
                :Select(function(category) return category.Recipes end) --
                :Where(function(recipes) return recipes:Any() end) --
            end,
        },
    }

    function self:SortAll() end

    return self

end

return Entity
