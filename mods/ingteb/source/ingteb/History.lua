local Constants = require("Constants")
local class = require("core.class")

local History = class:new("History")

function History:new(target)
    local instance = self:adopt{
        Data = target and target.Data or {},
        Index = target and target.index or 0,
    }
    self = instance
    instance:properties{
        IsForePossible = {
            get = function(self)
                return self.Index < #self.Data --
            end,
        },
        IsBackPossible = {
            get = function(self)
                return self.Index > 1 --
            end,
        },
        Current = {get = function(self)
            return self.Index > 0 and self.Data[self.Index] or nil
        end},
    }
    return instance
end

function History:Log(tag)
    log(">>>---" .. tag)
    for index = 1, #self.Data do
        log((index == self.Index and "*" or " ") .. index .. " " .. self.Data[index].SpriteName)
    end
    log("<<<---" .. tag)
end

function History:ResetTo(target)
    self.Data = {target}
    self.Index = 1
    self:Log("History:ResetTo")
end

function History:AdvanceWith(target)
    self.Index = self.Index + 1
    while #self.Data > self.Index do table.remove(self.Data, self.Index) end
    self.Data[self.Index] = target
    self:Log("History:AdvanceWith")
end

function History:Back() --
    assert(self.Index > 1)
    self.Index = self.Index - 1
    self:Log("History:Back")
end

function History:Fore() --
    assert(self.Index < #self.Data)
    self.Index = self.Index + 1
    self:Log("History:Fore")
end

function History:Save() return {Data = self.Data, Index = self.Index} end

return History
