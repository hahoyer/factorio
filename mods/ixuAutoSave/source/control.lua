local Constants = require("constants")
local TimeSpan = require("core/TimeSpan")

local function GetFrequency()
  local frequencyValue = settings.global[Constants.Frequency].value
  if not frequencyValue or frequencyValue == "" then
    return
  end
  local frequency = TimeSpan.FromString(frequencyValue)
  if frequency and frequency:getTicks() >= Constants.MinFrequencyTicks then
    return frequency:getTicks()
  end
end

local function CheckFrequency()
  local frequencyValue = settings.global[Constants.Frequency].value

  if not frequencyValue or frequencyValue == "" then
    return
  end

  local frequency = TimeSpan.FromString(frequencyValue)

  if not frequency then
    game.print({"message.invalidFrequency", frequencyValue}, {r=1})
    return
  end

  if frequency:getTicks() < Constants.MinFrequencyTicks then
    game.print({"message.tooSmallFrequency", serpent.block(frequency)}, {r=1})
    return
  end

  game.print{"message.actualFrequency", serpent.block(frequency)}
end

local function FinalizeGui()
  game.tick_paused = false
  script.on_event(defines.events.on_gui_confirmed, nil)
  script.on_event(defines.events.on_gui_closed, nil)
  CheckFrequency()
end

local function on_gui_confirmed(args)
  global.Prefix = args.element.text
  game.players[args.player_index].opened.destroy()
  FinalizeGui()
end

local function on_gui_closed(args)
  global.Prefix = settings.global[Constants.GlobalPrefix].value or ""
  args.element.destroy()
  FinalizeGui()
end

local function OpenGui(player)
  if global.Prefix then
    return
  end

  game.tick_paused = true

  local frame = {type = "frame", caption = "Prefix", direction = "vertical"}
  frame = player.gui.screen.add(frame)
  local textField = {type = "textfield"}
  textField.text = settings.global[Constants.GlobalPrefix].value or ""
  textField = frame.add(textField)
  player.opened = frame
  frame.force_auto_center()

  script.on_event(defines.events.on_gui_confirmed, on_gui_confirmed)
  script.on_event(defines.events.on_gui_closed, on_gui_closed)
end

local function on_tick(event)
  local name = global.Prefix

  if event.tick > 0 then
    local timeSpan = TimeSpan.FromTicks(event.tick)

    local dayPart = "_"
    local days = tostring(timeSpan.Days)
    if timeSpan.Days < 1 then
      dayPart = "_"
    else
      dayPart = days .. "."
    end
    if timeSpan.Days >= 10 then
      dayPart = "d" .. days.len .. "." + dayPart
    end

    name = global.Prefix .. dayPart .. timeSpan:getTimeAsHHMMSS()
  end
  if not (game.is_multiplayer()) then
    return game.auto_save(name)
  else
    return game.server_save(name)
  end
end

local function on_player_created(event)
  script.on_event(defines.events.on_player_created, nil)
  OpenGui(game.players[event.player_index])
end

local function RegisterOnTickHandler()
  local frequency = GetFrequency()
  if not frequency then return end

  -- Deregister former handler
  if global.Frequency then
    script.on_nth_tick(global.Frequency)
  end
  
  script.on_nth_tick(frequency, on_tick)
  global.Frequency = frequency
end

local function on_init()
  RegisterOnTickHandler()
  if settings.global[Constants.EnterPrefixOnInit].value then
    script.on_event(defines.events.on_player_created, on_player_created)
  else
    global.Prefix = settings.global[Constants.GlobalPrefix].value or ""
  end
end

local function on_runtime_mod_setting_changed(args)
  if args.setting == Constants.Frequency and args.setting_type == "runtime-global" then
    RegisterOnTickHandler()
    CheckFrequency()
  end
end

script.on_init(on_init)
script.on_event(defines.events.on_runtime_mod_setting_changed, on_runtime_mod_setting_changed)
