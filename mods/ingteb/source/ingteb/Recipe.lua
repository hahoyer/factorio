local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
require("ingteb.Common")

function Recipe(name, prototype, database)
    local self = Common(name, prototype, database)
    self.class_name = "Recipe"
    self.SpriteType = "recipe"
    self.Technologies = Array:new()

    self.Time = self.Prototype.energy

    self.property.Technology = {
        get = function(self)
            if self.Technologies:Count() <= 1 then return self.Technologies:Top() end

            local researched = self.Technologies:Where(function(technology) return technology.IsResearched end)
            if researched:Count() > 0 then return researched:Top() end

            local ready = self.Technologies:Where(function(technology) return technology.IsReady end)
            if ready:Count() > 0 then return researched:Top() end

            return self.Technologies:Top()
        end,
    }

    self.property.IsResearched = {
        get = function(self)
            return --
            not self.Technologies:Any() --
            or self.Technologies:Any(function(technology) return technology.IsResearched end)
        end,
    }

    self.property.NumberOnSprite = {
        get = function(self)
            if not self.HandCrafter then return end
            return global.Current.Player.get_craftable_count(self.Name)
        end,
    }

    self.property.Order = {get = function(self) return self.IsResearched and 1 or 0 end}
    self.property.SubOrder = {get = function(self) return self.Technology.IsReady and 1 or 0 end}

    function self:Setup()
        local category = self.Prototype.category .. " crafting"
        self.In = Array:new(self.Prototype.ingredients) --
        :Select(
            function(ingredient)
                local result = database:GetItemSet(ingredient)
                self:AppendForKey(category, result.Item.In)
                return result
            end
        )

        self.Out = Array:new(self.Prototype.products) --
        :Select(
            function(product)
                local result = database:GetItemSet(product)
                self:AppendForKey(category, result.Item.Out)
                return result
            end
        )

        self.WorkingEntities = database.WorkingEntities[category]
        self.WorkingEntities:Select(function(entity) entity.CraftingRecipes:Append(self) end)

        if self.Name == ("transport-belt") then assert(true) end
        self.HandCrafter = self.WorkingEntities:Where(function(worker) return worker.Name == "character" end):Top()
    end

    function self:IsBefore(other)
        if self == other then return false end

        if (not self.Technology) ~= (not other.Technology) then return not self.Technology end
        if self.IsResearched ~= other.IsResearched then return self.IsResearched end
        if self.Technology then
            if self.Technology.IsReady ~= other.Technology.IsReady then return self.Technology.IsReady end
        end

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

