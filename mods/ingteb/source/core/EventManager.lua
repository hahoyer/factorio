local event = require("__flib__.event")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local class = require("core.class")
local UI = require("core.UI")

-- __DebugAdapter.breakpoint(mesg:LocalisedString)
local EventManager = class:new("EventManager")

EventManager.EventDefinesByIndex = Dictionary:new(defines.events) --
:ToDictionary(function(value, key) return {Key = value, Value = key} end) --
:ToArray()

function EventManager:new()
    local self = EventManager:adopt{}
    
    self:properties{
        Player = {
            get = function() return UI.Player end,
            set = function(_, value)
                if value then
                    local acutalValue = --
                    type(value) == "number" and game.players[value] --
                    or type(value) == "table" and value.object_name == "LuaPlayer" and value --
                        or assert(release)
                    if acutalValue == UI.Player then return end
                    UI.Player = acutalValue
                else
                    UI.Player = nil
                end
            end,
        },
    }
    return self
end

function EventManager:Watch(handler, eventId)
    return function(...)
        self:Enter(eventId, ...)
        local result = handler(self, ...)
        self:Leave(eventId)
        return result
    end
end

function EventManager:Enter(name, event) self.Active = {name, self.Active} end

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

