local Dictionary = {}
local Array = {}

function Dictionary:Clone(predicate)
    local result = Dictionary:new{}
    for key, value in pairs(self) do if not predicate or predicate(value, key) then result[key] = value end end
    return result
end

function Array:Clone(predicate)
    local result = Array:new{}
    for index, value in ipairs(self) do if predicate(value, index) then result:Append(value) end end
    return result
end

function Dictionary:Where(predicate) return self:Clone(predicate) end
function Array:Where(predicate) return self:Clone(predicate) end

function Array:Sort(order) table.sort(self, order) end

function Dictionary:new(target)
    if not target then target = {} end
    if getmetatable(target) == "private" then target = Dictionary.Clone(target) end
    setmetatable(target, self)
    self.__index = self
    return target
end

function Array:new(target)
    if not target then target = {} end
    if getmetatable(target) == "private" then target = Array.Clone(target) end
    setmetatable(target, self)
    self.__index = self
    return target
end

function Dictionary:Select(transformation)
    local result = Dictionary:new{}
    for key, value in pairs(self) do result[key] = transformation(value, key) end
    return result
end

function Array:Select(transformation)
    local result = Array:new{}
    for key, value in ipairs(self) do result:Append(transformation(value, key)) end
    return result
end

function Dictionary:Sum(predicate)
    local result = 0
    for key, value in pairs(self) do if not predicate or predicate(value, key) then result = result + value end end
    return result
end

function Dictionary:Max()
    local result
    for key, value in pairs(self) do if not result or result < value then result = value end end
    return result
end

function Dictionary:Min()
    local result
    for key, value in pairs(self) do if not result or result > value then result = value end end
    return result
end

function Array:Sum(predicate)
    local result = 0
    for key, value in ipairs(self) do if not predicate or predicate(value, key) then result = result + value end end
    return result
end

function Array:Max()
    local result
    for key, value in ipairs(self) do if not result or result < value then result = value end end
    return result
end

function Array:Min()
    local result
    for key, value in ipairs(self) do if not result or result > value then result = value end end
    return result
end

function Dictionary:Count(predicate)
    local result = 0
    for key, value in pairs(self) do if not predicate or predicate(value, key) then result = result + 1 end end
    return result
end

function Array:Count(predicate)
    local result = 0
    for key, value in ipairs(self) do if not predicate or predicate(value, key) then result = result + 1 end end
    return result
end

function Dictionary:All(predicate)
    for key, value in pairs(self) do if not predicate(value, key) then return false end end
    return true
end

function Array:All(predicate)
    for key, value in ipairs(self) do if not predicate(value, key) then return false end end
    return true
end

function Dictionary:Any(predicate)
    for key, value in pairs(self) do if not predicate or predicate(value, key) then return true end end
    return nil
end

function Array:Any(predicate)
    for key, value in ipairs(self) do if not predicate or predicate(value, key) then return true end end
    return nil
end

function Array:Contains(item)
    for key, value in ipairs(self) do if value == item then return true end end
    return nil
end

function Dictionary:ToDictionary(getPair)
    local result = Dictionary:new{}
    for key, value in pairs(self) do
        if getPair then
            local pair = getPair(value, key)
            result[pair.Key] = pair.Value
        else
            result[key] = value
        end
    end
    return result
end

function Array:ToDictionary(getPair)
    local result = Dictionary:new{}
    for key, value in ipairs(self) do
        if getPair then
            local pair = getPair(value, key)
            result[pair.Key] = pair.Value
        else
            result[key] = value
        end
    end
    return result
end

function Array:ToGroup(getPair)
    local result = Dictionary:new{}
    for key, value in ipairs(self) do
        if getPair then
            local pair = getPair(value, key)
            local current = result[pair.Key] or Array:new{}
            current:Append(pair.Value)
            result[pair.Key] = current
        else
            local current = result[key] or Array:new{}
            current:Append(value)
            result[key] = current
        end
    end
    return result
end

function Dictionary:ToArray(getItem)
    local result = Array:new{}
    for key, value in pairs(self) do result:Append(getItem and getItem(value, key) or value) end
    return result
end

function Array:ToArray(getItem)
    local result = Array:new{}
    for key, value in ipairs(self) do result:Append(getItem and getItem(value, key) or value) end
    return result
end

function Array:Top(allowEmpty, allowMultiple, onEmpty, onMultiple)
    if #self == 0 then
        if allowEmpty == false or onEmpty then error(onEmpty and onEmpty() or "Array contains no element.") end
        return
    elseif #self > 1 then
        if allowMultiple == false or onMultiple then
            error(onMultiple and onMultiple(#self) or "Array contains more than one element (" .. #self .. ").")
        end
    end
    return self[1]
end

function Dictionary:Top(allowEmpty, allowMultiple, onEmpty, onMultiple)
    local result
    for _, value in pairs(self) do
        if allowMultiple ~= false then return value end
        if result then
            error(onMultiple and onMultiple(#self) or "Array contains more than one element (" .. #self .. ").")
        end
        result = {value = value}
    end

    if result then return result.value end

    if allowEmpty == false or onEmpty then error(onEmpty and onEmpty() or "Array contains no element.") end
end

function Array:Bottom(allowEmpty, allowMultiple, onEmpty, onMultiple)
    if #self == 0 then
        if allowEmpty == false or onEmpty then error(onEmpty and onEmpty() or "Array contains no element.") end
        return
    elseif #self > 1 then
        if allowMultiple == false or onMultiple then
            error(onMultiple and onMultiple(#self) or "Array contains more than one element (" .. #self .. ").")
        end
    end
    return self[#self]
end

function Array:Concat(otherArray)
    if not self:Any() then return otherArray end
    if not otherArray:Any() then return self end

    local result = Array:new{}
    result:AppendMany(self)
    result:AppendMany(otherArray)
    return result
end

function Array:ConcatMany()
    local result = Array:new{}
    for _, values in ipairs(self) do for _, value in ipairs(values) do result:Append(value) end end
    return result
end

function Dictionary:Concat(other, combine)
    if not self:Any() then return other end
    if not other:Any() then return self end

    local result = self:Clone()
    for key, value in pairs(other) do result[key] = result[key] and combine(result[key], value) or value end
    return result
end

function Array:Take(count)
    if #self <= count then return self end

    local result = Array:new{}
    for index = 1, count do result:Append(self[index]) end
    return result
end

function Array:Skip(count)
    if count <= 0 then return self end
    local result = Array:new{}
    for index = 1 + count, #self do result:Append(self[index]) end
    return result
end

function Array:Append(value) return table.insert(self, value) end
function Array:AppendMany(values) for _, value in ipairs(values) do table.insert(self, value) end end

function Array:InsertAt(position, value) return table.insert(self, position, value) end

local Result = {Array = Array, Dictionary = Dictionary}

function Result.new(self, target)
    if #target == 0 and next(target) then
        return self.Dictionary:new(target)
    elseif not next(target, #target > 0 and #target or nil) then
        return self.Array:new(target)
    end
    error("Cannot decide if it is an array or a dictionary")
end

return Result
