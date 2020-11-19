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

        self:AppendForKey(category, resource.In)
        self.In = Array:new{resource}

        if configuration.required_fluid then
            local fluid = self.Database:GetItemSet {
                type = "fluid",
                name = configuration.required_fluid,
                amount = configuration.fluid_amount,
            }
            self:AppendForKey(category, fluid.Item.In)
            self.In:Append(fluid)
        end

        self.Out = Array:new(configuration.products) --
        :Select(
            function(product)
                local result = database:GetItemSet(product)
                self:AppendForKey(category, result.Item.Out)
                return result
            end
        )

        self.WorkingEntities = database.WorkingEntities[category]

        self.WorkingEntities:Select(function(entity) entity.CraftingRecipes:Append(self) end)
    end

    return self
end

