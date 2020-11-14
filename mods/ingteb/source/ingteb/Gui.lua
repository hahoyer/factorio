local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local function CreateSpriteAndRegister(frame, target, style)
    local item = Helper.GetPrototype(target)
    local result =
        frame.add {
        type = "sprite-button",
        tooltip = Helper.GetLocalizeName(target),
        sprite = Helper.FormatSpriteName(target),
        number = target.amount,
        style = style
    }

    global.Current.Links[result.index] = target
    return result
end

local function GetPropertyStyle(property)
    if property.type == "utility" and property.name == "clock" then
        return "slot_button"
    end
    if property.type == "technology" then
        return "slot_button"
    end
    if property.type == "recipe" then
        if property.amount and property.cache.Prototype.Value.category == "crafting" then
            return Constants.GuiStyle.LightButton
        end
        return "red_slot_button"
    end

    return
end

local function CreateRecipeLine(frame, target, inCount, outCount)
    local subFrame = frame.add {type = "flow", direction = "horizontal"}
    local inPanel = subFrame.add {name = "in", type = "flow", direction = "horizontal"}

    for _ = target.In:Count() + 1, inCount do
        inPanel.add {type = "sprite-button"}
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
        outPanel.add {type = "sprite-button"}
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
        local targetRichText = Helper.FormatSpriteName(target.Target)

        CreateCraftingGroupsPane(
            mainFrame,
            target.In,
            Array:new {targetRichText, "utility/go_to_arrow", "utility/missing_icon"}
        )

        CreateCraftingGroupsPane(
            mainFrame,
            target.Out,
            Array:new {"utility/missing_icon", "utility/go_to_arrow", targetRichText}
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