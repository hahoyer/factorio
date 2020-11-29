local PropertyProvider = require("core.PropertyProvider")

local FunctionCache = {object_name = "FunctionCache"}

function FunctionCache:new(getValueFunction)
    local result = {getValueFunction = getValueFunction}
    setmetatable(result, self)
    self.__index = self

    return result
end

function FunctionCache:GetValue(key) return self:Ensure(key) end

function FunctionCache:IsValid(key) return self.value[key] ~= nil end

function FunctionCache:Ensure(key)
    local value = self.value[key]
    if not value then
        value = self:getValueFunction()
        self.value = {value}
    end
    return value[1]
end

function FunctionCache:Reset(key) self.value[key] = nil end

return FunctionCache
