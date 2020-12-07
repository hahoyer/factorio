local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
local Common = require("ingteb.Common")
local StackOfGoods = require("ingteb.StackOfGoods")

local Bonus = Common:class("Bonus")

function Bonus:new(name, prototype, database)
    local self = Common:new(prototype, database)
    self.object_name = "Bonus"
    self.Name = name
    self.SpriteType = "utility"
    self.UsedBy = Dictionary:new{}
    self.CreatedBy = Array:new{}
    self.Input = Array:new{}
    self.Output = Array:new{}
    self.UsePercentage = true

    self:properties{

    NumberOnSprite = {
        get = function()
            return self.Prototype.modifier
        end,
    },

    }
    function self:SortAll() end

    return self
end

function BonusSet(bonus, amounts, database)
    local self = ItemSet(bonus, {value = amounts, probability = 1}, database)
    self.object_name = "BonusSet"
    return self
end

return Bonus
