local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")

function MiningRecipe(resource, database)
    local self = Common(resource.Name, resource.Prototype, database)
    self.class_name = "MiningRecipe"
    self.SpriteType = "entity"

    self.Time = self.Prototype.mineable_properties.mining_time

    function self:Setup()
        local configuration = self.Prototype.mineable_properties
        if not configuration or not configuration.minable then return end
        local category = (self.Prototype.resource_category or " hand") --
                             .. (configuration.required_fluid and " fluid" or "") --
        .. " mining"

        self.Category = self.Database.Categories[category]

        local isHidden = false

        resource.In:AppendForKey( category, self)
        self.In = Array:new{resource}

        if configuration.required_fluid then
            local fluid = self.Database:GetItemSet{
                type = "fluid",
                name = configuration.required_fluid,
                amount = configuration.fluid_amount,
            }
            fluid.Item.In:AppendForKey(category, self)
            self.In:Append(fluid)
        end

        self.Out = Array:new(configuration.products) --
        :Select(
            function(product)
                local result = database:GetItemSet(product)
                if result then  result.Item.Out:AppendForKey(category, self) else isHidden = true end
                return result
            end
        )

        self.IsHidden = isHidden

        if isHidden then return end

        self.Category.Recipes:Append(self)
    end

    self.property.Order = {get = function(self) return 1 end}
    self.property.SubOrder = {get = function(self) return 1 end}

    function self:IsBefore(other)
        if self == other then return false end
        if self.class_name ~= other.class_name then return true end
        if self.Prototype.group ~= other.Prototype.group then
            return self.Prototype.group.order < other.Prototype.group.order
        end
        if self.Prototype.subgroup ~= other.Prototype.subgroup then
            return self.Prototype.subgroup.order < other.Prototype.subgroup.order
        end

        return self.Prototype.order < other.Prototype.order
    end

    return self
end

