local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")
require("ingteb.ItemSet")

function Bonus(name, database)
    local prototype = {
        name = (name .. "-modifier-icon"):gsub("-", "_"),
        localised_name = {"gui-bonus." .. name},
        localised_description = {"modifier-description." .. name},
    }
    local self = Common(name, prototype, database)
    self.object_name = "Bonus"
    self.SpriteType = "utility"
    self.UsedBy = Dictionary:new{}
    self.CreatedBy = Array:new{}

    function self:Setup() end

    return self
end

function BonusSet(bonus, amounts, database)
    local self = ItemSet(bonus, {value = amounts, probability = 1}, database)
    self.object_name = "BonusSet"
    return self
end

