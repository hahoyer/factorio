local Constants = require("Constants")

local result = {
    Data = {},
    Index = 0
}

function result:new()
    local target = {}
    setmetatable(target, self)
    self.__index = self
    return target
end

function result:RemoveAll()
    self.Data = {}
    self.Index = 0
end

function result:HairCut(target)
    if target then
        self.Index = self.Index + 1
        while #self.Data >= self.Index do
            table.remove(self.Data, self.Index)
        end
        self.Data[self.Index] = target
        return target
    end
end

function result:Back()
    if self.Index > 1 then
        self.Index = self.Index - 1
        return self.Data[self.Index]
    end
end

function result:GetCurrent()
    return self.Data[self.Index]
end

function result:Fore()
    if self.Index < #self.Data then
        self.Index = self.Index + 1
        return self.Data[self.Index]
    end
end

function result:Save()
    return {Data = self.Data, Index = self.Index}
end

function result:Load(target)
    if target then
        self.Data = target.Data
        self.Index = target.Index
    end
end

return result
