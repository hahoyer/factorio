local event = require("__flib__.event")

local result = {}

function result.ActualType(target)
    local type = target.type
    if type == "resource" or type == "tree" then
        return "entity"
    end
    return type
end

function result.FormatSpriteName(target)
    if not target.name then return end
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
            return game.technology_prototypes[name]
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

function result.SetHandler(name, handler, register)
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

function result.SetHandlers(list)
    list:Select(
        function(command, key)
            result.SetHandler(key, command[1], command[2])
        end
    )
end

return result
