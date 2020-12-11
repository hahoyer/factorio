local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local class = require("core.class")
local UI = require("core.UI")

local Common = class:new("Common")

Common.property = {
    CommonKey = {get = function(self) return self.class.name .. "." .. self.Name end},
    ClickTarget = {get = function(self) return self.CommonKey end},
    Group = {get = function(self) return self.Prototype.group end},
    SubGroup = {get = function(self) return self.Prototype.subgroup end},

    LocalisedName = {
        get = function(self)
            return {
                "gui-text-tags.following-text-" .. (self.TypeForLocalisation or self.SpriteType),
                self.Prototype.localised_name,
            }
        end,
    },

    SpecialFunctions = {get = function(self) return Array:new() end},
    AdditionalHelp = {get = function(self) return Array:new{} end},
    FunctionalHelp = {
        get = function(self)
            return self.SpecialFunctions:Select(
                function(specialFunction)
                    return (not specialFunction.IsAvailable or specialFunction.IsAvailable()) --
                               and specialFunction.HelpText --
                               and UI.GetHelpTextForButtons(
                            specialFunction.HelpText, specialFunction.UICode
                        ) or nil
                end
            )
        end,
    },
    HasLocalisedDescription = {get = function() end},

    HelperText = {
        get = function(self)

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
        cache = true,
        get = function(self) return self.SpriteType .. "/" .. self.Prototype.name end,
    },

    RichTextName = {get = function(self) return "[img=" .. self.SpriteName .. "]" end},
}

function Common:GetHandCraftingRequest(event) end
function Common:GetResearchRequest(event) end

function Common:SealUp()
    self:SortAll()
    return self
end

function Common:GetAction(event)
    for _, specialFunction in pairs(self.SpecialFunctions) do
        if UI.IsMouseCode(event, specialFunction.UICode) then
            if (not specialFunction.IsAvailable or specialFunction.IsAvailable()) then
                return specialFunction.Action(event)
            end
        end
    end
end

function Common:new(prototype, database)
    assert(release or prototype)
    assert(release or database)

    local self = self:adopt{Prototype = prototype, Database = database}
    self.Name = self.Prototype.name
    self.LocalizedDescription = self.Prototype.localised_description

    return self

end

return Common
