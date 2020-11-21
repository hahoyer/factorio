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

    self.property.FunctionHelp = {
        get = function(self) --
            if self.IsResearched and self.NumberOnSprite then
                return {
                    "ingteb_utility.craft",
                    {"control-keys.alt"},
                    {"control-keys.control"},
                    {"control-keys.shift"},
                    {"control-keys.mouse-button-1-alt-1"},
                    {"control-keys.mouse-button-2-alt-1"},
                }
            end
        end,
    }

    self.property.Technology = {
        get = function(self)
            if self.Technologies:Count() <= 1 then return self.Technologies:Top() end

            local researched = self.Technologies:Where(
                function(technology) return technology.IsResearched end
            )
            if researched:Count() > 0 then return researched:Top() end

            local ready = self.Technologies:Where(
                function(technology) return technology.IsReady end
            )
            if ready:Count() > 0 then return researched:Top() end

            return self.Technologies:Top()
        end,
    }

    self.property.IsResearched = {
        get = function(self)
            return --
            not self.Technologies:Any() --
                or self.Technologies:Any(
                    function(technology) return technology.IsResearched end
                )
        end,
    }

    self.property.NumberOnSprite = {
        get = function(self)
            if not self.HandCrafter then return end
            local result = global.Current.Player.get_craftable_count(self.Name)
            if result > 0 then return result end
        end,
    }

    function self:IsBefore(other)
        if self == other then return false end

        if self.class_name ~= other.class_name then return false end

        if self.IsResearched ~= other.IsResearched then return self.IsResearched end
        if not self.IsResearched and self.Technology.IsReady ~= other.Technology.IsReady then
            return self.Technology.IsReady
        end
        if self.Prototype.group ~= other.Prototype.group then
            return self.Prototype.group.order < other.Prototype.group.order
        end
        if self.Prototype.subgroup ~= other.Prototype.subgroup then
            return self.Prototype.subgroup.order < other.Prototype.subgroup.order
        end

        return self.Prototype.order < other.Prototype.order
    end

    self.property.Order = {get = function(self) return self.IsResearched and 1 or 0 end}
    self.property.SubOrder = {
        get = function(self)
            return (not self.Technology or self.Technology.IsReady) and 1 or 0
        end,
    }

    self.IsDynamic = true

    self.property.SpriteStyle = {
        get = function(self)
            if not self.IsResearched then return "red_slot_button" end
            if self.NumberOnSprite then return Constants.GuiStyle.LightButton end
        end,
    }

    function self:Setup()
        local category = self.Prototype.category .. " crafting"
        self.Category = self.Database.Categories[category]

        local isHidden = false

        self.In = Array:new(self.Prototype.ingredients) --
        :Select(
            function(ingredient)
                local result = database:GetItemSet(ingredient)
                if result then
                    result.Item.In:AppendForKey(category, self)
                else
                    isHidden = true
                end
                return result
            end
        ) --
        :Where(function(value) return not (value.flags and value.flags.hidden) end) --

        self.Out = Array:new(self.Prototype.products) --
        :Select(
            function(product)
                local result = database:GetItemSet(product)
                if result then
                    result.Item.Out:AppendForKey(category, self)
                else
                    isHidden = true
                end
                return result
            end
        ) --
        :Where(function(value) return value end) --

        self.IsHidden = isHidden

        if isHidden then return end

        self.Category.Recipes:Append(self)

        self.HandCrafter = self.Category.Workers:Where(
            function(worker) return worker.Name == "character" end
        ):Top()
    end

    return self
end

