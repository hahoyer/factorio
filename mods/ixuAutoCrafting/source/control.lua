local mod_name = 'ixuAutoCrafting'

local SHORTCUT_NAME = 'ixuAutoCrafting-toggle'

local debugging = false

local max_arr = {0, .2, .5, .8, 1, 2, 3, 4}
local work_tick = 5


local map                = require('lib.functional').map
local filter             = require('lib.functional').filter
local fnn                = require('lib.functional').fnn
local range              = require('lib.functional').range
local list_iter_filt_map = require('lib.functional').list_iter_filt_map
local iterlist_iter      = require('lib.functional').iterlist_iter

-- If we complete an autostarted craft then mark it as done so that the next
-- crafting queue size check knows it wasn't because the player canceled it.
script.on_event(defines.events.on_player_crafted_item, function(event)
    -- How can an item stack that I recieve in an event handler and used immediatly be invalid? Apparently it can...
    -- This fix will pause ixuAutoCrafting instead of crashing, at least slightly better.
    if not event.item_stack.valid_for_read then return nil end

    local p = game.players[event.player_index]
    local d = global.players_data[event.player_index]
    if d.current_job == event.item_stack.name then
        d.current_job = nil
        d.hh_request_tick = game.tick
        if p.crafting_queue and p.crafting_queue[1].count == 1 and p.mod_settings["autocraft-sound-enabled"].value then
            p.play_sound{path = 'ixuAutoCrafting-core-crafting_finished'--[[, volume_modifier = 1--]]}
        end
    elseif --[[p.crafting_queue_size == 1 and--]] p.crafting_queue and p.crafting_queue[1].count == 1 and d.current_job == nil then
    -- elseif event.item_stack.count == 1 then
        p.play_sound{path = 'ixuAutoCrafting-core-crafting_finished'--[[, volume_modifier = 1--]]}
    end
    global.players_data[event.player_index] = d
end)

-- Init all new joining players.
script.on_event(defines.events.on_player_joined_game, function(event)
    local p = game.players[event.player_index]
    if debugging then p.print('joined!') end
    local d = global.players_data[event.player_index]
    if d == nil then
        d = init_player(event.player_index)
    end
    global.players_data[event.player_index] = d
end)

-- The mod core logic
script.on_event(defines.events.on_tick, function(event)
    for player_index = game.tick % work_tick + 1, #game.players, work_tick do
        local p = game.players[player_index]
        if p.connected and p.controller_type == defines.controllers.character then
            local d = global.players_data[player_index]
            local canceled_autocraft = false
            if d.current_job ~= nil and p.crafting_queue_size > 0 then
                if d.current_job ~= p.crafting_queue[#p.crafting_queue].recipe or p.crafting_queue[#p.crafting_queue].count > 1 then
                    d.current_job = nil
                elseif p.crafting_queue[d.current_job] ~= nil and p.crafting_queue[d.current_job].recipe == nil then
                    canceled_autocraft = true
                    enabled(player_index, false)
                    -- d.paused = true
                end
            end
            if p.crafting_queue_size == 0 then
                if d.current_job ~= nil then
                    canceled_autocraft = true
                    enabled(player_index, false)
                    -- d.paused = true
                end
            -- end
            -- if p.crafting_queue_size == 0 then
                if not d.paused and (d.hh_request_tick + work_tick >= game.tick or game.tick % 200 == game.tick % 5) then
                    d.hh_request_tick = d.hh_request_tick - work_tick
                    hh_player(player_index)
                end
            end
            if canceled_autocraft then
                p.print(mod_name..' is now paused until you hit increase or decrease key (Options > Controls > Mods).')
                d.current_job = nil
            end
        end
    end
end)

local stack_size_cache = {}
function stack_size(item)
    stack_size_cache[item] = stack_size_cache[item] or game.item_prototypes[item].stack_size
    return stack_size_cache[item]
end

function get_request_count(p, d, item)
    local setting = d.settings[item]
    if setting == nil then
        setting = d.settings['Default']
        if setting == nil then
            p.print(mod_name..' Error: Uninitialised Default setting.')
        end
    end
    local stsz = stack_size(item)
    -- For better interaction with stack size changing mods: Pretend that the max stack size is only 1/10 as big.
    if stsz >= 500 then
        stsz = stsz / 10
    end
    local mi = math.ceil(stsz*setting)
    -- Don't keep autocrafting this item if it will just end up in logistics trash slots.
    if p.character and p.auto_trash_filters[item] then
        mi = math.min(mi, p.auto_trash_filters[item])
    end
    return mi
end

function build_request_iterator(p, d)
    -- List is an iterator over all items that are candidates for autocrafting.
    -- qb: The quickbar items. We filter away items which are not "filtered" (blue item lock on quickbar) and pick the "filter_" instead of the item stack.
    local list = {}
    local aqb = fnn(map(range(1,9), function(q) return p.get_active_quick_bar_page(q) end))
    local slots = iterlist_iter(map(aqb, function(page)
        return map(range(1,10), function(index)
            return (page-1)*10+index
        end)
    end))
    slots = filter(map(slots, function(q)
        local slot = p.get_quick_bar_slot(q)
        if slot ~= nil then return slot.name end
        return ''
    end), function(q) return q ~= '' end)
    table.insert(list, slots)

    return list
end

function craft(p, d, item, count)
    if p.cheat_mode == false then
        p.begin_crafting{count=1, recipe=item, silent=true}
        d.current_job = item
    else
        p.begin_crafting{count=count, recipe=item, silent=true}
    end
end

function hh_player(player_index)
    local p = game.players[player_index]
    if p.connected and p.controller_type ~= defines.controllers.character then return nil end
    if p.crafting_queue_size ~= 0 then return nil end
    local d = global.players_data[player_index]
    if d.paused then return nil end
    -- local mi = p.get_main_inventory()

    local selected_item = nil
    local count_selected = nil
    local max_selected = nil
    local cs = p.cursor_stack
    local item

    list = iterlist_iter(build_request_iterator(p, d))
    for item in list do
        if debugging then p.print('debug ' .. item) end
        -- Check that we can craft this item. If not, skip.
        local ci = p.get_item_count(item)
        local mi = get_request_count(p, d, item)
        if ci < mi and p.force.recipes[item] ~= nil and (p.cheat_mode or (p.force.recipes[item].enabled == true and p.get_craftable_count(item) > 0)) then
            local prio_selected = 1
            if selected_item ~= nil then
                prio_selected = count_selected/max_selected
            end
            -- prio is a bit backwards. Lower prio number values for prioritized items.
            local prio_current = ci/mi
            local item_held = cs.valid_for_read and cs.name == item
            -- Prioritise items held in cursor above all. Prioritise less fulfilled autocraft requests.
            local prioritised = prio_current < prio_selected or item_held
            -- Check that we can craft the item
            if prio_current < 1 and prioritised then
                selected_item = item
                count_selected = ci
                max_selected = mi
                if cs.valid_for_read and cs.name == item then
                    break
                end
            end
        end
    end
    if selected_item ~= nil then craft(p, d, selected_item, max_selected-count_selected) end
end

-- Handle hotkey presses.
script.on_event('ixuAutoCrafting-increase', function(event) change(event, true) end)
script.on_event('ixuAutoCrafting-decrease', function(event) change(event, false) end)


-- local event_handlers = {

--     on_lua_shortcut = function(event)
--         -- if event.prototype_name ~= SHORTCUT_NAME then return end
--         local p = game.players[event.player_index]
--         printOrFly(p, '1 '..event.prototype_name)
--         remotes.paused(event.player_index, not remotes.paused(event.player_index))
--         p.set_shortcut_toggled(SHORTCUT_NAME, remotes.paused(event.player_index))
--     end,

--     -- ['ixuAutoCrafting-increase'] = function(event) change(event, true) end,
--     -- ['ixuAutoCrafting-decrease'] = function(event) change(event, false) end

--     -- [SHORTCUT_NAME] = function(event)
--     --     local p = game.players[event.player_index]
--     --     printOrFly(p, '2')
--     --     set_enabled(p, not p.is_shortcut_toggled(SHORTCUT_NAME))
--     -- end,
-- }
-- for event_name, handler in pairs (event_handlers) do
--   script.on_event(defines.events[event_name] or event_name, handler)
-- end


function enabled(player_index, set)
    if set ~= nil then
        global.players_data[player_index].paused = not set
        local p = game.players[player_index]
        -- printOrFly(p, set)
        p.set_shortcut_toggled(SHORTCUT_NAME, enabled(player_index))
    end
    return not global.players_data[player_index].paused
end

script.on_event(defines.events.on_lua_shortcut, function(event)
    -- game.print(game.table_to_json(event))
    if event.prototype_name ~= SHORTCUT_NAME then return end
    local p = game.players[event.player_index]
    enabled(event.player_index, not enabled(event.player_index))
end)

function change(event, positive)
    local p = game.players[event.player_index]
    -- init_player(event.player_index, false)
    local d = global.players_data[event.player_index]
    d.hh_request_tick = game.tick
    -- global.players_data[event.player_index] = d
    if d.paused == true then
        enabled(event.player_index, true)
        -- d = global.players_data[event.player_index]
        printOrFly(p, mod_name..' is now running again!')
        return
    end
    local item = 'Default'
    if p.cursor_stack.valid_for_read == true then
        item = p.cursor_stack.name
        if d.settings[item] == nil then
            d.settings[item] = d.settings['Default']
        end
    end
    local changed = false
    if positive then
        for i = 1, #max_arr do
            if max_arr[i] > d.settings[item] then
                d.settings[item] = max_arr[i]
                changed = true
                break
            end
        end
    else
        for i = 1, #max_arr do
            if max_arr[#max_arr-i+1] < d.settings[item] then
                d.settings[item] = max_arr[#max_arr-i+1]
                changed = true
                break
            end
        end
    end
    if changed then
        function settingsmessage(item)
            local trash_warning = ''
            if p.character and p.auto_trash_filters[item] ~= nil and p.auto_trash_filters[item] < math.ceil(game.item_prototypes[item].stack_size*d.settings[item]) then
                trash_warning = ' [Auto trash: '..p.auto_trash_filters[item]..']'
            end
            return '[item='..item..']: '..d.settings[item]..' stacks ('..math.ceil(game.item_prototypes[item].stack_size*d.settings[item])..' items)'..trash_warning
        end
        function printall()
            p.print('Changed default autocraft stack size: '..d.settings['Default']..' stacks.')
            for k in pairs(d.settings) do
                if k ~= 'Default' then p.print(settingsmessage(k)) end
            end
        end
        if item == 'Default' then printall()
        else printOrFly(p, settingsmessage(item)) end
    elseif positive == false and d.settings[item] == 0 then
        if item == 'Default' then
            init_player_settings(event.player_index, true)
            d = global.players_data[event.player_index]
            d.settings['Default'] = 0
            p.print('All your ixuAutoCrafting settings were deleted.')
        else
            d.settings[item] = nil
            printOrFly(p, 'Your ixuAutoCrafting setting for [item='..item..'] was deleted [Default is '..d.settings['Default']..' stacks]')
        end
    end
    global.players_data[event.player_index] = d
end

function printOrFly(p, text)
    if p.character ~= nil then
        p.create_local_flying_text({
            ['text'] = text,
            ['position'] = p.character.position
        })
    else
        p.print(text)
    end
end

function init(event, force)
    -- Might be called with nil event and force
    if global.players_data == nil or force then
        global.players_data = {}
    end
    for i = 1, #game.players do
        init_player(i,force)
    end
end

function init_player(player_index, force)
    return init_player_settings(player_index, force)
end

function init_player_settings(player_index, force)
    local wasnil = global.players_data[player_index] == nil
    if wasnil or force then
        global.players_data[player_index] = {}
        global.players_data[player_index].hh_request_tick = 0
        global.players_data[player_index].hh_last_exec_tick = 0
        global.players_data[player_index].settings = {}
        global.players_data[player_index].settings['Default'] = 0.2
        -- Only for beginners, so that you don't lose your starting iron to ammo before you have your pick axe ;>
        if wasnil then
            global.players_data[player_index].settings['firearm-magazine'] = 0.05*100/game.item_prototypes['firearm-magazine'].stack_size
            game.players[player_index].print(
                'ixuAutoCrafting autocrafting enabled for quickbar filtered items and ammo. Default amount: '..global.players_data[player_index].settings['Default']..' stacks.'
            )
            game.players[player_index].print('Change ixuAutoCrafting autocrafting limits with hotkeys (Options > Controls > Mods).')
            game.players[player_index].print('Empty cursor: change Default. Forget all ixuAutoCrafting settings by deleting Default setting.')
            game.players[player_index].print('Individual item settings are modified when held in cursor and deleted when decreased below 0.')
        end
    end
    return global.players_data[player_index]
end

script.on_init(init)
script.on_event(defines.events.on_player_joined_game, function(event) init_player(event.player_index, false) end)

-- Data migration
script.on_configuration_changed(function(event)
    for i, p in pairs(game.players) do
        if global.players_data and global.players_data[i] then
            local d = global.players_data[i]
            for item, value in pairs(d.settings) do
                if item ~= 'Default' and not game.item_prototypes[item] then d.settings[item] = nil end
            end
        end
    end
end)

remote.add_interface('ixuAutoCrafting', {
    -- call change(player_index, positive) to simulate increase/decrease hotkey events. (positive: true for increase, false for decrease)
    change = function(player_index, positive) change({player_index = player_index}, positive) end,
    -- set(player_index, item, limit) sets the autocraft limit for a specific item to the provided limit. limit == nil to remove the setting. Limits are in stacks (float).
    set = function(player_index, item, limit) global.players_data[player_index].settings[item] = limit end,
    -- call settings(player_index) to get the Key/Value pairs to get the stack size limits
    -- call settings(player_index, limits) to set the Key/Value pairs stack size limits (limits is a Key[item_name] --> Value (stacks_to_craft) object)
    settings = function(player_index, limits)
        if limits ~= nil then
            global.players_data[player_index].settings = limits
        end
        return global.players_data[player_index].settings
    end,
    -- call enabled(player_index) to get enabled state for the player
    -- call enabled(player_index, set) to set enabled state for the player (set is a boolean)
    enabled = enabled,
})