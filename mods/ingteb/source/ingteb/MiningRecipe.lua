local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
local Common = require("ingteb.Common")

local MiningRecipe = Common:class("MiningRecipe")

local function GetCategoryAndRegister(self, domain, category)
    local result = self.Database:GetCategory(domain .. "." .. category)
    return result
end

function MiningRecipe:new(name, prototype, database)
    local self = Common:new(prototype or game.entity_prototypes[name], database)
    self.object_name = MiningRecipe.object_name

    self.TypeOrder = 2
    self.SpriteType = "entity"
    self.Time = self.Prototype.mineable_properties.mining_time

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

    local configuration = self.Prototype.mineable_properties
    assert(release or configuration and configuration.minable)

    local domain = "mining"
    if not self.Prototype.resource_category then domain = "hand-mining" end
    if configuration.required_fluid then domain = "fluid-mining" end
    local category = self.Prototype.resource_category or "steel-axe"

    self.Category = GetCategoryAndRegister(self, domain, category)

    self.Resource = self.Database:GetEntity(nil, self.Prototype)
    self.Resource.UsedBy:AppendForKey(self.Category.Name, self)

    self.Input = Array:new{self.Resource}
    if configuration.required_fluid then
        local fluid = self.Database:GetStackOfGoods{
            type = "fluid",
            name = configuration.required_fluid,
            amount = configuration.fluid_amount,
        }
        fluid.Goods.UsedBy:AppendForKey(self.Category.Name, self)
        self.Input:Append(fluid)
    end

    self.IsHidden = false
    self.Output = Array:new(configuration.products) --
    :Select(
        function(product)
            local result = database:GetStackOfGoods(product)
            if result then
                result.Goods.CreatedBy:AppendForKey(self.Category.Name, self)
            else
                self.IsHidden = true
            end
            return result
        end
    )


    function self:IsBefore(other)
        if self == other then return false end
        return self.OrderValue < other.OrderValue
    end

    function self:SortAll() end

    return self
end

return MiningRecipe
