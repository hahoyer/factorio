local class = require("core.classclass")

local ValueCache = class:new("ValueCache")

ValueCache.property = {
    IsValid = {
        get = function(self) return self:get_IsValid(self.Client) end,
        set = function(self, value) self:set_IsValid(self.Client, value) end,
    },
    Value = {get = function(self) return self:get_Value(self.Client) end},
}

function ValueCache:new(client, getValueFunction)
    local instance = self:adopt{getValueFunction = getValueFunction}
    instance.Client = client
    return instance
end

function ValueCache:get_Value(client)
    self:Ensure(client)
    return self.value
end

function ValueCache:get_IsValid() return self.isValid end

function ValueCache:set_IsValid(client, value)
    if value == self:get_IsValid() then return end
    if value then
        self:Ensure(client)
    else
        self:Reset()
    end
end

function ValueCache:Ensure(client)
    if not self.isValid then
        self.value = rawget(self,"getValueFunction")(client)
        self.isValid = true
    end
end

function ValueCache:Reset()
    if self.isValid then
        self.value = nil
        self.isValid = false
    end
end

return ValueCache
