local Result = {}

local function Clone(target)
  local result = {}
  local length = #target
  for index = 1, length do
    result[index] = target[index]
  end
  return result
end

function Result:new (target)
  if getmetatable(target) == "private" then target = Clone(target) end
  setmetatable(target,self)
  self.__index = self
  return target
end

function Result:Select(transformation)
  local result = Result:new{}
  local length = #self
  for index = 1, length do
    result[index] = transformation(self[index])
  end
  return result
end

function Result:Sum()
local result = 0
local length = #self
for index = 1, length do
    result = result + self[index]
end
return result
end

function Result:Count()
  return #self
end
  
function Result:Where(predicate)
local result = Result:new{}
local length = #self
local resultIndex = 1
for index = 1, length do
  if predicate(self[index]) then
    result[resultIndex] = self[index]
    resultIndex = resultIndex+1
  end
end
return result
end

function Result:Any(predicate)
local length = #self
if not predicate then return length > 0 end
for index = 1, length do
  if predicate(self[index]) then return true end
end
return nil
end

return Result