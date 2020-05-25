local Dictionary = {}
local Array = {}
Array.insert = table.insert
Array.remove = table.remove

function Dictionary:Clone(predicate)
  local result = Dictionary:new {}
  for key, value in pairs(self) do
    if not predicate or predicate(value, key) then
      result[key] = value
    end
  end
  return result
end

function Array:Clone(predicate)
  local result = Array:new {}
  for index, value in ipairs(self) do
    if predicate(value, index) then
      result:insert(value)
    end
  end
  return result
end

function Dictionary:new(target)
  if getmetatable(target) == "private" then
    target = Dictionary.Clone(target)
  end
  setmetatable(target, self)
  self.__index = self
  return target
end

function Array:new(target)
  if getmetatable(target) == "private" then
    target = Array.Clone(target)
  end
  setmetatable(target, self)
  self.__index = self
  return target
end

function Dictionary:Select(transformation)
  local result = Dictionary:new {}
  for key, value in pairs(self) do
    result[key] = transformation(value, key)
  end
  return result
end

function Array:Select(transformation)
  local result = Array:new {}
  for key, value in ipairs(self) do
    result:insert(transformation(value, key))
  end
  return result
end

function Dictionary:Sum(predicate)
  local result = 0
  for key, value in pairs(self) do
    if not predicate or predicate(value, key) then
      result = result + value
    end
  end
  return result
end

function Array:Sum(predicate)
  local result = 0
  for key, value in ipairs(self) do
    if not predicate or predicate(value, key) then
      result = result + value
    end
  end
  return result
end

function Dictionary:Count(predicate)
  local result = 0
  for key, value in pairs(self) do
    if not predicate or predicate(value, key) then
      result = result + 1
    end
  end
  return result
end

function Array:Count(predicate)
  local result = 0
  for key, value in ipairs(self) do
    if not predicate or predicate(value, key) then
      result = result + 1
    end
  end
  return result
end

function Dictionary:Any(predicate)
  for key, value in pairs(self) do
    if not predicate or predicate(value, key) then
      return true
    end
  end
  return nil
end

function Array:Any(predicate)
  for key, value in ipairs(self) do
    if not predicate or predicate(value, key) then
      return true
    end
  end
  return nil
end

function Dictionary:ToDictionary(getPair)
  local result = Dictionary:new {}
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
  local result = Dictionary:new {}
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

function Dictionary:ToTable(getItem)
  local result = Array:new {}
  for key, value in pairs(self) do
    result:insert(getItem and getItem(value, key) or value)
  end
  return result
end

function Array:ToTable(getItem)
  local result = Array:new {}
  for key, value in ipairs(self) do
    result:insert(getItem and getItem(value, key) or value)
  end
  return result
end

function Array:Top(allowEmpty, allowMultiple, onEmpty, onMultiple)
  if #self == 0 then
    if allowEmpty == false or onEmpty then
      error(onEmpty and onEmpty() or "Array contains no element.")
    end
    return
  elseif #self > 1 then
    if allowMultiple == false or onMultiple then
      error(onMultiple and onMultiple(#self) or "Array contains more than one element (" .. #self .. ").")
    end
  end
  return self[1]
end

function Array:Take(count)
  if #self <= count then
    return self
  end

  local result = Array:new {}
  for index = 1, count do
    result:insert(self[index])
  end
  return result
end

function Array:Skip(count)
  local result = Array:new {}
  for index = 1 + count, #self do
    result:insert(self[index])
  end
  return result
end

local Result = {
  Array = Array,
  Dictionary = Dictionary
}

function Result.new(self, target)
  if #target == 0 and next(target) then
    return self.Dictionary:new(target)
  elseif not next(target,#target > 0 and #target or nil) then
    return self.Array:new(target)
  end
  error("Cannot decide if it is an array or a dictionary")
end

return Result
