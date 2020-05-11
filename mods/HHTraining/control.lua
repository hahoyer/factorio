
local function getInfoFromEntity(player, target)
    player.print("selected entity: ".. target.type .. " " .. target.name)
end

local function getInfoFromPrototype(player, target)
    player.print("selected prototype: ".. target.type .. " " .. target.name)
end

script.on_event("hh-training-get-info", function(event)
    local player = game.players[event.player_index]
    local fs = nil
    if player.selected then
        getInfoFromEntity(player, player.selected)
        fs = true
    end
    if player.cursor_stack.valid_for_read then
        getInfoFromPrototype(player, player.cursor_stack.prototype)
        fs = true
    end
    if fs then return end
    player.print(serpent.block(event))
end)

script.on_event(defines.events.on_player_joined_game, function(event)
    game.players[event.player_index].game_view_settings.show_entity_info = true
end)
