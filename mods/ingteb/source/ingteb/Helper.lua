local event = require("__flib__.event")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local result = {}
local EventDefinesByIndex = Dictionary:new(defines.events):ToDictionary(
    function(value, key) return {Key = value, Value = key} end
):ToArray()

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

function result.ShowFrame(name, create)
    local frame = global.Current.Player.gui.screen.add {
        type = "frame",
        name = name,
        direction = "vertical",
    }
    create(frame)
    global.Current.Player.opened = frame
    global.Current.Frame = frame
    if global.Current.Location[name] then
        frame.location = global.Current.Location[name]
    else
        frame.force_auto_center()
    end
    return frame
end

function result.HideFrame()
    if global.Current.Frame then
        global.Current.Location[global.Current.Frame.name] = global.Current.Frame.location
        global.Current.Frame.destroy()
        game.tick_paused = false
        global.Current.Frame = nil
        global.Current.Links = {}
        global.Current.Gui = Dictionary:new{}

        global.Current.Player.opened = nil
    end
end

function result.SetHandler(eventId, handler, register)
    if not handler then register = false end
    if register == nil then register = true end

    local name = type(eventId) == "number" and EventDefinesByIndex[eventId] or eventId

    State[name] = "activating..." .. tostring(register)

    if register == false then handler = nil end

    local eventRegistrar = event[eventId]
    if eventRegistrar then
        eventRegistrar(handler)
    else
        event.register(eventId, handler)
    end

    State[name] = register
end

function result.SetHandlers(list)
    list:Select(function(command, key) result.SetHandler(key, command[1], command[2]) end)
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

return result
