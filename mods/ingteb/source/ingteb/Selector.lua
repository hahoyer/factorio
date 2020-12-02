local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Database = require("ingteb.Database")
local UI = require("core.UI")

ColumnCount = 12 

local Selector = {}

---@param data Dictionary Dictionary where the key is searched
---@param key string
---@param value any Value to use if key is not jet contained in data
---@return any the value stored at key
local function EnsureKey(data, key, value)
    local result = data[key]
    if not result then
        result = value or {}
        data[key] = result
    end
    return result
end

--- @param frame LuaGuiElement
--- @param targets Array | nil
function Selector:new(frame, targets)
    self.Frame = frame
    self.Targets = targets
    frame.caption = {"ingteb_utility.selector"}

    if #targets > 0 then
        self:ShowTargets()
    else    
        self:ShowAllItems()
    end
end

function Selector:ShowTargets()

    local frame = self.Frame.add {type = "flow", direction = "vertical"}
    local targetPanel = frame.add {type = "table", column_count = ColumnCount}
    frame.add {type = "line", direction = "horizontal"}
    local recent = frame.add {type = "table", column_count = ColumnCount}

    self.Targets:Select(
        function(target)
            targetPanel.add {
                type = "sprite-button",
                sprite = target.SpriteName,
                name = target.CommonKey,
                tooltip = target.LocalisedName,
            }
        end
     )

    return frame
end

function Selector:ShowAllItems()

    self.Targets = game.item_prototypes
    local groups = Dictionary:new{}
    local maximalColumns = 0
    for key, item in pairs(self.Targets) do
        local group = EnsureKey(groups, item.group.name, Dictionary:new{})
        local subgroup = EnsureKey(group, item.subgroup.name, Array:new{})
        subgroup:Append(item)
        if maximalColumns < subgroup:Count() then maximalColumns = subgroup:Count() end
    end

    local groupPanel = self.Frame.add {type = "tabbed-pane"}

    groups:Select(
        function(group)
            local groupHeader = group[next(group)][1].group
            local tab = groupPanel.add {
                type = "tab",
                caption = "[item-group=" .. groupHeader.name .. "]",
                style = "ingteb-big-tab",
                tooltip = groupHeader.localised_name,
            }
            local itemPanel = groupPanel.add {type = "flow", direction = "vertical"}

            groupPanel.add_tab(tab, itemPanel)
            group:Select(
                function(subgroup)
                    local itemline = itemPanel.add {type = "table", column_count = ColumnCount}
                    subgroup:Select(
                        function(item)
                            itemline.add {
                                type = "sprite-button",
                                sprite = "item." .. item.name,
                                name = "Item." .. item.name,
                                tooltip = item.localised_name,
                            }
                        end
                    )
                end
            )
        end
    )
    return groups
end

return Selector
