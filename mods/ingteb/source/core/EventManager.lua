local event = require("__flib__.event")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local class = require("core.class")

-- __DebugAdapter.breakpoint(mesg:LocalisedString)
local EventManager = class:new("EventManager")

EventManager.EventDefinesByIndex = Dictionary:new(defines.events) --
:ToDictionary(function(value, key) return {Key = value, Value = key} end) --
:ToArray()

function EventManager:Watch(handler, eventId)
    return function(...)
        self:Enter(eventId, ...)
        local result = handler(self, ...)
        self:Leave(eventId)
        return result
    end
end

function EventManager:Enter(name, event)
    assert(
        name == "on_gui_closed" and event.gui_type == 3 --
        or not self.Active --
    )
    self.Active = {name, self.Active}
end

function EventManager:Leave(name) self.Active = self.Active[2] end

function EventManager:SetHandler(eventId, handler, register)
    if not self.State then self.State = {} end
    if not handler then register = false end
    if register == nil then register = true end

    local name = type(eventId) == "number" and self.EventDefinesByIndex[eventId] or eventId

    self.State[name] = "activating..." .. tostring(register)

    if register == false then handler = nil end
    local watchedEvent = handler and self:Watch(handler, name) or nil

    local eventRegistrar = event[eventId]
    if eventRegistrar then
        eventRegistrar(watchedEvent)
    else
        event.register(eventId, watchedEvent)
    end

    self.State[name] = register
end

function EventManager:SetHandlers(list)
    list:Select(function(command, key) self:SetHandler(key, command[1], command[2]) end)
end

return EventManager

