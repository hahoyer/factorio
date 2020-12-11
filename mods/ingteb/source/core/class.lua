local class = require("core.classclass")
local ValueCache = require("core.ValueCache2")

--- installs the cache for a cached property
--- @param instance table will be patched to contain metatable, property, inherited and cache , if required
--- @param classInstance class
--- @param name string then property name
--- @param getter function the function that calulates the actual value
function class.addCachedProperty(instance, classInstance, name, getter)
    local className = classInstance.name
    if not rawget(instance, "cache") then rawset(instance, "cache", {}) end
    if not instance.cache[className] then instance.cache[className] = {} end
    instance.cache[className][name] = ValueCache:new(instance, getter)
end

return class
