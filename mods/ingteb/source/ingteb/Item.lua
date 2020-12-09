local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local Goods = require("ingteb.Goods")
local UI = require("core.UI")

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

local Item = Common:class("Item")

function Item:new(name, prototype, database)
    local self = Goods:new(prototype or game.item_prototypes[name], database)
    self.object_name = Item.object_name
    self.SpriteType = "item"

    assert(release or self.Prototype.object_name == "LuaItemPrototype")

    self:properties{
        Entity = {
            cache = true,
            get = function()
                if self.Prototype.place_result then
                    return self.Database:GetEntity(self.Prototype.place_result.name)
                end
            end,
        },

        FuelDescription = {
            get = function()
                local result = Array:new{}

                if self.Prototype.fuel_value then
                    result:Append{
                        "",
                        {"description.fuel-value"},
                        " " .. FormatEnergy(self.Fuel.Value),
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
