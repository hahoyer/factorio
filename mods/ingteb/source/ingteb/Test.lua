local event = require("__flib__.event")
local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Helper = require("ingteb.Helper")
local Gui = require("ingteb.Gui")
local History = require("ingteb.History"):new()
local Database = require("ingteb.Database"):new()
local ValueCache = require("core.ValueCache")
local PropertyProvider = require("core.PropertyProvider")

function TestProperyProvider()
    local x = PropertyProvider:new{data = false}

    x.property.TestValue = {
        get = function(self) return self.data end,
        set = function(self, value) self.data = value end,
    }

    assert(not x.TestValue)

    x.TestValue = true

    assert(x.TestValue)
    log(x.TestValue)

    x.data = "hallo"

    assert(x.TestValue == "hallo")
end

function TestValueCache()
    local count = 0

    local p = ValueCache(
        function()
            count = count + 1
            return count
        end
    )

    log(p.Value)
    assert(p.Value == 1)
    log(p.Value)
    assert(p.Value == 1)

    p.IsValid = false

    assert(p.Value == 2)
end

TestProperyProvider()
TestValueCache()

local xxx = c / v
