local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local UI = require("core.UI")

ColumnCount = 12

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

local Selector = {}
--- @param frame LuaGuiElement
--- @param targets Array | nil
function Selector:new(frame, targets)
    self.Frame = frame
    frame.caption = {"ingteb-utility.selector"}

    if #targets > 0 then
        self:ShowTargets(targets)
    else
        -- self:ShowSelectionForAllItems()
        self:ShowAllItems()
    end

end

function Selector:ShowTargets(targets)

    local frame = self.Frame.add {type = "flow", direction = "vertical"}
    local targetPanel = frame.add {type = "table", column_count = ColumnCount}
    frame.add {type = "line", direction = "horizontal"}
    local recent = frame.add {type = "table", column_count = ColumnCount}

    targets:Select(
        function(target)
            if target.SpriteType == "fuel-category" then
                targetPanel.add {
                    type = "sprite-button",
                    sprite = target.Name,
                    name = target.CommonKey,
                    tooltip = target.LocalisedName,
                }
            else
                local targetBox = targetPanel.add {
                    type = "choose-elem-button",
                    elem_type = target.SpriteType,
                    name = target.CommonKey,
                }
                targetBox.elem_value = target.Name
                targetBox.locked = true
            end
        end
    )

    return frame
end

local SelectorCache = {}
function SelectorCache.EnsureGroups()
    local self = SelectorCache
    if not self.Groups then
        local maximalColumns = 0
        self.Groups = Dictionary:new{}
        local targets = {game.item_prototypes, game.fluid_prototypes}
        for _, domain in pairs(targets) do
            for _, goods in pairs(domain) do
                local group = EnsureKey(self.Groups, goods.group.name, Dictionary:new{})
                local subgroup = EnsureKey(group, goods.subgroup.name, Array:new{})
                subgroup:Append(goods)
                if maximalColumns < subgroup:Count() then
                    maximalColumns = subgroup:Count()
                end
            end
        end
        self.ColumnCount =
            maximalColumns < ColumnCount and maximalColumns or self.Groups:Count() * 2
    end
    return self.Groups
end

function Selector:ShowAllItems()
    local groups = SelectorCache:EnsureGroups()
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

            local scrollframe = itemPanel.add {
                type = "scroll-pane",
                horizontal_scroll_policy = "never",
                direction = "vertical",
            }

            local itemPanel = scrollframe.add {
                type = "flow",
                direction = "vertical",
                style = "ingteb-flow-fill",
            }

            group:Select(
                function(subgroup)
                    local itemline = itemPanel.add {
                        type = "table",
                        column_count = SelectorCache.ColumnCount,
                    }
                    subgroup:Select(
                        function(goods)
                            itemline.add {
                                type = "sprite-button",
                                sprite = (goods.object_name == "LuaItemPrototype" and "item"
                                    or "fluid") .. "." .. goods.name,
                                name = (goods.object_name == "LuaItemPrototype" and "Item" or "Fluid")
                                    .. "." .. goods.name,
                                tooltip = goods.localised_name,
                            }
                        end
                    )
                end
            )
        end
    )
    return groups
end

function Selector:ShowSelectionForAllItems()
    return self.Frame.add {type = "choose-elem-button", elem_type = "signal"}
end

return Selector
