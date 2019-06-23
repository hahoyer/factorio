require "util"

function msg(message)
    for _, p in pairs(game.players) do
        p.print(message)
    end
end

function formatDatetime(value)
    return value.year .. "/" .. value.month .. "/" .. value.day .. " " ..
            value.hour .. ":" .. value.minute .. ":" .. value.second
end

function on_tick(event)
    local lastTime = global.Mmasf.TimeStamp
    local thisTime = os.time
    msg("LastTime: " .. formatDateTime(lastTime))
    msg("ThisTime: " .. formatDateTime(lastTime))
    global.Mmasf.TimeStamp = thisTime
end

--script.on_event (defines.events.on_tick, on_tick)
script.on_init(function()
    --    global.Mmasf = {TimeStamp = os.time}
end)

play_time_seconds = -1

function FormatPlaytimeInformation(player)
    local ticks  = game.tick
    local daytime = player.surface.daytime + 0.5
    local lightness = 1 - player.surface.darkness
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

    local lightnessString = string.format("%d%%", lightness*100)

    return hourString .. playtimeString .. " " .. crashdaysString .. " " .. daytimeString .. " " .. lightnessString
end

function Show(event)
    local previous = play_time_seconds

    play_time_seconds = math.floor(game.tick / 60)

    if previous == play_time_seconds then
        return
    end


    for i, player in pairs(game.connected_players) do
        if player.gui.top.clockGUI == nil then
            player.gui.top.add { type = "label", name = "hwclock" }
        end

        local value = FormatPlaytimeInformation(player)
        player.gui.top.clockGUI.caption = value
    end
end

script.on_event(defines.events.on_tick, Show)
