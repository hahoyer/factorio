local Constants = require("Constants")
local Common = require("Common")

local StackOfGoods = Common:class("StackOfGoods")

function StackOfGoods:new(goods, amounts, database)
    assert(goods)
    local self = Common:new(goods.Prototype, database)
    self.object_name = StackOfGoods.object_name
    assert(
        self.Prototype.object_name == "LuaItemPrototype" or self.Prototype.object_name
            == "LuaFluidPrototype"
    )

    self.Goods = goods
    self.Amounts = amounts
    self.SpriteType = goods.SpriteType
    self.UsePercentage = self.Amounts.probability ~= nil

    self:properties{
        NumberOnSprite = {
            cache = true,
            get = function()
                local amounts = self.Amounts
                if not amounts then return end

                local probability = (amounts.probability or 1)
                local value = amounts.value

                if not value then
                    if not amounts.min then
                        value = amounts.max
                    elseif not amounts.max then
                        value = amounts.min
                    else
                        value = (amounts.max + amounts.min) / 2
                    end
                elseif type(value) ~= "number" then
                    return
                end

                return value * probability
            end,
        },
        CommonKey = {get = function() return self.Goods.CommonKey end},
        SpriteName = {get = function() return self.Goods.SpriteName end},
    }

    return self

end

return StackOfGoods
