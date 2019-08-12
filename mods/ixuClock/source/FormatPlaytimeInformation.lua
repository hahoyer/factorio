require("util")

play_time_seconds = -1

function FormatPlaytimeInformation(player)
    local ticks  = game.tick
    local daytime = player.surface.daytime + 0.5
    local windSpeed = player.surface.wind_speed
    local windOrientation = player.surface.wind_orientation
    local crashdays = game.tick / player.surface.ticks_per_day

    local seconds = math.floor(ticks / 60)
    local minutes = math.floor(seconds / 60)
    local hours = math.floor(minutes / 60)
    local hourString = ""
    if hours > 0 then
        hourString = string.format("%d:", hours)
    end
    local playtimeString = string.format("%02d:%02d", minutes % 60, seconds % 60)

    local crashdaysString  = string.format("%d", crashdays);

    local dayminutes = math.floor(daytime * 24 * 60) % 60
    local dayhour = math.floor(daytime * 24 ) % 24
    local daytimeString = string.format("%02d:%02d", dayhour, dayminutes)

    local windSpeedString = tostring(windSpeed)
    local directions = {"N","NNE","NE","ENE","E","ESE","SE","SSE", "S", "SSW","SW","WSW","W","WNW","NW", "NNW","N"}
    local windOrientationString = directions[math.floor(windOrientation*16 + 0.5)]

    return hourString
            .. playtimeString .. " "
            .. crashdaysString .. " "
            .. daytimeString
end

function ShowFormatPlaytimeInformation(event)
    local previous = play_time_seconds

    play_time_seconds = math.floor(game.tick / 60)

    if previous == play_time_seconds then
        return
    end


    for i, player in pairs(game.connected_players) do
        if player.gui.top.hwclock == nil then
            player.gui.top.add { type = "label", name = "hwclock" }
        end

        local value = FormatPlaytimeInformation(player)
        player.gui.top.hwclock.caption = value
    end
end


