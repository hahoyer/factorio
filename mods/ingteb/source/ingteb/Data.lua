local Constants = require("Constants")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary

local function SpreadHandMiningRecipe(prototype)
    return {
        In = Array:new {
            {type = prototype.type, name = prototype.name}
        },
        Properties = Array:new {
            {type = "utility", name = "clock", amount = prototype.mineable_properties.mining_time}
        },
        Out = Array:new(prototype.mineable_properties.products)
    }
end

local function GetAmountForRecipe(recipe)
    local result = global.Current.Player.get_craftable_count(recipe.name)
    if result > 0 then
        return result
    end
end

local function SpreadRecipe(recipe)
    local technology =
        Dictionary:new(game.technology_prototypes):Where(
        function(technology)
            return Array:new(technology.effects):Any(
                function(effect)
                    return effect.type == "unlock-recipe" and effect.recipe == recipe.name
                end
            )
        end
    ):Top(true, true)

    return {
        In = Array:new(recipe.ingredients),
        Properties = Array:new {
            {type = "technology", name = technology and technology.name, cache = {Prototype = {Value = technology}}},
            {
                type = "recipe",
                name = recipe.name,
                amount = GetAmountForRecipe(recipe),
                cache = {Prototype = {Value = recipe}}
            },
            {type = "utility", name = "clock", amount = recipe.energy}
        },
        Out = Array:new(recipe.products)
    }
end

local function SpreadHandMining(target)
    local prototype = game.entity_prototypes[target.name]

    if prototype.mineable_properties.minable then
        local modifier = global.Current.Player.character_mining_speed_modifier
        if modifier == 0 then
            modifier = nil
        end
        return {
            Actors = Array:new {{type = "entity", name = "character", amount = modifier}},
            Recipes = Array:new {SpreadHandMiningRecipe(prototype)}
        }
    end
end

local function SpreadResource(target)
    local groups = Table.Array:new {}
    local handMining = SpreadHandMining(target)
    if handMining then
        groups:Append(handMining)
    end

    return {Target = target, In = groups, Out = Array:new {}}
end

local function SpreadActors(key)
    return Dictionary:new(game.entity_prototypes):Where(
        function(entity)
            return entity.crafting_categories and entity.crafting_categories[key]
        end
    ):ToArray():Select(
        function(entity)
            local target = {name = entity.name, type = "entity", amount = entity.crafting_speed}
            return target
        end
    )
end

local function SpreadItemGroup(target, key)
    local actors = SpreadActors(key)
    return {
        Actors = actors,
        Recipes = target:Select(
            function(recipe)
                return SpreadRecipe(recipe)
            end
        )
    }
end

local function SpreadItemIn(target)
    return Dictionary:new(game.recipe_prototypes):Where(
        function(recipe)
            if recipe.hidden then
                return false
            end
            return Array:new(recipe.ingredients):Any(
                function(this)
                    return this.name == target.name and this.type == target.type
                end
            )
        end
    ):ToArray():ToGroup(
        function(recipe)
            return {Key = recipe.category, Value = recipe}
        end
    ):Select(
        function(group, key)
            return SpreadItemGroup(group, key)
        end
    )
end

local function SpreadItemOut(target)
    return Dictionary:new(game.recipe_prototypes):Where(
        function(item)
            return Array:new(item.products):Any(
                function(this)
                    return this.name == target.name and this.type == target.type
                end
            )
        end
    ):ToArray():ToGroup(
        function(recipe)
            return {Key = recipe.category, Value = recipe}
        end
    ):Select(
        function(group, key)
            return SpreadItemGroup(group, key)
        end
    )
end

local function SpreadItem(target)
    return {Target = target, In = SpreadItemIn(target), Out = SpreadItemOut(target)}
end

local function SpreadEntity(target)
    local entity = game.entity_prototypes[target.name]
    if not entity then
        return
    end
    local candidates = entity.items_to_place_this
    if not candidates or #candidates == 0 then
        return
    end
    local item = candidates[1]
    return SpreadItem({type = "item", name = item.name})
end

local function ProvideHelp(target)
    local result
    if target.type == "technology" or target.type == "recipe" then
        return
    end
    if target.type == "resource" or target.type == "tree" or target.type == "simple-entity" then
        result = SpreadResource(target)
    elseif target.type == "item" or target.type == "fluid" then
        result = SpreadItem(target)
    else
        result = SpreadEntity(target)
    end
    if not result then
        target.type = "item"
        result = SpreadItem(target)
    end
    return result
end

local function ProvideResearch(technologyName)
end

local function ProvideCrafting(recipeName)
end

local result = {}

function result.Get(target)
    if not target.target then
        return ProvideHelp(target)
    end
    local subTarget = target.target
    if target.name and not target.amount then
        return ProvideResearch(target.name)
    end
    if target.amount and target.amout > 0 then
        return ProvideCrafting(subTarget.name)
    end
end

function result.FindTarget()
    local result = {}

    local cursor = global.Current.Player.cursor_stack
    if cursor and cursor.valid and cursor.valid_for_read then
        return {type = cursor.type, name = cursor.name}
    end
    local cursor = global.Current.Player.cursor_ghost
    if cursor then
        return {type = cursor.type, name = cursor.name}
    end
    local cursor = global.Current.Player.selected
    if cursor then
        return {type = cursor.type, name = cursor.name}
    end

    local cursor = global.Current.Player.opened
    if cursor then
        if global.Current.Links and global.Current.Links[cursor.index] then
            local target = global.Current.Links[cursor.index]
            return target
        end
        table.insert(result, {type = cursor.type, name = cursor.name})
        if cursor.burner then
            return {fuel_categories = cursor.burner.fuel_categories}
        end
        if cursor.type == "mining-drill" and cursor.mining_target then
            return {type = cursor.mining_target.type, name = cursor.mining_target.name}
        end

        if cursor.type == "furnace" and cursor.previous_recipe then
            return {type = "recipe", name = cursor.previous_recipe.name}
        end
        if cursor.type == "assembling-machine" and cursor.get_recipe() then
            return {type = "recipe", name = cursor.get_recipe().name}
        end
    end
end

return result
