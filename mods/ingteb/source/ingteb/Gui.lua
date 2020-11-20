local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local function CreateSpriteAndRegister(frame, target, style)
    local result

    if target and target.class_name == "Recipe" then local h = target.HelperText end

    if target then
        result = frame.add {
            type = "sprite-button",
            tooltip = target.HelperText,
            sprite = target.SpriteName,
            number = target.NumberOnSprite,
            show_percent_for_small_numbers = target.probability ~= nil,
            style = style or "slot_button",
        }
    else
        result = frame.add {type = "sprite-button", style = style or "slot_button"}
    end

    global.Current.Links[result.index] = target
    if target and target.IsDynamic then global.Current.Gui:AppendForKey(target, result) end
    return result
end

local function CreateRecipeLine(frame, target, inCount, outCount)
    local subFrame = frame.add {type = "flow", direction = "horizontal"}
    local inPanel = subFrame.add {name = "in", type = "flow", direction = "horizontal"}

    for _ = target.In:Count() + 1, inCount do --
        inPanel.add {type = "sprite", style = Constants.GuiStyle.UnButton}
    end

    target.In:Select(function(item) return CreateSpriteAndRegister(inPanel, item) end)

    local properties = subFrame.add {type = "flow", direction = "horizontal"}
    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}

    CreateSpriteAndRegister(properties, target.Technology, target.Technology and target.Technology.SpriteStyle)
    CreateSpriteAndRegister(properties, target, target.SpriteStyle)
    CreateSpriteAndRegister(properties, {SpriteName = "utility/clock", NumberOnSprite = target.Time})

    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}
    local outPanel = subFrame.add {name = "out", type = "flow", direction = "horizontal"}

    target.Out:Select(function(item) return CreateSpriteAndRegister(outPanel, item) end)

    for _ = target.Out:Count() + 1, outCount do --
        outPanel.add {type = "sprite", style = Constants.GuiStyle.UnButton}
    end
end

local function CreateCraftingGroupPanel(frame, target, key, inCount, outCount)
    frame.add {type = "line", direction = "horizontal"}

    local workersPane = frame.add {
        type = "flow",
        style = Constants.GuiStyle.CenteredFlow,
        direction = "horizontal",
    }

    local workers = target[1].Database.Categories[key].Workers
    workers:Select(function(worker) return CreateSpriteAndRegister(workersPane, worker) end)

    frame.add {type = "line", direction = "horizontal"}

    target:Select(function(recipe) CreateRecipeLine(frame, recipe, inCount, outCount) end)

    frame.add {type = "line", direction = "horizontal"}
end

local function CreateCraftingGroupsPanel(frame, target, headerSprites)
    if not target or not target:Any() then return end

    local targetArray = target:ToArray(function(value, key) return {Value = value, Key = key} end)
    targetArray:Sort(
        function(a, b)
            if a == b then return false end
            local aOrder = a.Value:Select(function(recipe) return recipe.Order end):Sum()
            local bOrder = b.Value:Select(function(recipe) return recipe.Order end):Sum()
            if aOrder ~= bOrder then return aOrder > bOrder end

            local aSubOrder = a.Value:Select(function(recipe) return recipe.SubOrder end):Sum()
            local bSubOrder = b.Value:Select(function(recipe) return recipe.SubOrder end):Sum()
            return aSubOrder > bSubOrder

        end
    )

    local subFrame = frame.add {
        type = "frame",
        horizontal_scroll_policy = "never",
        direction = "vertical",
    }

    local labelFlow = subFrame.add {
        type = "flow",
        direction = "horizontal",
        style = Constants.GuiStyle.CenteredFlow,
    }

    headerSprites:Select(function(sprite) labelFlow.add {type = "sprite", sprite = sprite} end)

    local inCount = target:Select(
        function(group)
            return group:Select(function(recipe) return recipe.In:Count() end):Max()
        end
    ):Max()

    local outCount = target:Select(
        function(group)
            return group:Select(function(recipe) return recipe.Out:Count() end):Max()
        end
    ):Max()

    targetArray:Select(
        function(pair)
            pair.Value:Sort(function(a, b) return a:IsBefore(b) end)
            CreateCraftingGroupPanel(subFrame, pair.Value, pair.Key, inCount, outCount)
        end
    )
end

local function CreateMainPanel(frame, target)
    frame.caption = target.LocalisedName

    local item = target.Item
    local entity = target.Entity

    local scrollframe = frame.add {
        type = "scroll-pane",
        horizontal_scroll_policy = "never",
        direction = "vertical",
        name = "frame",
    }

    local mainFrame = scrollframe
    local columnCount = (target.RecipeList:Any() and 1 or 0) + --
    (target.In:Any() and 1 or 0) + --
    (target.Out:Any() and 1 or 0)

    if columnCount > 1 then
        mainFrame = scrollframe.add {type = "frame", direction = "horizontal", name = "frame"}
    end

    if columnCount == 0 then
        local none = mainFrame.add {type = "frame", direction = "horizontal"}
        none.add {
            type = "label",
            caption = "[img=utility/crafting_machine_recipe_not_unlocked][img=utility/go_to_arrow]",
        }

        CreateSpriteAndRegister(none, target)

        none.add {
            type = "label",
            caption = "[img=utility/go_to_arrow][img=utility/crafting_machine_recipe_not_unlocked]",
        }

        return
    end

    CreateCraftingGroupsPanel(mainFrame, target.RecipeList, Array:new{target.SpriteName, "factorio"})

    CreateCraftingGroupsPanel(
        mainFrame, target.In,
            Array:new{target.SpriteName, "utility/go_to_arrow", "utility/missing_icon"}
    )

    CreateCraftingGroupsPanel(
        mainFrame, target.Out,
            Array:new{"utility/missing_icon", "utility/go_to_arrow", target.SpriteName}
    )

end

local result = {}

function result.SelectTarget()
    return Helper.ShowFrame(
        "Selector", function(frame)
            frame.caption = "select"
            frame.add {type = "choose-elem-button", elem_type = "signal"}
        end
    )
end

function result.Main(data)
    return Helper.ShowFrame("Main", function(frame) return CreateMainPanel(frame, data) end)
end

return result
