local event = require("__flib__.event")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local result = {}

function result.GetActualType(type)
    if type == "item" or type == "fluid" or type == "technology" or type == "entity" or type
        == "recipe" then return type end
    if type == "tool" then return "technology" end
    return "entity"
end

function result.FormatSpriteName(target)
    if target.name then return result.GetActualType(target.type) .. "." .. target.name end
end

function result.FormatRichText(target)
    return "[" .. result.GetActualType(target) .. "=" .. target.name .. "]"
end

function result.HasForce(type) return type == "technology" or type == "recipe" end

function result.GetForce(type, name)
    if type == "technology" then return global.Current.Player.force.technologies[name] end

    if type == "recipe" then return global.Current.Player.force.recipes[name] end

    assert()
end

function result.ShowFrame(player, name, create)
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

function result.DeepEqual(a, b)
    if not a then return not b end
    if not b then return false end
    if type(a) ~= type(b) then return false end
    if type(a) ~= "table" then return a == b end

    local keyCache = {}

    for key, value in pairs(a) do
        keyCache[key] = true
        if not result.DeepEqual(value, b[key]) then return false end
    end

    for key, _ in pairs(b) do if not keyCache[key] then return false end end

    return true

end

function result.SpriteStyleFromCode(code)
    return code == true and "ingteb-light-button" --
    or code == false and "red_slot_button" --
    or "slot_button"
end

local function UpdateGui(list, target)
    local helperText = target.HelperText
    local number = target.NumberOnSprite
    local style = result.SpriteStyleFromCode(target.SpriteStyle)

    list:Select(
        function(guiElement)
            guiElement.tooltip = helperText
            guiElement.number = number
            guiElement.style = style
        end
    )
end

function result.RefreshMainInventoryChanged(dataBase)
    global.Current.Gui --
    :Where(function(_, target) return target.object_name == "Recipe" end) --
    :Select(function(target)return dataBase:GetProxy(target.object_name, target.name)end) --
    :Select(UpdateGui) --
end

function result.RefreshStackChanged(dataBase) end

function result.RefreshResearchChanged(dataBase)
    global.Current.Gui --
    :Where(function(_, target) return target.object_name == "Technology" end) --
    :Select(function(target)return dataBase:GetProxy(target.object_name, target.name)end) --
    :Select(UpdateGui) --
end

local function RefreshDescription(this, dataBase)
    global.Current.Gui --
    :Where(function(_, target) return target == this end) --
    :Select(function(target)return dataBase:GetProxy(target.object_name, target.name)end) --
    :Select(UpdateGui) --
end

function result.InitiateTranslation()
    local pending = global.Current.PendingTranslation:Top()
    if pending then assert(global.Current.Player.request_translation {pending.Key}) end
end

function result.CompleteTranslation(event, dataBase)
    local complete = global.Current.PendingTranslation[event.localised_string]
    global.Current.PendingTranslation[event.localised_string] = nil

    if event.translated then
        local thing = complete.Value
        thing.HasLocalisedDescriptionPending = false
        RefreshDescription(thing, dataBase)
    end
    result.InitiateTranslation()
end

return result
