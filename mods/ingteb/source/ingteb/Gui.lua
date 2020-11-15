local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local function GetNumberValueForSprite(target)
    if target.amount_min and target.amount_max and target.probability and not target.amount then
        return (target.amount_max + target.amount_min) / 2 * target.probability
    end
    if not target.amount_min and not target.amount_max and target.probability and target.amount then
        return target.amount * target.probability
    end
    if target.amount_min or target.amount_max or target.probability then
        local a = b
    end
    return target.amount
end

local function CreateSpriteAndRegister(frame, target, style)
    local result =
        frame.add {
        type = "sprite-button",
        tooltip = target.details or Helper.GetLocalizeName(target),
        sprite = Helper.FormatSpriteName(target),
        number = GetNumberValueForSprite(target),
        show_percent_for_small_numbers = target.probability ~= nil,
        style = style or "slot_button"
    }

    global.Current.Links[result.index] = target
    return result
end

local function GetPropertyStyle(property)
    if property.type == "utility" and property.name == "clock" then
        return
    end
    if property.type == "technology" then
        local data = Helper.GetPrototype(property)
        if not data then
            return
        end

        if data.researched then
            return
        end

        if property.hasPrerequisites then
            return "red_slot_button"
        end

        return Constants.GuiStyle.LightButton
    end
    if property.type == "recipe" then
        local data = Helper.GetPrototype(property)
        if not data.enabled then
            return "red_slot_button"
        end
        if data.category == "crafting" and property.amount then
            return Constants.GuiStyle.LightButton
        end
    end

    return
end

local function CreateRecipeLine(frame, target, inCount, outCount)
    local subFrame = frame.add {type = "flow", direction = "horizontal"}
    local inPanel = subFrame.add {name = "in", type = "flow", direction = "horizontal"}

    for _ = target.In:Count() + 1, inCount do
        inPanel.add {type = "sprite", style = Constants.GuiStyle.UnButton}
    end

    target.In:Select(
        function(item)
            return CreateSpriteAndRegister(inPanel, item)
        end
    )

    local properties =
        subFrame.add {
        type = "flow",
        direction = "horizontal"
    }
    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}

    target.Properties:Select(
        function(property)
            return CreateSpriteAndRegister(properties, property, GetPropertyStyle(property))
        end
    )

    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}
    local outPanel = subFrame.add {name = "out", type = "flow", direction = "horizontal"}

    target.Out:Select(
        function(item)
            return CreateSpriteAndRegister(outPanel, item)
        end
    )

    for _ = target.Out:Count() + 1, outCount do
        outPanel.add {type = "sprite", style = Constants.GuiStyle.UnButton}
    end
end

local function CreateCraftingGroupPane(frame, target, inCount, outCount)
    frame.add {type = "line", direction = "horizontal"}

    local actors =
        frame.add {
        type = "flow",
        style = Constants.GuiStyle.CenteredFlow,
        direction = "horizontal"
    }

    target.Actors:Select(
        function(actor)
            return CreateSpriteAndRegister(actors, actor)
        end
    )

    frame.add {type = "line", direction = "horizontal"}

    target.Recipes:Select(
        function(recipe)
            CreateRecipeLine(frame, recipe, inCount, outCount)
        end
    )

    frame.add {type = "line", direction = "horizontal"}
end

local function CreateCraftingGroupsPane(frame, target, headerSprites)
    if not target or not target:Any() then
        return
    end

    local subFrame =
        frame.add {
        type = "frame",
        horizontal_scroll_policy = "never",
        direction = "vertical"
    }

    local labelFlow =
        subFrame.add {
        type = "flow",
        direction = "horizontal",
        style = Constants.GuiStyle.CenteredFlow
    }

    headerSprites:Select(
        function(sprite)
            labelFlow.add {type = "sprite", sprite = sprite}
        end
    )

    local inCount =
        target:Select(
        function(group)
            return group.Recipes:Select(
                function(recipe)
                    return recipe.In:Count()
                end
            ):Max()
        end
    ):Max()

    local outCount =
        target:Select(
        function(group)
            return group.Recipes:Select(
                function(recipe)
                    return recipe.Out:Count()
                end
            ):Max()
        end
    ):Max()

    target:Select(
        function(group)
            CreateCraftingGroupPane(subFrame, group, inCount, outCount)
        end
    )
end

local function CreateMainPanel(frame, target)
    frame.caption = Helper.GetLocalizeName(target.Target)

    local scrollframe =
        frame.add {
        type = "scroll-pane",
        horizontal_scroll_policy = "never",
        direction = "vertical",
        name = "frame"
    }

    local mainFrame = scrollframe
    if target.In:Any() and target.Out:Any() then
        mainFrame = scrollframe.add {type = "frame", direction = "horizontal", name = "frame"}
    end

    if target.In:Any() or target.Out:Any() then
        local targetSprite = Helper.FormatSpriteName(target.Target)

        CreateCraftingGroupsPane(
            mainFrame,
            target.In,
            Array:new {targetSprite, "utility/go_to_arrow", "utility/missing_icon"}
        )

        CreateCraftingGroupsPane(
            mainFrame,
            target.Out,
            Array:new {"utility/missing_icon", "utility/go_to_arrow", targetSprite}
        )
    else
        local none = mainFrame.add {type = "frame", direction = "horizontal"}
        none.add {
            type = "label",
            caption = "[img=utility/crafting_machine_recipe_not_unlocked][img=utility/go_to_arrow]"
        }
        CreateSpriteAndRegister(none, target.Target)
        none.add {
            type = "label",
            caption = "[img=utility/go_to_arrow][img=utility/crafting_machine_recipe_not_unlocked]"
        }
    end
end

local result = {}

function result.SelectTarget()
    return Helper.ShowFrame(
        "Selector",
        function(frame)
            frame.caption = "select"
            frame.add {type = "choose-elem-button", elem_type = "signal"}
        end
    )
end

function result.Main(data)
    return Helper.ShowFrame(
        "Main",
        function(frame)
            return CreateMainPanel(frame, data)
        end
    )
end

return result
