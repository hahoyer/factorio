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

function Helper.ShowFrame(player, name, create)
    local frame = player.gui.screen
    local main = frame[name]
    if main then
        main.clear()
    else
        main = frame.add {type = "frame", name = name, direction = "vertical", style = "ingteb-main-frame"}
    end
    create(main)
    player.opened = main
    if global.Location[name] then
        main.location = global.Location[name]
    else
        main.force_auto_center()
    end
    return main
end

function Helper.OnClose(name, frame) global.Location[name] = frame.location end

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

return Helper
