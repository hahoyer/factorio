local class = {}

function class:new(name, super)
    local classInstance = {name = name, metatable = {}, property = {}}

    local metatable = classInstance.metatable

    function metatable:__index(key)
        local accessors = classInstance.property[key]
        if accessors then
            return accessors.get(self)
        elseif classInstance[key] ~= nil then
            return classInstance[key]
        elseif super then
            return super.metatable.__index(self, key)
        else
            return nil
        end
    end

    function metatable:__newindex(key, value)
        local accessors = classInstance.property[key]
        if accessors then
            return accessors.set(self, value)
        elseif super then
            return super.metatable.__newindex(self, key, value)
        else
            rawset(self, key, value)
        end
    end

    function classInstance:adopt(instance)
        return setmetatable(instance, self.metatable)
    end

    function classInstance:adopt(instance)
        return setmetatable(instance, self.metatable)
    end

    function classInstance:properties(list)
        for key, value in pairs(list) do
            if value.cache then
                class:addCachedProperty(key, value.get)
            else
                self.property[key] = {get = value.get, set = value.set}
            end

        end
    end

    return classInstance
end

return class
