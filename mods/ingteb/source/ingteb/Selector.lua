local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Database = require("ingteb.Database")
local UI = require("core.UI")

local Selector = {}

local function EnsureKey(data, key, value)
    local result = data[key]
    if not result then
        result = value or {}
        data[key] = result
    end
    return result
end

function Selector:new(frame, targets)
    frame.caption = {"ingteb_utility.selector"}

    assert(not targets)

    targets = game.item_prototypes
    local groups = Dictionary:new{}
    local maximalColumns = 0
    for key, item in pairs(targets) do
        local group = EnsureKey(groups, item.group.name, Dictionary:new{})
        local subgroup = EnsureKey(group, item.subgroup.name, Array:new{})
        subgroup:Append(item)
        if maximalColumns < subgroup:Count() then maximalColumns = subgroup:Count() end
    end

    local groupPanel = frame.add {type = "tabbed-pane"}

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
                    local itemline = itemPanel.add {type = "table", column_count = 10}
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
end

return Selector
