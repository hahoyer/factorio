local PropertyProvider = {object_name = "PropertyProvider"}

function PropertyProvider:__index(index)
    local functions = self.property[index]
    if functions then return functions.get(self) end
    return rawget(self, index)
end

function PropertyProvider:__newindex(index, value)
    local functions = self.property[index]
    if functions then
        functions.set(self, value)
        return value
    end
    rawset(self, index, value)
    return value
end

function PropertyProvider:addProperty(name, getter, setter)
    self.property[name] = {get = getter, set = setter}
end

function PropertyProvider:new(result)
    if not result then result = {} end
    result.property = {}

    setmetatable(result, self)
    return result
end

return PropertyProvider
