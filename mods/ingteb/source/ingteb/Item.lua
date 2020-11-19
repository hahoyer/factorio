local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")

function Item(name, prototype, database)
    local self = CommonThing(name, prototype, database)
    self.class_name = "Item"
    self.SpriteType = "item"

    if self.Name:find("mini") then --
        local x = y
    end

    function self:Setup()
        if self.Prototype.place_result then
            self.Entity = self.Database.Entities[self.Prototype.place_result
                              .name]
        end
    end

    return self
end

