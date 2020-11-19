local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local function CreateSpriteAndRegister(frame, target, style)
    local result

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

    global.Current.Links[result.index] = target and target.Item
    return result
end

local function GetPropertyStyle(property)
    if property.type == "utility" and property.name == "clock" then return end
    if property.type == "technology" then
        local data = Helper.GetFactorioData(property)
        if not data then return end

        if data.researched then return end

        if property.hasPrerequisites then return "red_slot_button" end

        return Constants.GuiStyle.LightButton
    end
    if property.type == "recipe" then
        local data = Helper.GetFactorioData(property)
        if not data.enabled then return "red_slot_button" end
        if data.category == "crafting" and property.amount then return Constants.GuiStyle.LightButton end
    end

    return
end

local function GetTechnologyStyle(property)
    if not property or property.IsResearched then return end
    if property.IsReady then return Constants.GuiStyle.LightButton end
    return "red_slot_button"

end

local function GetRecipeStyle(property)
    if not property.IsResearched then return "red_slot_button" end
    if property.NumberOnSprite then return Constants.GuiStyle.LightButton end
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

    CreateSpriteAndRegister(properties, target.Technology, GetTechnologyStyle(target.Technology))
    CreateSpriteAndRegister(properties, target, GetRecipeStyle(target))
    CreateSpriteAndRegister(properties, {SpriteName = "utility/clock", NumberOnSprite = target.Time})

    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}
    local outPanel = subFrame.add {name = "out", type = "flow", direction = "horizontal"}

    target.Out:Select(function(item) return CreateSpriteAndRegister(outPanel, item) end)

    for _ = target.Out:Count() + 1, outCount do --
        outPanel.add {type = "sprite", style = Constants.GuiStyle.UnButton}
    end
end

local function CreateCraftingGroupPane(frame, target, key, inCount, outCount)
    frame.add {type = "line", direction = "horizontal"}

    local workersPane = frame.add {type = "flow", style = Constants.GuiStyle.CenteredFlow, direction = "horizontal"}

    local workers = target[1].Database.WorkingEntities[key]
    workers:Select(function(worker) return CreateSpriteAndRegister(workersPane, worker) end)

    frame.add {type = "line", direction = "horizontal"}

    target:Select(function(recipe) CreateRecipeLine(frame, recipe, inCount, outCount) end)

    frame.add {type = "line", direction = "horizontal"}
end

local function CreateCraftingGroupsPane(frame, target, headerSprites)
    if not target or not target:Any() then return end

    local targetArray = target:ToArray(function(value, key) return {value = value, key = key} end)
    targetArray:Sort(
        function(a, b)
            if a == b then return false end
            local aOrder = a.value:Select(function(recipe) return recipe.Order end):Sum()
            local bOrder = b.value:Select(function(recipe) return recipe.Order end):Sum()
            if aOrder ~= bOrder then return aOrder > bOrder end

            local aSubOrder = a.value:Select(function(recipe) return recipe.SubOrder end):Sum()
            local bSubOrder = b.value:Select(function(recipe) return recipe.SubOrder end):Sum()
            return aSubOrder > bSubOrder

        end
    )

    local subFrame = frame.add {type = "frame", horizontal_scroll_policy = "never", direction = "vertical"}

    local labelFlow = subFrame.add {type = "flow", direction = "horizontal", style = Constants.GuiStyle.CenteredFlow}

    headerSprites:Select(function(sprite) labelFlow.add {type = "sprite", sprite = sprite} end)

    local inCount = target:Select(
        function(group) return group:Select(function(recipe) return recipe.In:Count() end):Max() end
    ):Max()

    local outCount = target:Select(
        function(group) return group:Select(function(recipe) return recipe.Out:Count() end):Max() end
    ):Max()

    targetArray:Select(
        function(pair)
            pair.value:Sort(function(a, b) return a:IsBefore(b) end)
            CreateCraftingGroupPane(subFrame, pair.value, pair.key, inCount, outCount)
        end
    )
end

local function CreateMainPanel(frame, target)
    frame.caption = target.LocalisedName

    local scrollframe = frame.add {
        type = "scroll-pane",
        horizontal_scroll_policy = "never",
        direction = "vertical",
        name = "frame",
    }

    local mainFrame = scrollframe
    local columnCount = (target.CraftingRecipes:Any() and 1 or 0) + --
    (target.In:Any() and 1 or 0) + --
    (target.Out:Any() and 1 or 0)

    if columnCount > 1 then mainFrame = scrollframe.add {type = "frame", direction = "horizontal", name = "frame"} end

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

    assert(not target.CraftingRecipes:Any())

    CreateCraftingGroupsPane(
        mainFrame, target.In, Array:new{target.SpriteName, "utility/go_to_arrow", "utility/missing_icon"}
    )

    CreateCraftingGroupsPane(
        mainFrame, target.Out, Array:new{"utility/missing_icon", "utility/go_to_arrow", target.SpriteName}
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

function result.Main(data) return Helper.ShowFrame("Main", function(frame) return CreateMainPanel(frame, data) end) end

return result
