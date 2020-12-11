local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local Goods = require("ingteb.Goods")
local class = require("core.class")

local Fluid = class:new("Fluid", Goods)

Fluid.property = {
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

            return result
        end,
    },
}

function Fluid:new(name, prototype, database)
    local self = self:adopt(self.base:new(prototype or game.fluid_prototypes[name], database))
    self.SpriteType = "fluid"

    assert(release or self.Prototype.object_name == "LuaFluidPrototype")

    return self

end

return Fluid
