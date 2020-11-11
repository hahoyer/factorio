local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local s = 1 --/w
local s = 1 --/w

State = {}
StateHandler = nil

local History = {
    Data = {},
    Index = 0
}

function History.RemoveAll()
    History.Data = {}
    History.Index = 0
end

function History.HairCut(target)
    if target then
        History.Index = History.Index + 1
        while #History.Data >= History.Index do
            table.remove(History.Data, History.Index)
        end
        History.Data[History.Index] = target
        return target
    end
end

function History.New(target)
    if target then
        History.Data = {target}
        History.Index = 1
        return target
    end
end

function History.Back()
    if History.Index > 1 then
        History.Index = History.Index - 1
        return History.Data[History.Index]
    end
end

function History.Fore()
    if History.Index < #History.Data then
        History.Index = History.Index + 1
        return History.Data[History.Index]
    end
end

function History.Save()
    global.Current.History = History.Data
    global.Current.HistoryIndex = History.Index
end

function History.Load()
    if global.Current then
        History.Data = global.Current.History
        History.Index = global.Current.HistoryIndex
    end
end

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
    if type == "resource" or type == "entity" then
        local result = game.entity_prototypes[name]
        if type == "entity" or result.type == type then
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

    local result = game.item_prototypes[name]
    if result then
        return result
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
    if not result then
        target.type = "item"
        result = SpreadItem(target)
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

local function ShowFrame(name, create)
    local frame = global.Current.Player.gui.screen.add {type = "frame", caption = name, direction = "vertical"}
    create(frame)
    global.Current.Player.opened = frame
    global.Current.Frame = frame
    if global.Current[name] then
        frame.location = global.Current[name]
    else
        frame.force_auto_center()
    end
    return frame
end

local function CreateMainPanel(frame, target)
    frame.caption = GetLocalizeName(target.Target)

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
end

local function SelectTarget()
    return ShowFrame(
        "select",
        function(frame)
            frame.add {type = "choose-elem-button", elem_type = "signal"}
        end
    )
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

local function CloseGui()
    if global.Current.Frame then
        global.Current[global.Current.Frame.name] = global.Current.Frame.location
        global.Current.Frame.destroy()
        game.tick_paused = false
        global.Current.Frame = nil
        global.Current.Links = {}

        global.Current.Player.opened = nil
    end
end

local function ShowMainPanel(target)
    event.register(defines.events.on_gui_click, nil)
    return ShowFrame(
        "main",
        function(frame)
            return CreateMainPanel(frame, target)
        end
    )
end

local function OpenMainGui(target)
    if target then
        local data = GetData(target)
        if data then
            CloseGui()
            ShowFrame(
                "main",
                function(frame)
                    return CreateMainPanel(frame, data)
                end
            )
            StateHandler {mainPanel = true}
            return target
        end
    end
end

--------------------------------------------------------------------------

local function SetHandler(name, handler, register)
    if register == nil then
        register = true
    end
    local eventId = name
    local eventFunction = "register"

    if name:find("^on_gui_") then
        eventId = defines.events[name]
    elseif name == "on_load" then
        eventId = nil
        eventFunction = name
    end

    State[name] = "activating..." .. tostring(register)

    if register == false then
        handler = nil
    end

    if eventId then
        event[eventFunction](eventId, handler)
    else
        event[eventFunction](handler)
    end

    State[name] = register
end

local function SetHandlers(list)
    list:Select(
        function(command, key)
            SetHandler(key, command[1], command[2])
        end
    )
end

--------------------------------------------------------------------------

function MainForOpen(event)
    EnsureGlobal()
    global.Current.Player = game.players[event.player_index]
    local target = FindTarget()
    if not target then
        SelectTarget()
        StateHandler {selectPanel = true}
        return
    end

    target = OpenMainGui(target)
    History.New(target)
end

function ForeNavigation()
    OpenMainGui(History.Fore())
end

function BackNavigation()
    OpenMainGui(History.Back())
end

local function GuiClickForMain(event)
    global.Current.Player = game.players[event.player_index]
    local target = OpenMainGui(global.Current.Links and global.Current.Links[event.element.index])
    History.HairCut(target)
end

local function GuiElementChangedForSelect(event)
    global.Current.Player = game.players[event.player_index]
    StateHandler {selectPanel = false}
    OpenMainGui(event.element.elem_value)
end

local function GuiClose()
    CloseGui()
    StateHandler {mainPanel = false}
end

local function MainForClose()
    CloseGui()
    StateHandler {mainPanel = false, selectPanel = false}
    History.RemoveAll()
end

local function MainForOpen(event)
    EnsureGlobal()
    global.Current.Player = game.players[event.player_index]
    local target = FindTarget()
    if not target then
        SelectTarget()
        StateHandler {selectPanel = true}
        return
    end

    target = OpenMainGui(target)
    History.New(target)
end

local function Load()
    History.Load()
end

StateHandler = function(state)
    state.mainPanel = state.mainPanel == true
    state.selectPane = state.selectPane == true

    local handlers = Dictionary:new {}

    handlers[Constants.Key.Fore] = {ForeNavigation, state.mainPanel}
    handlers[Constants.Key.Back] = {BackNavigation, state.mainPanel}

    handlers[Constants.Key.Main] =
        ((state.mainPanel or state.selectPane) and {MainForClose, "close mode"}) or {MainForOpen, "open mode"}

    handlers["on_gui_click"] = {GuiClickForMain, state.mainPanel}
    handlers["on_gui_elem_changed"] = {GuiElementChangedForSelect, state.selectPanel}
    handlers["on_gui_closed"] = {GuiClose, state.mainPanel or state.selectPane}

    SetHandlers(handlers)
    History.Save()
end

SetHandler(Constants.Key.Main, MainForOpen, "open mode")
SetHandler("on_load", Load)
