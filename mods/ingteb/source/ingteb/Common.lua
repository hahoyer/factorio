local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
local PropertyProvider = require("core.PropertyProvider")

function Common(name, prototype, database)
    local self = PropertyProvider:new{
        Name = name,
        Prototype = prototype,
        Database = database,
        cache = {},
    }
    self.In = Dictionary:new{}
    self.Out = Dictionary:new{}

    function self:addCachedProperty(name, getter)
        self.cache[name] = ValueCache(getter)
        self.property[name] = {get = function(self) return self.cache[name].Value end}
    end

    self:addCachedProperty("SpriteName", function() return self.SpriteType .. "/" .. self.Name end)
    self.LocalisedName = self.Prototype.localised_name
    self.property.FunctionHelp = {get = function() return end}

    self.property.HasLocalisedDescription = {
        get = function()
            if self.HasLocalisedDescriptionPending ~= nil then
                return not self.HasLocalisedDescriptionPending
            end

            global.Current.PendingTranslation:AppendForKey(
                self.Prototype.localised_description[1],
                    {Key = self.Prototype.localised_description, Value = self}
            )

            global.Current.Player.request_translation(self.Prototype.localised_description)
            self.HasLocalisedDescriptionPending = true

            return nil

        end,
    }

    self.property.HelperText = {
        get = function()
            local name = self.Prototype.localised_name
            local description = self.Prototype.localised_description
            local help = self.FunctionHelp

            local result = name
            if self.HasLocalisedDescription then
                result = {"ingteb_utility.Lines2", result, description}
            end
            if help then result = {"ingteb_utility.Lines2", result, help} end
            return result
        end,
    }

    return self
end

function CommonThing(name, prototype, database)
    local self = Common(name, prototype, database)

    self.TechnologyIngredients = Array:new{}

    return self
end
