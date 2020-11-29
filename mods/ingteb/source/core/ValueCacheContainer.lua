local PropertyProvider = require("core.PropertyProvider")
local ValueCache = require("core.ValueCache")

local ValueCacheContainer = {class_name = "ValueCacheContainer"}

function ValueCacheContainer:new(target)
    local result = PropertyProvider:new(target)
    result.cache = {}

    function result:addCachedProperty(name, getter)
        self.cache[name] = ValueCache(getter)
        self.property[name] = {get = function(self) return self.cache[name].Value end}
    end

    return result

end

return ValueCacheContainer
