require "story"
require("functions.help_functions")
require("functions.update_queue")
require("functions.draw_grid")
require("functions.initialise")

script.on_configuration_changed(
    function(event)
        init()
        local players = game.players
        for _, player in pairs(players) do
            player_init(player)
        end
        local forces = game.forces
        for name, force in pairs(forces) do
            if global.researchQ[name] ~= nil and #global.researchQ[name] > 0 then
                check_queue(force)
            end
        end
    end
)

script.on_init(
    function()
        init()
    end
)

script.on_event(
    defines.events.on_gui_closed,
    function(event)
        local gui_type = event.gui_type
        local element = event.element
        if gui_type == defines.gui_type.custom and element and element.name == "Q" then
            local player = game.players[event.player_index]
            player.gui.center.Q.destroy()
            player.play_sound {path = "utility/gui_click"}
        end
    end
)

script.on_event(
    defines.events.on_gui_text_changed,
    function(event)
        if event.element.name == "rqon-text-filter" then
            local player = game.players[event.player_index]

            global.text_filter = event.element.text
            global.offset_tech[player.index] = 0
            draw_grid_player(player)
            player.gui.center.Q.add2q["rqon-text-filter"].focus()
        end
    end
)

function toggle_gui_window(player)
    if player.gui.center.Q then
        player.gui.center.Q.destroy()
        global.offset_tech[player.index] = 0
        global.showIcon[player.index] = true
    else
        local Q = player.gui.center.add {type = "flow", name = "Q", style = "rqon-flow"}
        update_queue_player(player)
        draw_grid_player(player)
    end
end

script.on_event(
    defines.events.on_gui_click,
    function(event)
        local player = game.players[event.player_index]
        local force = player.force

        if event.element.name == "rqon-text-filter" and event.button == defines.mouse_button_type.right then
            global.text_filter = ""
            global.offset_tech[player.index] = 0
            draw_grid_player(player)
        elseif event.element.name == "research_Q_TONT" then
            toggle_gui_window(player)
        elseif string.find(event.element.name, "rqon-add", 1, true) == 1 then
            local tech = string.gsub(event.element.name, "rqon%-add", "")

            if not force.technologies[tech].researched then
                add_research(force, tech)
                if not force.current_research then
                    force.add_research(global.researchQ[force.name][1])
                end
                update_queue_force(force)
                draw_grid_force(force)
            end
        elseif string.find(event.element.name, "rqoncancelbutton", 1, true) == 1 then
            local tech = string.gsub(event.element.name, "rqoncancelbutton", "")
            remove_research(force, tech)
            update_queue_force(force)
            draw_grid_force(force)
        elseif string.find(event.element.name, "rqonupbutton", 1, true) == 1 then
            local tech = string.gsub(event.element.name, "rqonupbutton", "")
            if event.button ~= defines.mouse_button_type.left or event.control then
                up(player, tech, 999999)
            elseif event.alt or event.shift then
                up(player, tech, 5)
            else
                up(player, tech, 1)
            end
            update_queue_force(force)
        elseif string.find(event.element.name, "rqondownbutton", 1, true) == 1 then
            local tech = string.gsub(event.element.name, "rqondownbutton", "")
            if event.button ~= defines.mouse_button_type.left or event.control then
                down(force, tech, 999999)
            elseif event.alt or event.shift then
                down(force, tech, 5)
            else
                down(force, tech, 1)
            end
            update_queue_force(force)
        elseif string.find(event.element.name, "rqon-science", 1, true) == 1 then
            global.offset_tech[player.index] = 0
            local tool = string.gsub(event.element.name, "rqon%-science", "")
            global.science_packs[tool][player.index] = not global.science_packs[tool][player.index]
            draw_grid_player(player)
        elseif event.element.name == "rqonscrollqueueup" then
            local move_size = 1
            if event.button ~= defines.mouse_button_type.left or event.control then
                move_size = #global.researchQ[force.name]
            elseif event.alt or event.shift then
                move_size = 5
            end
            global.offset_queue[player.index] = math.max(global.offset_queue[player.index] - move_size, 0)
            update_queue_player(player)
        elseif event.element.name == "rqonscrollqueuedown" then
            local move_size = 1
            local queue_size = #global.researchQ[force.name]
            if event.button ~= defines.mouse_button_type.left or event.control then
                move_size = queue_size
            elseif event.alt or event.shift then
                move_size = 5
            end
            local offset = global.offset_queue[player.index]
            local visible_rows = player.mod_settings["research-queue-the-old-new-thing-rows-count"].value
            global.offset_queue[player.index] = math.max(math.min(offset + move_size, queue_size - visible_rows), 0)
            update_queue_player(player)
        elseif event.element.name == "rqonextend-button" then
            global.offset_tech[player.index] = 0
            global.showExtended[player.index] = not global.showExtended[player.index]
            if not global.showExtended[player.index] then
                global.offset_tech[player.index] = 0
            end
            for name, science in pairs(global.science_packs) do
                if global.bobsmodules[name] then
                    science[player.index] = global.showBobsmodules[player.index]
                end
                if global.bobsaliens[name] then
                    science[player.index] = global.showBobsaliens[player.index]
                end
            end
            draw_grid_player(player)
        elseif event.element.name == "rqonscrolltechup" then
            global.offset_tech[player.index] = global.offset_tech[player.index] - 1
            draw_grid_player(player)
        elseif event.element.name == "rqonscrolltechdown" then
            global.offset_tech[player.index] = global.offset_tech[player.index] + 1
            draw_grid_player(player)
        elseif event.element.name == "rqon-overwrite-yes" then
            -- TODO: Add message to communicate the other case to the player?
            if not force.research_queue_enabled then
                force.add_research(global.researchQ[force.name][1])
            end
            if player.gui.center.warning then
                player.gui.center.warning.destroy()
            end
            if not player.gui.center.Q then
                player.gui.center.add {type = "flow", name = "Q", style = "rqon-flow"}
            end
            update_queue_force(force)
            draw_grid_force(force)
        elseif event.element.name == "rqon-overwrite-no" then
            if player.gui.center.warning then
                player.gui.center.warning.destroy()
            end
            if not player.gui.center.Q then
                player.gui.center.add {type = "flow", name = "Q", style = "rqon-flow"}
            end
            update_queue_player(player)
            draw_grid_player(player)
        elseif event.element.name == "rqontext" then
            global.showIcon[player.index] = not global.showIcon[player.index]
            draw_grid_player(player)
        elseif event.element.name == "rqonscience" then
            global.showResearched[player.index] = not global.showResearched[player.index]
            draw_grid_player(player)
        elseif event.element.name == "rqon-native-queue" then
            force.research_queue_enabled = not force.research_queue_enabled
            local notice =
                force.research_queue_enabled and {"rqon-notices.native-research-queue-enabled"} or
                {"rqon-notices.native-research-queue-disabled"}

            draw_grid_force(force)
            for _, player in pairs(force.connected_players) do
                player.print(notice)
            end
        elseif event.element.name == "rqon-bobsmodules" then
            global.showBobsmodules[player.index] = not global.showBobsmodules[player.index]
            -- This can be easily improved with a custom iterator
            for name, science in pairs(global.science_packs) do
                if global.bobsmodules[name] then
                    science[player.index] = global.showBobsmodules[player.index]
                end
            end
            draw_grid_player(player)
        elseif event.element.name == "rqon-bobsaliens" then
            global.showBobsaliens[player.index] = not global.showBobsaliens[player.index]
            -- This can be easily improved with a custom iterator
            for name, science in pairs(global.science_packs) do
                if global.bobsaliens[name] then
                    science[player.index] = global.showBobsaliens[player.index]
                end
            end
            draw_grid_player(player)
        end
    end
)

script.on_event(
    defines.events.on_research_finished,
    function(event)
        for _, player in pairs(event.research.force.connected_players) do
            player.print {"rqon-notices.research-finished", event.research.localised_name}
            -- The on_player_created event for this mod may not be called in time if another mod triggers this event.
            if global.offset_queue[player.index] == nil then
                player_init(player)
            end
        end

        remove_research(event.research.force, event.research.name)
        local refresh_gui, refresh_counter = {}, 0
        for index, player in pairs(event.research.force.connected_players) do
            local length_queue = #global.researchQ[event.research.force.name]

            -- All of these cases require a full redraw because a technology has been finished.
            if player.gui.center.Q then
                refresh_gui[player] = player
                refresh_counter = refresh_counter + 1
            elseif
                length_queue == 0 and
                    player.mod_settings["research-queue-the-old-new-thing_popup-on-queue-finish"].value
             then
                local Q = player.gui.center.add {type = "flow", name = "Q", style = "rqon-flow"}
                refresh_gui[player] = player
                refresh_counter = refresh_counter + 1
            end
            local offset_queue = global.offset_queue
            offset_queue[index] =
                math.max(
                0,
                math.min(
                    offset_queue[index],
                    length_queue - player.mod_settings["research-queue-the-old-new-thing-rows-count"].value
                )
            )
        end
        if refresh_counter > 0 then
            update_queue_force(event.research.force, false, refresh_gui)
            draw_grid_force(event.research.force)
        end
    end
)

script.on_event(
    defines.events.on_player_created,
    function(event)
        player_init(game.players[event.player_index])
    end
)

script.on_event(
    defines.events.on_force_created,
    function(event)
        if global.researchQ[event.force.name] == nil then
            global.researchQ[event.force.name] = {}
        end
        if global.labs[event.force.name] == nil then
            global.labs[event.force.name] = map_all_entities({type = "lab", force = event.force})
        end
    end
)

local lab_filter = {
    {filter = "type", type = "lab"}
}

script.on_event(
    defines.events.on_built_entity,
    function(event)
        -- The entity type check is still needed to handle events raised by other mods.
        -- Such events are not filtered by the game engine.
        if event.created_entity.type == "lab" then
            global.labs[event.created_entity.force.name][event.created_entity] = event.created_entity
        end
    end,
    lab_filter
)

function remove_lab(event)
    -- The entity type check is still needed to handle events raised by other mods.
    -- Such events are not filtered by the game engine.
    if event.entity.type == "lab" then
        global.labs[event.entity.force.name][event.entity] = nil
    end
end

script.on_event(defines.events.on_entity_died, remove_lab, lab_filter)
script.on_event(defines.events.on_pre_player_mined_item, remove_lab, lab_filter)
script.on_event(defines.events.on_robot_pre_mined, remove_lab, lab_filter)

-- This ensures that disconnected players don't have the GUI opened, and thus don't get updates
--  nor cause crashes when reconnecting with old, inconsistent GUI instances
-- Overwrite prompts are not removed but shouldn't be able to cause crashes nor are they ever updated
script.on_event(
    defines.events.on_player_left_game,
    function(event)
        local Q_gui = game.players[event.player_index].gui.center.Q
        if Q_gui then
            Q_gui.destroy()
        end
        global.offset_queue[event.player_index] = 0
        global.offset_tech[event.player_index] = 0
    end
)

script.on_event(
    "open-research-queue-the-old-new-thing",
    function(event)
        toggle_gui_window(game.players[event.player_index])
    end
)
