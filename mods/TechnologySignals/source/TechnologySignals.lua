local locale = require "__rusty-locale__.locale"
local result = {}

local function GetIngredients()
    local ingredients = {}
    for _, value in pairs(data.raw["technology"]) do
        for _, ingredient in pairs(value.unit.ingredients) do
            local name = ingredient[1]
            ingredients[name] = (ingredients[name] or 0) + 1
        end
    end

    local ingredientsTable = {}
    for key, value in pairs(ingredients) do
        table.insert(ingredientsTable, {name = key, weight = value})
    end

    table.sort(
        ingredientsTable,
        function(a, b)
            return a.weight < b.weight
        end
    )

    local result = {}
    for _, value in ipairs(ingredientsTable) do
        table.insert(result, value.name)
    end
    return result
end

local function GetSubgroup(technology, ingredients)
    local currentIngredients = technology.unit.ingredients
    local result = ""
    for _, value in ipairs(ingredients) do
        for _, name in pairs(currentIngredients) do
            result = result .. (name == value and "1" or "0")
        end
    end
    return result
end

local function GetSubgroups(ingredients)
    local result = {}

    for _, value in pairs(data.raw["technology"]) do
        local subgroup = GetSubgroup(value, ingredients)
        result[subgroup] = true
    end

    return result
end

local function RegisterSubgroupForIngredientCombination()
    local ingredients = GetIngredients()

    for key, _ in pairs(GetSubgroups(ingredients)) do
        data:extend {
            {
                type = "item-subgroup",
                name = key,
                group = "TechnologySignals",
                order = key
            }
        }
    end
end

local function GetLocalizedNames (value)
    if value.type == "technology"then

        local result =  {}

        local name, index = value.name:match("^(.*)%-(%d*)$")
        if value.localised_name then
            result.Name = value.localised_name 
        elseif name then
            result.Name ={"technology-name."..name,value.name}
        else
            result.Name ={"technology-name."..value.name}
        end

        if value.localised_description then
            result.Description = value.localised_description
        end
        if value.description then
            log("value.description="..value.description)
            local x = value / 2
        end
        
        
        return result
    end
end


local function RegisterTechnologySignals()
    local ingredients = GetIngredients()

    for _, value in pairs(data.raw["technology"]) do
        local locale = GetLocalizedNames(value)

        local signal = {
            type = "virtual-signal",
            name = "technology-" .. value.name,
            localised_name = {"TechnologySignals.technology",locale.Name},
            localised_description = locale.Description,
            group = "TechnologySignals",
            subgroup = GetSubgroup(value, ingredients),
            icon = value.icon,
            icon_size = value.icon_size,
            icon_mipmaps = value.icon_mipmaps,
            order = value.order
        }

        if value.description then 
           log(serpent.block(value))
           local x = value/3
        end
        data:extend {signal}
    end
end

local function RegisterTechnologySignalGroup()
    local lab = data.raw["lab"]["lab"]
    local signals = data.raw["item-group"]["signals"]

    data:extend {
        {
            type = "item-group",
            name = "TechnologySignals",
            icon = lab.icon,
            icon_size = lab.icon_size,
            icon_mipmaps = lab.icon_mipmaps,
            order = signals.order .. "1"
        }
    }
end

function result.RegisterData()
    RegisterTechnologySignalGroup()
end

function result.RegisterDataUpdates()
    RegisterSubgroupForIngredientCombination()
    RegisterTechnologySignals()
end

function result.RegisterDataFinalFixes()
end

function result.RegisterControlHandling()
end

return result
