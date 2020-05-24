function SetAltModeOn(event)
    game.players[event.player_index].game_view_settings.show_entity_info = true
end

--script.on_event(defines.events.on_player_joined_game, SetAltModeOn)

script.on_event(defines.events.on_player_created, SetAltModeOn)
