local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCacheContainer = require("core.ValueCacheContainer")
local UI = require("core.UI")

local Common = {object_name = "Common"}

function Common:class(name) return {object_name = name} end

function Common:new(prototype, database)
    assert(release or prototype)
    assert(release or database)

    local self = ValueCacheContainer:new{Prototype = prototype, Database = database}
    function self:properties(list)
        for key, value in pairs(list) do
            if value.cache then
                self:addCachedProperty(key, value.get)
            else
                self.property[key] = {get = value.get, set = value.set}
            end

        end
    end

    self.object_name = Common.object_name
    self.Name = self.Prototype.name
    self.LocalizedDescription = self.Prototype.localised_description

    self:properties{
        CommonKey = {get = function() return self.object_name .. "." .. self.Name end},
        ClickTarget = {get = function() return self.CommonKey end},
        Group = {get = function() return self.Prototype.group end},
        SubGroup = {get = function() return self.Prototype.subgroup end},

        LocalisedName = {
            get = function()
                return {
                    "gui-text-tags.following-text-" .. (self.TypeForLocalisation or self.SpriteType),
                    self.Prototype.localised_name,
                }
            end,
        },

        SpecialFunctions = {get = function() return Array:new() end},
        AdditionalHelp = {get = function() return Array:new{} end},
        FunctionalHelp = {
            get = function()
                return self.SpecialFunctions:Select(
                    function(specialFunction)
                        return (not specialFunction.IsAvailable or specialFunction.IsAvailable()) --
                                   and specialFunction.HelpText --
                                   and UI.GetHelpTextForButtons(specialFunction.HelpText,specialFunction.UICode) or nil
                    end
                )
            end,
        },
        HasLocalisedDescription = {get = function() end},

        HelperText = {
            get = function()

                local name = self.Prototype.localised_name
                local lines = Array:new{""}
                local function append(line)
                    if line then
                        lines:Append("\n")
                        lines:Append(line)
                    end
                end
                -- append(self.LocalizedDescription)
                self.AdditionalHelp:Select(append)
                self.FunctionalHelp:Select(append)
                return {"", name, lines}
            end,
        },
        SpriteName = {
            chache = true,
            get = function() return self.SpriteType .. "/" .. self.Prototype.name end,
        },

        RichTextName = {get = function() return "[img=" .. self.SpriteName .. "]" end},
    }
    function self:GetHandCraftingRequest(event) end
    function self:GetResearchRequest(event) end

    function self:SealUp()
        self:SortAll()
        return self
    end

    function self:GetAction(event)
        for _, specialFunction in pairs(self.SpecialFunctions) do
            if UI.IsMouseCode(event, specialFunction.UICode) then
                if (not specialFunction.IsAvailable or specialFunction.IsAvailable()) then
                    return specialFunction.Action(event)
                end
            end
        end
    end

    return self

end

return Common
