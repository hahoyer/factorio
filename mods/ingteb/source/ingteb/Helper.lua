local event = require("__flib__.event")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local Helper = {}

function Helper.GetActualType(type)
    if type == "item" or type == "fluid" or type == "technology" or type == "entity" or type
        == "recipe" then return type end
    if type == "tool" then return "technology" end
    return "entity"
end

function Helper.FormatSpriteName(target)
    if target.name then return Helper.GetActualType(target.type) .. "." .. target.name end
end

function Helper.FormatRichText(target)
    return "[" .. Helper.GetActualType(target) .. "=" .. target.name .. "]"
end

function Helper.HasForce(type) return type == "technology" or type == "recipe" end

function Helper.GetForce(type, name)
    if type == "technology" then return global.Current.Player.force.technologies[name] end
    if type == "recipe" then return global.Current.Player.force.recipes[name] end

    assert(release)
end

function Helper.ShowFrame(player, name, create)
    local frame = player.gui.screen
    local main = frame[name]
    if main then
        main.clear()
    else
        main = frame.add {type = "frame", name = name, direction = "vertical"}
    end
    create(main)
    player.opened = main
    if global.Current.Location[name] then
        main.location = global.Current.Location[name]
    else
        main.force_auto_center()
    end
    return main
end

function Helper.OnClose(name, frame) global.Current.Location[name] = frame.location end

function Helper.DeepEqual(a, b)
    if not a then return not b end
    if not b then return false end
    if type(a) ~= type(b) then return false end
    if type(a) ~= "table" then return a == b end

    local keyCache = {}

    for key, value in pairs(a) do
        keyCache[key] = true
        if not Helper.DeepEqual(value, b[key]) then return false end
    end

    for key, _ in pairs(b) do if not keyCache[key] then return false end end

    return true

end

function Helper.SpriteStyleFromCode(code)
    return code == true and "ingteb-light-button" --
    or code == false and "red_slot_button" --
    or "slot_button"
end

local function UpdateGui(list, target, dataBase)
    target = dataBase:GetProxy(target.object_name, target.Name)
    local helperText = target.HelperText
    local number = target.NumberOnSprite
    local style = Helper.SpriteStyleFromCode(target.SpriteStyle)

    for _, guiElement in pairs(list) do
        guiElement.tooltip = helperText
        guiElement.number = number
        guiElement.style = style
    end
end

function Helper.RefreshMainInventoryChanged(dataBase)
    Dictionary:new(global.Current.Gui) --
    :Where(function(_, target) return target.object_name == "Recipe" end) --
    :Select(function(list, target) UpdateGui(list, target, dataBase) end) --
end

function Helper.RefreshStackChanged(dataBase) end

function Helper.RefreshResearchChanged(dataBase)
    Dictionary:new(global.Current.Gui) --
    :Where(function(_, target) return target.object_name == "Technology" end) --
    :Select(function(list, target) UpdateGui(list, target, dataBase) end) --
end

local function RefreshDescription(this, dataBase)
    Dictionary:new(global.Current.Gui) --
    :Where(function(_, target) return target == this end) --
    :Select(function(target) return dataBase:GetProxy(target.object_name, target.name) end) --
    :Select(function(list, target) UpdateGui(list, target, dataBase) end) --
end

function Helper.InitiateTranslation()
    local pending = global.Current.PendingTranslation:Top()
    if pending then assert(global.Current.Player.request_translation {pending.Key}) end
end

function Helper.CompleteTranslation(event, dataBase)
    local complete = global.Current.PendingTranslation[event.localised_string]
    global.Current.PendingTranslation[event.localised_string] = nil

    if event.translated then
        local thing = complete.Value
        thing.HasLocalisedDescriptionPending = false
        RefreshDescription(thing, dataBase)
    end
    Helper.InitiateTranslation()
end

return Helper
