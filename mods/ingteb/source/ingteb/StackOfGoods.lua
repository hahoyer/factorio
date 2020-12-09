local Constants = require("Constants")
local Common = require("Common")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local StackOfGoods = Common:class("StackOfGoods")

function StackOfGoods:new(goods, amounts, database)
    assert(release or goods)
    local self = Common:new(goods.Prototype, database)
    self.object_name = StackOfGoods.object_name
    assert(
        release or self.Prototype.object_name == "LuaItemPrototype" or self.Prototype.object_name
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
        ClickTarget = {get = function() return self.Goods.ClickTarget end},
        CommonKey = {
            get = function() return self.Goods.CommonKey .. "/" .. self:GetAmountsKey() end,
        },
        SpriteName = {get = function() return self.Goods.SpriteName end},

        AdditionalHelp = {
            get = function() if self.Goods then return self.Goods.FuelDescription end end,
        },

    }

    function self:GetAmountsKey()
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

        return tostring(value * probability)

    end

    function self:Clone(patchAmountsFunction)
        local newAmounts = Dictionary:new(self.Amounds):Clone()
        patchAmountsFunction(newAmounts)
        return StackOfGoods:new(self.Goods, newAmounts, self.Database)
    end
    return self

end

return StackOfGoods
