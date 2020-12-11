local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local Goods = require("ingteb.Goods")
local UI = require("core.UI")
local class = require("core.class")

function FormatEnergy(value)
    if value < 0 then return "-" .. FormatEnergy(-value) end
    if value < 10 then return value .. "J" end
    value = value / 1000
    if value < 10 then return value .. "kJ" end
    value = value / 1000
    if value < 10 then return value .. "MJ" end
    value = value / 1000
    if value < 10 then return value .. "GJ" end
    value = value / 1000
    if value < 10 then return value .. "TJ" end
    value = value / 1000
    return value .. "PJ"

end

local Item = class:new("Item", Goods)

Item .property = {
    Entity = {
        cache = true,
        get = function(self)
            if self.Prototype.place_result then
                return self.Database:GetEntity(self.Prototype.place_result.name)
            end
        end,
    },

    FuelDescription = {
        get = function(self)
            local result = Array:new{}

            if self.Prototype.fuel_value and self.Prototype.fuel_value > 0 then
                result:Append{
                    "",
                    {"description.fuel-value"},
                    " " .. FormatEnergy(self.Prototype.fuel_value),
                }
            end

            if self.Prototype.fuel_acceleration_multiplier
                and self.Prototype.fuel_acceleration_multiplier ~= 1 then
                result:Append{
                    "",
                    {"description.fuel-acceleration"},
                    " " .. self.Prototype.fuel_acceleration_multiplier,
                }
            end
            
            return result
        end,
    },

    SpecialFunctions = {
        get = function(self) --
            return Array:new{
                {
                    UICode = "--S l",
                    Action = function()
                        return {Selecting = self, Entity = self.Entity}
                    end,
                },
            }
        end,
    },
}

function Item:new(name, prototype, database)
    local self = self:adopt(self.base:new(prototype or game.item_prototypes[name], database))
    self.SpriteType = "item"

    assert(release or self.Prototype.object_name == "LuaItemPrototype")


    if self.Prototype.fuel_category then
        self.Fuel = {
            Category = self.Database:GetFuelCategory(self.Prototype.fuel_category),
            Value = self.Prototype.fuel_value,
            Acceleration = self.Prototype.fuel_acceleration_multiplier,
        }

    end

    return self

end

return Item
