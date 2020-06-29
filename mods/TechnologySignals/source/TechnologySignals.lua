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

local function GetLocalizedNames(value)
    if value.type == "technology" then
        local result = {}

        local name, index = value.name:match("^(.*)%-(%d*)$")
        if value.localised_name then
            result.Name = value.localised_name
        elseif name then
            result.Name = {"technology-name." .. name, value.name}
        else
            result.Name = {"technology-name." .. value.name}
        end

        if value.localised_description then
            result.Description = value.localised_description
        end
        if value.description then
            log("value.description=" .. value.description)
            local x = value / 2
        end

        return result
    end
end

local function EnsureLayerdIcons(value)
    if not value.icons then
        value.icons = {}
    end
    if value.icon then
        table.insert(value.icons, {icon = value.icon, icon_size = value.icon_size, icon_mipmaps = value.icon_mipmaps})
        value.icon = nil
        value.icon_size = nil
        value.icon_midmaps = nil
    end
end

local function GetLayerdIcons(value)
    local result = value.icons or {}
    if value.icon then
        table.insert(result, {icon = value.icon, icon_size = value.icon_size, icon_mipmaps = value.icon_mipmaps})
    end
    return result
end

local function ScaleSingleIcon(target, value)
    local scale = target.scale or 1
    scale = (target.scale or 1) * value
    if scale == 1 then
        scale = nil
    end

    local shift = target.shift or {0, 0}
    shift = {shift[1] * value, shift[2] * value}
    if shift[1] == 0 and shift[2] == 0 then
        shift = nil
    end

    return {
        icon = target.icon,
        icon_size = target.icon_size,
        icon_mipmaps = target.icon_mipmaps,
        tint = target.tint,
        scale = scale,
        shift = shift
    }
end

local function ShiftSingleIcon(target, value)
    local shift = target.shift or {0, 0}
    shift = {
        shift[1] + value[1] * target.icon_size,
        shift[2] + value[2] * target.icon_size
    }
    if shift[1] == 0 and shift[2] == 0 then
        shift = nil
    end
    return {
        icon = target.icon,
        icon_size = target.icon_size,
        icon_mipmaps = target.icon_mipmaps,
        tint = target.tint,
        scale = target.scale,
        shift = shift
    }
end

local function TransformIcon(target, scale, shift)
    local result = {}
    for _, value in ipairs(target) do
        value = ScaleSingleIcon(value, scale)
        value = ShiftSingleIcon(value, shift)
        table.insert(result, value)
    end
    return result
end

local function CombileIcons(targets)
    local result = {}
    for _, target in ipairs(targets) do
        for _, icon in ipairs(target) do
            table.insert(result, icon)
        end
    end
    return result
end

local function RegisterTechnologySignals()
    local ingredients = GetIngredients()

    local labIcon = TransformIcon(GetLayerdIcons(data.raw["lab"]["lab"]), 0.2, {-0.2, -0.2})

    for _, value in pairs(data.raw["technology"]) do
        local locale = GetLocalizedNames(value)

        local icon = GetLayerdIcons(value)

        local signal = {
            type = "virtual-signal",
            name = "technology-" .. value.name,
            localised_name = {"TechnologySignals.technology", locale.Name},
            localised_description = locale.Description,
            group = "TechnologySignals",
            subgroup = GetSubgroup(value, ingredients),
            icons = CombileIcons {icon, labIcon},
            order = value.order
        }

        data:extend {signal}
    end
end

local function RegisterTechnologySignalGroup()
    local icons = GetLayerdIcons(data.raw["lab"]["lab"])
    local signals = data.raw["item-group"]["signals"]
    local group = {
        type = "item-group",
        name = "TechnologySignals",
        icons = icons,
        order = signals.order .. "1"
    }
    data:extend {group}
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
