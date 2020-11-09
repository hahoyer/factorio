local Result = {}

function Result:__add(other)
    other = Result:new(other)
    local result = {
        x = self.x + other.x,
        y = self.y + other.y
    }
    setmetatable(result, self)
    return result
end

function Result:__sub(other)
    other = Result:new(other)
    local result = {
        x = self.x - other.x,
        y = self.y - other.y
    }
    setmetatable(result, self)
    return result
end

function Result:__mul(other)
    local result = {
        x = self.x * other,
        y = self.y * other
    }
    setmetatable(result, self)
    return result
end

function Result:__div(other)
    local result = {
        x = self.x / other,
        y = self.y / other
    }
    setmetatable(result, self)
    return result
end

function Result:__unm()
    local result = {
        x = -self.x,
        y = -self.y
    }
    setmetatable(result, self)
    return result
end

function Result:new(target)
    if not target then
        target = {x = 0, y = 0}
    end
    self.x = target.x or 0
    self.y = target.y or 0
    setmetatable(target, self)
    self.__index = self

    return target
end

return Result