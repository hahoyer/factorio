local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local s = 1 --/w
local s = 1 --/w

StateHandler = nil

function EnsureGlobal()
    if global.Current then
        return
    end

    global.Current = {
        Links = {},
        History = {},
        HistoryIndex = 1
    }
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
    local name = target.name
    local type = target.type
    if type == "utility" then
        return
    end
    if type == "item" then
        local result = game.item_prototypes[name]
        if result then
            return result
        end
        local x = b.point
    end
    if type == "fluid" then
        local result = game.fluid_prototypes[name]
        if result then
            return result
        end
        local x = b.point
    end
    if type == "resource" or type == "entity" then
        local result = game.entity_prototypes[name]
        if type == "entity" or result.type == type then
            return result
        end
        local x = b.point
    end
    local x = b.point
end

local function GetLocalizeName(target)
    local item = GetPrototype(target)
    return item and item.localised_name
end

local function CreateSpriteAndRegister(frame, target)
    local item = GetPrototype(target)
    local result =
        frame.add {
        type = "sprite-button",
        tooltip = GetLocalizeName(target),
        sprite = FormatSpriteName(target),
        number = target.amount
    }
    --result.mouse_button_filter = {"left", "right", "button-4", "button-5"}

    global.Current.Links[result.index] = target
    return result
end

local function SpreadHandMiningRecipe(prototype)
    return {
        In = Array:new {
            {type = prototype.type, name = prototype.name}
        },
        Properties = Array:new {
            {type = "utility", name = "clock", amount = prototype.mineable_properties.mining_time}
        },
        Out = Array:new(prototype.mineable_properties.products)
    }
end

local function SpreadRecipe(receipe)
    return {
        In = Array:new(receipe.ingredients),
        Properties = Array:new {
            {type = "utility", name = "clock", amount = receipe.energy}
        },
        Out = Array:new(receipe.products)
    }
end

local function SpreadHandMining(target)
    local prototype = game.entity_prototypes[target.name]

    if prototype.mineable_properties.minable then
        local modifier = global.Current.Player.character_mining_speed_modifier
        if modifier == 0 then
            modifier = nil
        end
        return {
            Actors = Array:new {{type = "entity", name = "character", amount = modifier}},
            Recipes = Array:new {SpreadHandMiningRecipe(prototype)}
        }
    end
end

local function SpreadResource(target)
    local groups = Table.Array:new {}
    local handMining = SpreadHandMining(target)
    if handMining then
        groups:Append(handMining)
    end

    return {Target = target, In = groups, Out = Array:new {}}
end

local function SpreadActors(key)
    return Dictionary:new(game.entity_prototypes):Where(
        function(entity)
            return entity.crafting_categories and entity.crafting_categories[key]
        end
    ):ToArray():Select(
        function(entity)
            local target = {name = entity.name, type = "entity", amount = entity.crafting_speed}
            return target
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

local function SpreadItemIn(target)
    return Dictionary:new(game.recipe_prototypes):Where(
        function(recipe)
            if recipe.hidden then
                return false
            end
            return Array:new(recipe.ingredients):Any(
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
            return SpreadItemGroup(group, key)
        end
    )
end

local function SpreadItemOut(target)
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
            return SpreadItemGroup(group, key)
        end
    )
end

local function SpreadItem(target)
    return {Target = target, In = SpreadItemIn(target), Out = SpreadItemOut(target)}
end

local function SpreadEntity(target)
    local entity = game.entity_prototypes[target.name]
    if not entity then
        return
    end
    local candidates = entity.items_to_place_this
    if not candidates or #candidates == 0 then
        return
    end
    local item = candidates[1]
    return SpreadItem({type = "item", name = item.name})
end

local function GetData(target)
    local result
    if target.type == "resource" or target.type == "tree" or target.type == "simple-entity" then
        result = SpreadResource(target)
    elseif target.type == "item" or target.type == "fluid" then
        result = SpreadItem(target)
    else
        result = SpreadEntity(target)
    end
    return result
end

local function CreateRecipeLine(frame, target, inCount, outCount)
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
    frame.add {
        type = "line",
        direction = "horizontal"
    }

    local header =
        frame.add {
        type = "flow",
        direction = "horizontal"
    }

    target.Actors:Select(
        function(actor)
            return CreateSpriteAndRegister(header, actor)
        end
    )

    frame.add {
        type = "line",
        direction = "horizontal"
    }

    target.Recipes:Select(
        function(recipe)
            CreateRecipeLine(frame, recipe, inCount, outCount)
        end
    )

    frame.add {
        type = "line",
        direction = "horizontal"
    }
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

local function ShowPanel(target)
    event.register(defines.events.on_gui_click, nil)

    local text = GetLocalizeName(target.Target)
    if not text then
        text = "ingteb"
    end

    local frame = global.Current.Player.gui.screen.add {type = "frame", caption = text, direction = "vertical"}

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
        local targetRichText = FormatRichText(target.Target)

        CreateCraftingGroupsPane(
            mainFrame,
            target.In,
            targetRichText .. "[img=utility/go_to_arrow][img=utility/missing_icon]"
        )

        CreateCraftingGroupsPane(
            mainFrame,
            target.Out,
            "[img=utility/missing_icon][img=utility/go_to_arrow]" .. targetRichText
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

    global.Current.Player.opened = frame
    global.Current.Frame = frame
    if global.Current.Location then
        frame.location = global.Current.Location
    else
        frame.force_auto_center()
    end
    --game.tick_paused = true
end

local function FindTarget()
    local result = {}

    local cursor = global.Current.Player.cursor_stack
    if cursor and cursor.valid and cursor.valid_for_read then
        return {type = cursor.type, name = cursor.name}
    end
    local cursor = global.Current.Player.cursor_ghost
    if cursor then
        return {type = cursor.type, name = cursor.name}
    end
    local cursor = global.Current.Player.selected
    if cursor then
        return {type = cursor.type, name = cursor.name}
    end

    local cursor = global.Current.Player.opened
    if cursor then
        if global.Current.Links and global.Current.Links[cursor.index] then
            local target = global.Current.Links[cursor.index]
            return target
        end
        table.insert(result, {type = cursor.type, name = cursor.name})
        if cursor.burner then
            return {fuel_categories = cursor.burner.fuel_categories}
        end
        if cursor.type == "mining-drill" and cursor.mining_target then
            return {type = cursor.mining_target.type, name = cursor.mining_target.name}
        end

        if cursor.type == "furnace" and cursor.previous_recipe then
            return {type = "recipe", name = cursor.previous_recipe.name}
        end
        if cursor.type == "assembling-machine" and cursor.get_recipe() then
            return {type = "recipe", name = cursor.get_recipe().name}
        end
    end
end

local History = {
    RemoveAll = function()
        global.Current.History = {}
        global.Current.HistoryIndex = 0
    end,
    HairCut = function(target)
        if target then
            global.Current.HistoryIndex = global.Current.HistoryIndex + 1
            while #global.Current.History >= global.Current.HistoryIndex do
                table.remove(global.Current.History, global.Current.HistoryIndex)
            end
            global.Current.History[global.Current.HistoryIndex] = target
            return target
        end
    end,
    New = function(target)
        if target then
            global.Current.History = {target}
            global.Current.HistoryIndex = 1
            return target
        end
    end,
    Back = function()
        if global.Current.HistoryIndex > 1 then
            global.Current.HistoryIndex = global.Current.HistoryIndex - 1
            return global.Current.History[global.Current.HistoryIndex]
        end
    end,
    Fore = function()
        if global.Current.HistoryIndex < #global.Current.History then
            global.Current.HistoryIndex = global.Current.HistoryIndex + 1
            return global.Current.History[global.Current.HistoryIndex]
        end
    end
}

local function CloseGui()
    if global.Current.Frame then
        global.Current.Location = global.Current.Frame.location
        global.Current.Frame.destroy()
        game.tick_paused = false
        global.Current.Frame = nil
        global.Current.Links = {}

        global.Current.Player.opened = nil
    end
end

local function OpenGui(target)
    if target then
        local data = GetData(target)
        if data then
            CloseGui()
            ShowPanel(data)
            StateHandler {panel = true}
            return target
        end
    end
end

local function RegisterMainForOpen()
    event.register(
        Constants.Key.Main,
        function(event)
            EnsureGlobal()
            global.Current.Player = game.players[event.player_index]
            local target = OpenGui(FindTarget())
            History.New(target)
        end
    )
end

local function RegisterMainForClose()
    event.register(
        Constants.Key.Main,
        function(event)
            CloseGui()
            StateHandler {panel = false}
            History.RemoveAll()
        end
    )
end

local function RegisterGuiKey(register)
    if register == false then
        event.register(defines.events.on_gui_click, nil)
    else
        event.register(
            defines.events.on_gui_click,
            function(event)
                global.Current.Player = game.players[event.player_index]
                local target = OpenGui(global.Current.Links and global.Current.Links[event.element.index])
                History.HairCut(target)
            end
        )
    end
end

local function RegisterBackNavigation(register)
    if register == false then
        event.register(Constants.Key.Back, nil)
    else
        event.register(
            Constants.Key.Back,
            function()
                OpenGui(History.Back())
            end
        )
    end
end

local function RegisterForeNavigation(register)
    if register == false then
        event.register(Constants.Key.Fore, nil)
    else
        event.register(
            Constants.Key.Fore,
            function()
                OpenGui(History.Fore())
            end
        )
    end
end

local function RegisterGuiClose(register)
    if register == false then
        event.register(defines.events.on_gui_closed, nil)
    else
        event.register(
            defines.events.on_gui_closed,
            function(event)
                CloseGui()
                StateHandler {panel = false}
            end
        )
    end
end

StateHandler = function(state)
    if state.panel then
        RegisterGuiKey()
        RegisterMainForClose()
        RegisterGuiClose()
        RegisterForeNavigation()
        RegisterBackNavigation()
    else
        RegisterGuiKey(false)
        RegisterMainForOpen()
        RegisterGuiClose(false)
        RegisterForeNavigation(false)
        RegisterBackNavigation(false)
    end
end

RegisterMainForOpen()
