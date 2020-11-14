local event = require("__flib__.event")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local result = {}
local EventDefinesByIndex =
    Dictionary:new(defines.events):ToDictionary(
    function(value, key)
        return {Key = value, Value = key}
    end
):ToArray()

function result.ActualType(target)
    local type = target.type
    if type == "resource" or type == "tree" then
        return "entity"
    end
    return type
end

function result.FormatSpriteName(target)
    if not target.name then
        return
    end
    return result.ActualType(target) .. "." .. target.name
end

function result.FormatRichText(target)
    return "[" .. result.ActualType(target) .. "=" .. target.name .. "]"
end

function result.GetPrototype(target)
    local function GetValue()
        local name = target.name
        local type = result.ActualType(target)
        if type == "utility" then
            return
        end

        if type == "technology" then
            return global.Current.Player.force.technologies[name]
        end

        if type == "entity" then
            return game.entity_prototypes[name]
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

        local result = game.entity_prototypes[name]
        if result then
            return result
        end

        local x = b.point
    end

    if not target.cache then
        target.cache = {}
    end
    if not target.cache.Prototype then
        target.cache.Prototype = {Value = GetValue()}
    end
    return target.cache.Prototype.Value
end

function result.GetLocalizeName(target)
    local item = result.GetPrototype(target)
    return item and item.localised_name
end

function result.ShowFrame(name, create)
    local frame = global.Current.Player.gui.screen.add {type = "frame", name = name, direction = "vertical"}
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

        global.Current.Player.opened = nil
    end
end

function result.SetHandler(eventId, handler, register)
    if register == nil then
        register = true
    end

    local name = type(eventId) == "number" and EventDefinesByIndex[eventId] or eventId

    State[name] = "activating..." .. tostring(register)

    if register == false then
        handler = nil
    end

    local eventRegistrar = event[eventId]
    if eventRegistrar then
        eventRegistrar(handler)
    else
        event.register(eventId, handler)
    end
   
    State[name] = register
end

function result.SetHandlers(list)
    list:Select(
        function(command, key)
            result.SetHandler(key, command[1], command[2])
        end
    )
end

return result
