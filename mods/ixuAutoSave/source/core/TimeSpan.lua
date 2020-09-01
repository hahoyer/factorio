local Result = {}

local function modulo(n, d)
    local q, r = math.modf(n / d)
    r = math.floor( r * d + 0.5)
    return q, r
  end
  
  function Result:new(target)
  if not target then
    target = {}
  end
  setmetatable(target, self)
  self.__index = self
  return target
end

function Result.FromString(target)
  local result = Result:new()

  local days, hours, minutes, seconds = target:match("^(%d+)%.(.*)$")

  if days then
    target = hours
    result.Days = days * 1
  else
    result.Days = 0
  end

  hours, minutes, seconds = target:match("^(%d+):(%d%d)$")

  if hours then
    seconds = 0
  else
    hours, minutes, seconds = target:match("^(%d+):(%d%d):(%d%d)$")
  end

  if hours == nil then
    hours, minutes, seconds = target:match("^(%d+):(%d%d):(%d%d%..*)$")
  end

  if hours then
    result.Hours = hours * 1
    result.Minutes = minutes * 1
    result.Seconds = seconds * 1
    return result
  end
end

function Result.FromTicks(target)
  local result = Result:new()

  local ticks
  target, ticks = modulo(target, 60)
  target, result.Seconds = modulo(target, 60)
  if ticks > 0 then result.Seconds = result.Seconds + ticks / 60 end
  target, result.Minutes = modulo(target, 60)
  result.Days, result.Hours = modulo(target, 24)
  return result
end

function Result:getTicks()
  return math.floor((((self.Days * 24 + self.Hours) * 60 + self.Minutes) * 60 + math.floor(self.Seconds + 0.5)) * 60)
end

function Result:getTimeAsHHMMSS()
  return string.sub(tostring(1000000 + self.Hours * 10000 + self.Minutes * 100 + math.floor(self.Seconds)), 2)
end

function Result:toString()
  return self.Days .. "." .. self.Hours ..":".. self.Minutes ..":".. self.Seconds
end

return Result
