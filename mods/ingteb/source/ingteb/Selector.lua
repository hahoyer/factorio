local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Database = require("ingteb.Database")
local UI = require("core.UI")

local Selector = {}

function Selector:new(frame, targets)
    frame.caption = {"ingteb_utility.selector"}

    assert(not targets)

    frame.add {type = "choose-elem-button", elem_type = "signal"}

end

return Selector