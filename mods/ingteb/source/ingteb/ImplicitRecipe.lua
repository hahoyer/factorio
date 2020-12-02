local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
local Common = require("ingteb.Common")

local ImplicitRecipe = Common:class("ImplicitRecipe")
local MiningRecipe = Common:class("MiningRecipe")
local BoilingRecipe = Common:class("BoilingRecipe")

local function GetCategoryAndRegister(self, domain, category)
    local result = self.Database:GetCategory(domain .. "." .. category)
    return result
end

function MiningRecipe:new(prototype, database)
    local self = Common:new(prototype, database)
    self.object_name = MiningRecipe.object_name
    self.SpriteType = "entity"
    self.TypeOrder = 2

    self.Time = self.Prototype.mineable_properties.mining_time

    local configuration = self.Prototype.mineable_properties
    assert(release or configuration and configuration.minable)

    local domain = "mining"
    if not self.Prototype.resource_category then domain = "hand-mining" end
    if configuration.required_fluid then domain = "fluid-mining" end
    local category = self.Prototype.resource_category or "steel-axe"

    self.Category = GetCategoryAndRegister(self, domain, category)

    self.Resource = self.Database:GetEntity(nil, prototype)
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

    self:properties{}

    return self
end

function BoilingRecipe:new(prototype, database)
    assert(release or prototype.name == "steam")
    local self = Common:new(prototype, database)
    self.object_name = BoilingRecipe.object_name
    self.SpriteType = "fluid"
    self.TypeOrder = 2.1
    self.Time = 1
    self.Category = GetCategoryAndRegister(self, "boiling", prototype.name)

    local input = self.Database:GetStackOfGoods{type = "fluid", amount = 60, name = "water"}
    input.Goods.UsedBy:AppendForKey(self.Category.Name, self)
    self.Input = Array:new{input}

    local output = self.Database:GetStackOfGoods{type = "fluid", amount = 60, name = "steam"}
    output.Goods.CreatedBy:AppendForKey(self.Category.Name, self)
    self.Output = Array:new{output}

    return self
end

function ImplicitRecipe:new(name, prototype, database)
    assert(release or name)
    local _, _, domain, prototypeName = name:find("^(.+)%.(.*)$")
    assert(release or not prototype or prototypeName == prototype.name)

    local self --
    = (domain == "mining" or domain == "fluid-mining" or domain == "hand-mining") --
          and MiningRecipe:new(prototype or game.entity_prototypes[prototypeName], database) --
    or domain == "boiling" and BoilingRecipe:new(prototype, database) --
    self.Domain = domain

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

return ImplicitRecipe
