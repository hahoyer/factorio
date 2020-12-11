local class = {name = "class"}

local function GetInherited(self, key)
    if not self then return end
    return self.property[key] or GetInherited(self.base, key)
end


--- Defines a class
--- @param name string the name of the class
--- @param base class optional, the base class
--- @return class new class 
function class:new(name, base)
    assert(release or type(name) == "string")
    if base then assert(release or base.class == class) end

    local classInstance = {name = name, metatable = {}, property = {}, class = class, base = base}

    local metatable = classInstance.metatable

    function metatable:__index(key)
        local accessors = classInstance.property[key]
        if accessors then
            if accessors.cache then
                return self.cache[accessors.class][key].Value
            else
                return accessors.get(self)
            end
        elseif rawget(classInstance, key) ~= nil then
            return classInstance[key]
        elseif base then
            return base.metatable.__index(self, key)
        else
            return nil
        end
    end

    function metatable:__newindex(key, value)
        local accessors = classInstance.property[key]
        if accessors then
            return accessors.set(self, value)
        elseif base then
            return base.metatable.__newindex(self, key, value)
        else
            rawset(self, key, value)
        end
    end

    --- "Adopts" any table as instance of a class by providing metatable and property setup
    --- @param self class
    --- @param instance table will be patched to contain metatable, property, inherited and cache , if required
    --- @return instance ... but patched 
    function classInstance:adopt(instance)
        instance.class = self
        setmetatable(instance, self.metatable)
        for key, value in pairs(self.property) do
            value.class = self.name
            local inherited = GetInherited(self.base, key)
            if inherited then
                if not rawget(instance, "inherited") then instance.inherited = {} end
                instance.inherited[key] = inherited
            end
            if value.cache then
                assert(release or not value.set)
                class.addCachedProperty(instance, self, key, value.get)
            end
        end
        return instance
    end

    return classInstance
end

return class
