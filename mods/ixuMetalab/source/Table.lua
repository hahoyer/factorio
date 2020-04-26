local Result = {}

function Result:Clone(predicate)
  local result = Result:new{}
  if predicate then
    for key, value in pairs(self) do
      if predicate(value, key) then result[key] = value end
    end
  end
  return result
end

function Result:new (target)
  if getmetatable(target) == "private" then target = Result.Clone(target) end
  setmetatable(target,self)
  self.__index = self
  return target
end

function Result:Select(transformation)
  local result = Result:new{}
  for key, value in pairs(self) do
    result[key] = transformation(value, key)
  end
  return result
end

function Result:Sum()
local result = 0
local length = #self
for index = 1, length do
    result = result + (self[index] or 0)
end
return result
end

function Result:Count(predicate)
  local result = 0
  for key, value in pairs(self) do
    if not predicate or predicate(value, key) then result = result+1 end
  end
  return result
end
  
function Result:Index(item)
  local length = #self
  for index = 1, length do
    if self[index] == item then return index end
  end
  return nil
end
    
function Result:Where(predicate)
  local result = Result:new{}
  local length = #self
  for index = 1, length do
    local value = self[index]
    if predicate(value) then table.insert(result, value) end
  end
  return result
end

function Result:Remove(item)
  table.remove( self, Result.Index(self, item))
end
      
function Result:Any(predicate)
  for key, value in pairs(self) do
    if key ~= "__self" then
      if not predicate then return true end
      if predicate(value, key) then return true end
    end
  end
  return nil
end

function Result:ToPairs(getKey, getValue)
  local result = Result:new{}
  for key, value in pairs(self) do
    local newKey = getKey(value,key)
    local newValue = getValue(value,key)
    result[newKey] = newValue
  end
  return result
end



return Result
