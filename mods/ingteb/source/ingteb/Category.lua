local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local ValueCache = require("core.ValueCache")
require("ingteb.Common")

function Category(domainName, prototype, database)
    local self = CommonThing(prototype.name, prototype, database)
    self.class_name = "Category"
    self.DomainName = domainName
    self.Workers = Array:new()
    self.Recipes = Array:new()

    function self:Setup() end

    return self
end

