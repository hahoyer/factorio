local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
local PropertyProvider = require("core.PropertyProvider")

function ItemSet(item, amounts, database)

    local self = PropertyProvider:new{
        class_name = "ItemSet",
        SpriteType = item.SpriteType,
        Item = item,
        Name = item.Name,
        Amounts = amounts,
        Database = database,

        cache = {},
    }

    assert(self.Item)

    function self:addCachedProperty(name, getter)
        self.cache[name] = ValueCache(getter)
        self.property[name] = {get = function(self) return self.cache[name].Value end}
    end

    self:addCachedProperty(
        "NumberOnSprite", function()
            local amounts = self.Amounts
            if not amounts then return end
            if amounts.min and amounts.max and amounts.probability and not amounts.value then
                return (amounts.max + amounts.min) / 2 * amounts.probability
            end
            if not amounts.min and not amounts.max and amounts.probability and amounts.value then
                return amounts.value * amounts.probability
            end
            if amounts.min or amounts.max or amounts.probability then assert() end
            return amounts.value
        end
    )

    self:addCachedProperty("SpriteName", function() return self.SpriteType .."/".. self.Name end)
    self:addCachedProperty("HelperText", function() return self.Item.LocalisedName end)

    return self
end

