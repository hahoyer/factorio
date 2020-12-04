local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")

local FuelCategory = Common:class("FuelCategory")

function FuelCategory:new(name, prototype, database)
    assert(release or name)

    local self = Common:new(prototype or game.fuel_category_prototypes[name], database)
    self.object_name = FuelCategory.object_name

    assert(release or self.Prototype.object_name == "LuaFuelCategoryPrototype")

    self.Workers = Array:new()
    self.SpriteType = "fuel-category"

    self:properties{
        Fuels = {
            cache = true,
            get = function()
                return self.Database.ItemsForFuelCategory[self.Prototype.name] --
                :Select(function(item) return self.Database:GetItem(nil, item) end)
            end,
        },
    }

    function self:SortAll() end

    return self

end

return FuelCategory

