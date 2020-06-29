local mod_gui = require("mod-gui")

function init()
    if global.researchQ == nil then global.researchQ = {} end
    if global.labs == nil then global.labs = {} end
    local forces = game.forces
    for name, force in pairs(forces) do
        if global.researchQ[name] == nil then
            global.researchQ[name] = {}
            if force.current_research then
                table.insert(global.researchQ[name], force.current_research.name)
            end
        end
        global.labs[name] = map_all_entities({type = "lab", force = force})
    end
    if global.showIcon == nil then global.showIcon = {} end
    if global.showResearched == nil then global.showResearched = {} end
    if global.offset_queue == nil then global.offset_queue = {} end
    if global.offset_tech == nil then global.offset_tech = {} end
    if global.showExtended == nil then global.showExtended = {} end
    -- Always clean known science packs to avoid iterating obsolete prototypes.
    global.science_packs = {}
    local item_prototypes = game.get_filtered_item_prototypes({{ filter = "type", type = "tool" }})
    for name, item in pairs(item_prototypes) do
        global.science_packs[name] = {}
    end
    global.bobsmodules = {
        ["module-case"] = true,
        ["module-circuit-board"] = true,
        ["speed-processor"] = true,
        ["effectivity-processor"] = true,
        ["productivity-processor"] = true,
        ["pollution-clean-processor"] = true,
        ["pollution-create-processor"] = true
    }
    if global.showBobsmodules == nil then global.showBobsmodules = {} end
    global.bobsaliens = {
        ["alien-science-pack-blue"] = true,
        ["alien-science-pack-orange"] = true,
        ["alien-science-pack-purple"] = true,
        ["alien-science-pack-yellow"] = true,
        ["alien-science-pack-green"] = true,
        ["alien-science-pack-red"] = true
    }
    if global.showBobsaliens == nil then global.showBobsaliens = {} end
end

function player_init(player)
    local top = mod_gui.get_button_flow(player)
    if not top.research_Q_TONT then top.add{type = "button", name = "research_Q_TONT", caption = "RQ", style = "rqon-top-button"} end
    global.showIcon[player.index] = true
    global.showResearched[player.index] = false
    global.offset_queue[player.index] = 0
    global.offset_tech[player.index] = 0
    global.showExtended[player.index] = false
    for name, science in pairs(global.science_packs) do
        if player.force.recipes[name] ~= nil then
            science[player.index] = player.force.recipes[name].enabled
        else
            science[player.index] = false
        end
    end
    global.showBobsmodules[player.index] = player.force.technologies["modules"].researched
    if player.force.technologies["alien-research"] then
        global.showBobsaliens[player.index] = player.force.technologies["alien-research"].researched
    else
        global.showBobsaliens[player.index] = false
    end
end


function map_all_entities(input)
    -- input = {name = string, type = string, force = string or force, surface = string or {table of surface(s)}}
    local map = {}

    if type(input.surface) == "string" then input.surface = {game.surfaces[input.surface]} end
    local surfaces = input.surface or game.surfaces

    for _, surface in pairs(surfaces) do
        for chunk in surface.get_chunks() do
            local entities = surface.find_entities_filtered{
                area = {left_top = {chunk.x*32, chunk.y*32}, right_bottom = {(chunk.x+1)*32, (chunk.y+1)*32}},
                name = input.name,
                type = input.type,
                force = input.force}
            for _, entity in ipairs(entities) do
                map[entity] = entity
            end
        end
    end
    return map
end