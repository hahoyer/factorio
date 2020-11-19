local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")

function Fluid(name, prototype, database)
    local self = CommonThing(name, prototype, database)
    self.class_name = "Fluid"
    self.SpriteType = "fluid"

    function self:Setup() end

    return self
end

