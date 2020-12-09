local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")
local Goods = require("ingteb.Goods")

local Fluid = Common:class("Fluid")

function Fluid:new(name, prototype, database)
    local self = Goods:new(prototype or game.fluid_prototypes[name], database)
    self.object_name = Fluid.object_name
    self.SpriteType = "fluid"

    assert(release or self.Prototype.object_name == "LuaFluidPrototype")

    self:properties{
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
                return result
            end,
        },

    }

    return self

end

return Fluid
