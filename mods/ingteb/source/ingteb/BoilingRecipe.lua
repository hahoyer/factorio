local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
local Common = require("ingteb.Common")

local BoilingRecipe = Common:class("BoilingRecipe")

local function GetCategoryAndRegister(self, domain, category)
    local result = self.Database:GetCategory(domain .. "." .. category)
    return result
end

function BoilingRecipe:new(name, prototype, database)
    local self = Common:new(prototype, database)
    self.object_name = BoilingRecipe.object_name
    self.Name = name
    self.TypeOrder = 2.1
    self.SpriteType = "fluid"
    self.Time = 1
    self.Category = GetCategoryAndRegister(self, "boiling", name)

    local input = self.Database:GetStackOfGoods{type = "fluid", amount = 60, name = "water"}
    input.Goods.UsedBy:AppendForKey(self.Category.Name, self)
    self.Input = Array:new{input}

    local output = self.Database:GetStackOfGoods{type = "fluid", amount = 60, name = "steam"}
    output.Goods.CreatedBy:AppendForKey(self.Category.Name, self)
    self.Output = Array:new{output}

    self:properties{
        OrderValue = {
            cache = true,
            get = function()
                return self.TypeOrder --
                .. " R R " --
                .. self.Prototype.group.order --
                .. " " .. self.Prototype.subgroup.order --
                .. " " .. self.Prototype.order
            end,
        },

    }

    function self:IsBefore(other)
        if self == other then return false end
        return self.OrderValue < other.OrderValue
    end

    function self:SortAll() end

    return self
end

return BoilingRecipe
