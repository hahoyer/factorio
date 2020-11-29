local PropertyProvider = require("core.PropertyProvider")

local ValueCacheRaw = {class_name = "ValueCacheRaw"}

function ValueCacheRaw:new(getValueFunction)
    local result = {getValueFunction = getValueFunction}
    setmetatable(result, self)
    self.__index = self

    return result
end

function ValueCacheRaw:get_Value()
    self:Ensure()
    return self.value
end

function ValueCacheRaw:get_IsValid() return self.isValid end

function ValueCacheRaw:set_IsValid(value)
    if value == self:get_IsValid() then return end
    if value then
        self:Ensure()
    else
        self:Reset()
    end
end

function ValueCacheRaw:Ensure()
    if not self.isValid then
        self.value = self:getValueFunction()
        self.isValid = true
    end
end

function ValueCacheRaw:Reset()
    if self.isValid then
        self.value = nil
        self.isValid = false
    end
end

function ValueCache(getter)
    local result = PropertyProvider:new{valueCache = ValueCacheRaw:new(getter)}
    result.class_name = "ValueCache"

    result.property.IsValid = {
        get = function(self) return self.valueCache:get_IsValid() end,
        set = function(self, value) self.valueCache:set_IsValid(value) end,
    }

    result.property.Value = {get = function(self) return self.valueCache:get_Value() end}

    return result
end

return ValueCache
