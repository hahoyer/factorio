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

    function self:Collect(entities, domain, dictionary)
        if not entities then return end
        for key, _ in pairs(entities) do self:AppendForKey(key .. " " .. domain, dictionary) end
    end

    function self:Setup()
        if self.Name:find("mini") then --
            local x = y
        end
        if self.Name == "coal" then --
            local x = y
        end

        if self.IsResource then self.Database:CreateMiningRecipe(self) end

        self:Collect(self.Prototype.resource_categories, "mining", self.Database.WorkingEntities)
        if #self.Prototype.fluidbox_prototypes > 0 then
            self:Collect(self.Prototype.resource_categories, "fluid mining", self.Database.WorkingEntities)
        end
        self:Collect(self.Prototype.crafting_categories, "crafting", self.Database.WorkingEntities)

    end

    return self
end

