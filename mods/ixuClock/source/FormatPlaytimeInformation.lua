local mod_gui = require("mod-gui")
require("util")

local play_time_seconds = -1

function FormatPlaytimeInformation(player)
    local ticks = game.tick
    local daytime = player.surface.daytime + 0.5
    local crashdays = game.tick / player.surface.ticks_per_day

    local seconds = math.floor(ticks / 60)
    local minutes = math.floor(seconds / 60)
    local hours = math.floor(minutes / 60)
    local days = math.floor(hours / 24)
    local hourString = ""
    if hours > 0 then hourString = string.format("%d:", hours % 24) end
    local dayString = ""
    if days > 0 then dayString = string.format("%d.", days) end
    local playtimeString = string.format("%02d:%02d", minutes % 60, seconds % 60)

    local crashdaysString = string.format("%d", crashdays);

    local dayminutes = math.floor(daytime * 24 * 60) % 60
    local dayhour = math.floor(daytime * 24) % 24
    local daytimeString = string.format("%02d:%02d", dayhour, dayminutes)

    return dayString .. hourString .. playtimeString .. " " .. crashdaysString .. " "
               .. daytimeString
end

function ShowFormatPlaytimeInformation(event)
    local previous = play_time_seconds

    play_time_seconds = math.floor(game.tick / 60)

    if previous == play_time_seconds then return end

    for i, player in pairs(game.connected_players) do
        -- remove legacy clock line
        if player.gui.top.hwclock then player.gui.top.hwclock.destroy() end

        local hwclock = mod_gui.get_frame_flow(player).hwclock
        if not hwclock then
            hwclock = mod_gui.get_frame_flow(player).add {type = "label", name = "hwclock"}
        end
        local value = FormatPlaytimeInformation(player)
        hwclock.caption = value
    end
end

