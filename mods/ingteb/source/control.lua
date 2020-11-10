local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local s = 1 --/w

local currentFrame
local currentLinks

local function on_gui_closed(player)
    if currentFrame then
        currentFrame.destroy()
        game.tick_paused = false
        currentFrame = nil
        currentLinks = nil
        player.opened = nil
    end
end

local function FormatSpriteName(target)
    local type = target.type
    if type == "resource" then
        type = "entity"
    end
    return type .. "." .. target.name
end

local function FormatRichText(target)
    local type = target.type
    if type == "resource" then
        type = "entity"
    end
    return "[" .. type .. "=" .. target.name .. "]"
end

local function GetPrototype(target)
    local name = target.Target.name
    local type = target.Target.type
    if type == "utility" then
        return
    end
    if type == "item" then
        local result = game.item_prototypes[name]
        if result then
            return result
        end
        local x = q / w
    end
    if type == "fluid" then
        local result = game.fluid_prototypes[name]
        if result then
            return result
        end
        local x = q / w
    end
    if type == "resource" or type == "entity" then
        local result = game.entity_prototypes[name]
        if type == "entity" or result.type == type then
            return result
        end
        local x = q / w
    end
    local x = q / w
    local result = game.item_prototypes[name] or game.fluid_prototypes[name] or game.entity_prototypes[name]
    if result then
        return result
    end
    local x = 1
end

local function GetSpriteButton(target)
    if target.Target.type == "resourceaaa" then
        local x = 2
    end

    local item = GetPrototype(target)

    return {
        type = "sprite-button",
        tooltip = item and item.localised_name,
        sprite = FormatSpriteName(target.Target),
        number = target.Target.amount
    }
end

local function SpreadHandMiningRecipe(prototype)
    return {
        In = Array:new {
            {
                Target = {type = prototype.type, name = prototype.name}
            }
        },
        Properties = Array:new {
            {
                Target = {type = "utility", name = "clock", amount = prototype.mineable_properties.mining_time}
            }
        },
        Out = Array:new(prototype.mineable_properties.products):Select(
            function(itemData)
                return {Target = itemData}
            end
        )
    }
end

local function SpreadRecipe(receipe)
    return {
        In = Array:new(receipe.ingredients):Select(
            function(itemData)
                return {Target = itemData}
            end
        ),
        Properties = Array:new {
            {
                Target = {type = "utility", name = "clock", amount = receipe.energy}
            }
        },
        Out = Array:new(receipe.products):Select(
            function(itemData)
                return {Target = itemData}
            end
        )
    }
end

local function SpreadHandMining(target, player)
    local prototype = game.entity_prototypes[target.name]

    if prototype.mineable_properties.minable then
        local modifier = player.character_mining_speed_modifier
        if modifier == 0 then
            modifier = nil
        end
        return {
            Actors = Array:new {{Target = {type = "entity", name = "character", amount = modifier}}},
            Recipes = Array:new {SpreadHandMiningRecipe(prototype)}
        }
    end
end

local function SpreadResource(target, player)
    local groups = Table.Array:new {}
    local handMining = SpreadHandMining(target, player)
    if handMining then
        groups:Append(handMining)
    end

    local item = game.item_prototypes[target.name]
    return {
        Target = target,
        In = groups
    }
end

local function SpreadActors(key)
    return Dictionary:new(game.entity_prototypes):Where(
        function(entity)
            return entity.crafting_categories and entity.crafting_categories[key]
        end
    ):ToArray():Select(
        function(entity)
            local target = {name = entity.name, type = "entity", amount = entity.crafting_speed}
            return {Target = target}
        end
    )
end

local function SpreadItemGroup(target, key)
    local actors = SpreadActors(key)
    return {
        Actors = actors,
        Recipes = target:Select(
            function(recipe)
                return SpreadRecipe(recipe)
            end
        )
    }
end

local function SpreadItemIn(target, player)
    return Dictionary:new(game.recipe_prototypes):Where(
        function(item)
            return Array:new(item.ingredients):Any(
                function(this)
                    return this.name == target.name and this.type == target.type
                end
            )
        end
    ):ToArray():ToGroup(
        function(recipe)
            return {Key = recipe.category, Value = recipe}
        end
    ):Select(
        function(group, key)
            return SpreadItemGroup(group, key, player)
        end
    )
end

local function SpreadItemOut(target, player)
    return Dictionary:new(game.recipe_prototypes):Where(
        function(item)
            return Array:new(item.products):Any(
                function(this)
                    return this.name == target.name and this.type == target.type
                end
            )
        end
    ):ToArray():ToGroup(
        function(recipe)
            return {Key = recipe.category, Value = recipe}
        end
    ):Select(
        function(group, key)
            return SpreadItemGroup(group, key, player)
        end
    )
end

local function SpreadItem(target, player)
    return {Target = target, In = SpreadItemIn(target, player), Out = SpreadItemOut(target, player)}
end

local function SpreadEntity(target, player)
    local entity = game.entity_prototypes[target.name]
    if not entity then
        return
    end
    local candidates = entity.items_to_place_this
    if not candidates or #candidates == 0 then
        return
    end
    local item = candidates[1]
    return SpreadItem({type = "item", name = item.name}, player)
end

local function Spread(target, player)
    local result
    if target.type == "resource" or target.type == "tree" or target.type == "simple-entity" then
        result = SpreadResource(target, player)
    elseif target.type == "item" or  target.type == "fluid" then
        result = SpreadItem(target, player)
    else
        result = SpreadEntity(target, player)
    end
    return result
end

local function CreateSpriteAndRegister(frame, target)
    local result = frame.add(GetSpriteButton(target))
    currentLinks[result.index] = target.Target
    return result
end

local function CreateRecipeLine(frame, target, inCount, outCount)
    frame.add {
        type = "line",
        direction = "horizontal"
    }

    local subFrame =
        frame.add {
        type = "flow",
        direction = "horizontal"
    }

    local inPanel =
        subFrame.add {
        name = "in",
        type = "flow",
        direction = "horizontal"
    }

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
        name = "properties",
        type = "flow",
        direction = "horizontal"
    }

    properties.add {
        type = "sprite",
        sprite = "utility/go_to_arrow"
    }

    target.Properties:Select(
        function(property)
            return CreateSpriteAndRegister(properties, property)
        end
    )

    properties.add {
        type = "sprite",
        sprite = "utility/go_to_arrow"
    }

    local outPanel =
        subFrame.add {
        name = "out",
        type = "flow",
        direction = "horizontal"
    }

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
    local subFrame =
        frame.add {
        type = "frame",
        direction = "vertical"
    }

    local header =
        subFrame.add {
        name = "header",
        type = "flow",
        direction = "horizontal"
    }

    target.Actors:Select(
        function(actor)
            local result = header.add(GetSpriteButton(actor))
            currentLinks[result.index] = actor.Target
            return result
        end
    )

    target.Recipes:Select(
        function(recipe)
            CreateRecipeLine(subFrame, recipe, inCount, outCount)
        end
    )
end

local function CreateCraftingGroupsPane(frame, target, caption)
    if not target or not target:Any() then
        return
    end

    local subFrame =
        frame.add {
        type = "frame",
        horizontal_scroll_policy = "never",
        caption = caption,
        direction = "vertical"
    }

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

local function CreateGui(target, player)
    local frame = player.gui.screen.add {type = "frame", caption = "ingteb", direction = "vertical"}

    if target.Localize then
        local localize = frame.add {type = "flow", direction = "horizontal"}
        if target.Localize.Name then
            localize.add {type = "label", caption = target.Localize.Name}
        end
        if target.Localize.Description then
            localize.add {type = "label", caption = target.Localize.Description}
        end
    end

    local scrollframe =
        frame.add {
        type = "scroll-pane",
        horizontal_scroll_policy = "never",
        direction = "vertical",
        name = "frame"
    }

    local inOutframe = scrollframe.add {type = "frame", direction = "horizontal", name = "frame"}

    local targetRichText = FormatRichText(target.Target)

    CreateCraftingGroupsPane(
        inOutframe,
        target.In,
        targetRichText .. "[img=utility/go_to_arrow][img=utility/missing_icon]"
    )

    CreateCraftingGroupsPane(
        inOutframe,
        target.Out,
        "[img=utility/missing_icon][img=utility/go_to_arrow]" .. targetRichText
    )

    player.opened = frame
    currentFrame = frame
    frame.force_auto_center()
    game.tick_paused = true
end

local function OpenGui(player, targets)

    currentLinks = {}

    local index = 1
    local target = targets[index]
    if target.type == "fluid" then
        local b = 1
    end

    local data = Spread(target, player)
    if data then
        CreateGui(data, player)
    end
end

local function get_targets(player)
    local result = {}

    local cursor = player.cursor_stack
    if cursor and cursor.valid and cursor.valid_for_read then
        table.insert(result, {type = cursor.type, name = cursor.name})
    end
    local cursor = player.cursor_ghost
    if cursor then
        table.insert(result, {type = cursor.type, name = cursor.name})
    end
    local cursor = player.selected
    if cursor then
        table.insert(result, {type = cursor.type, name = cursor.name})
    end

    local cursor = player.opened
    if cursor then
        if currentLinks and currentLinks[cursor.index] then
            local target = currentLinks[cursor.index]
            table.insert(result, target)
        end
        table.insert(result, {type = cursor.type, name = cursor.name})
        if cursor.burner then
            table.insert(result, {fuel_categories = cursor.burner.fuel_categories})
        end
        if cursor.type == "mining-drill" and cursor.mining_target then
            table.insert(result, {type = cursor.mining_target.type, name = cursor.mining_target.name})
        end

        if cursor.type == "furnace" and cursor.previous_recipe then
            table.insert(result, {type = "recipe", name = cursor.previous_recipe.name})
        end
        if cursor.type == "assembling-machine" and cursor.get_recipe() then
            table.insert(result, {type = "recipe", name = cursor.get_recipe().name})
        end
    end
    if #result > 0 then
        return result
    end
    local result = nil
end

local function on_lua_shortcut(player)
    if currentFrame then
        on_gui_closed(player)
        return
    end
    local targets = get_targets(player)
    if targets then
        OpenGui(player, targets)
        local x = event
    end
end

local function on_gui_click(event)
    local target = currentLinks and currentLinks[event.element.index]
    if target then
        local player = game.players[event.player_index]
        on_gui_closed(player)
        OpenGui(player, {target})
    end
end

event.register(
    defines.events.on_gui_closed,
    function(event)
        on_gui_closed(game.players[event.player_index])
    end
)

event.register(
    "ingteb-main-key",
    function(event)
        on_lua_shortcut(game.players[event.player_index])
    end
)

event.register(
    defines.events.on_gui_click,
    function(event)
        on_gui_click(event)
    end
)
