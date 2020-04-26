local Constants = require('constants')
local Table = require('Table')
local Gui = require('gui')


local function Names(lab, name)
  local inventory = lab.get_inventory(defines.inventory.lab_input)
  for index = 1, #inventory do
    local slot = inventory[index]
    if slot.valid_for_read ~= false and slot.name and slot.name == name and slot.count > 0 then return 0 end
  end
  return 1
end

local function CountNotFoundIngredients(inactivelabs, name)
  local ingredients = Table.Select(inactivelabs, function(lab) return Names(lab,name) end)
  local result = ingredients:Sum()
  return result
end

local function GetInactiveLabs(labs)
  return Table.Where(labs, function(lab) return lab.status == defines.entity_status.missing_science_packs end)
end

local function CollectCurrentResearchIngredients()
  local labs = global.Labs
  if labs == nil or #labs == 0 then return {} end

  local currentResearch = game.forces.player.current_research
  if currentResearch == nil then return {} end
  
  local inactiveLabs = GetInactiveLabs(labs)
  local ingredients = currentResearch.research_unit_ingredients
  local missingIngredients = Table.ToPairs
  (
    ingredients, 
    function(ingredient) return ingredient.name end,
    function(ingredient) return CountNotFoundIngredients(inactiveLabs, ingredient.name) end
  )

  return 
  {
    MissingIngredients = missingIngredients,
    Labs = #labs,
    InactiveLabs = #inactiveLabs
  }
end

local function SetSignals(configuration, signals)
  local behavior = configuration.Entity.get_control_behavior()
  for index = 1,behavior.signals_count do behavior.set_signal(index, nil) end
  local index = 0
  if signals.MissingIngredients then
    for name, count in pairs(signals.MissingIngredients) do
      if count ~= 0 then
        index = index+1
        behavior.set_signal(index, {signal={type="item", name=name}, count=-count})
      end
    end
  end

  if signals.Labs and signals.Labs ~= 0 and configuration.LabsSignal then
    index = index+1
    behavior.set_signal(index, {signal=configuration.LabsSignal, count=signals.Labs})
  end

  if signals.InactiveLabs and signals.InactiveLabs ~= 0 and configuration.InactiveLabsSignal then
    index = index+1
    behavior.set_signal(index, {signal=configuration.InactiveLabsSignal, count=-signals.InactiveLabs})
  end
end

local function CalculateSignals()
  local monitors = global.Monitors
  if not monitors or not Table.Any(monitors) then return end
  local signals = CollectCurrentResearchIngredients()
  Table.Select(monitors, function(monitor)return SetSignals(monitor, signals) end)
end

local function on_tick()
  CalculateSignals()
end

local function OpenGui(player, entity)
  local data = global.Monitors[entity.unit_number]
  Gui.entity
  (
    entity,
    {
      Gui.section
      {
        name = "Monitor",
        Gui.ChooseElementButton("LabsSignal","signal", data.LabsSignal),
        Gui.ChooseElementButton("InactiveLabsSignal","signal",data.InactiveLabsSignal),
      },
    }
  )
  :open(player)
end

local function on_gui_opened(event)
  local entity = event.entity
  if not entity then return end
  if entity.name == "metalab-monitor" then OpenGui(game.players[event.player_index], entity) end
end

local function on_gui_closed(event)
	local element = event.element
	if element and element.valid and element.name and element.name:match("^"..Constants.ModName..":") then
		element.destroy()
	end
end

local function on_init()
  global.Monitors = global.Monitors or {}
  global.Labs = global.Labs or {}
end

local function on_built(event)
  local entity = event.created_entity or event.entity
  if entity and entity.name == "metalab-monitor" then
    global.Monitors[entity.unit_number] =
    {
      Entity = entity,
      LabsSignal= {type=Constants.LabsSignal.type, name=Constants.LabsSignal.name},
      InactiveLabsSignal = {type=Constants.InactiveLabsSignal.type,name=Constants.InactiveLabsSignal.name}
    }
  elseif entity and entity.name == "lab" then
    table.insert(global.Labs,entity)
  end

end

local function on_destroyed(event)
  local entity = event.entity
  if entity and entity.name == "metalab-monitor" then
    global.Monitors[entity.unit_number] = nil
  elseif entity and entity.name == "lab" then
    Table.Remove( global.Labs, entity)
  end
end

local function on_gui_changed(event)
	local element = event.element
end

local function on_gui_elem_changed(event)
	local element = event.element
  if element and element.valid and element.name and element.name:match("^"..Constants.ModName..":") then
    local gui_name, unit_number, elementPath = Gui.parse_entity_gui_name(element.name)
    if gui_name == 'metalab-monitor' then
      local parts = Gui.split(elementPath, ":")
      if parts[1] == "Monitor" then
        global.Monitors[unit_number][parts[2]] = element.elem_value
      end
    end
  end
end

local function on_gui_click(event)
	local element = event.element
  if element and element.valid and element.name and element.name:match("^"..Constants.ModName..":") then
    return
  else
    return
  end

end

script.on_event(defines.events.on_built_entity, on_built)
script.on_event(defines.events.on_robot_built_entity, on_built)
script.on_event(defines.events.script_raised_built, on_built)
script.on_event(defines.events.script_raised_revive, on_built)

script.on_event(defines.events.on_pre_player_mined_item, on_destroyed)
script.on_event(defines.events.on_robot_pre_mined, on_destroyed)
script.on_event(defines.events.on_entity_died, on_destroyed)
script.on_event(defines.events.script_raised_destroy, on_destroyed)

script.on_init(on_init)
--script.on_event(defines.events.on_tick, on_tick)
script.on_nth_tick(60,on_tick)

script.on_event(defines.events.on_gui_checked_state_changed, on_gui_changed)
script.on_event(defines.events.on_gui_click, on_gui_click)
script.on_event(defines.events.on_gui_closed, on_gui_closed)
script.on_event(defines.events.on_gui_confirmed, on_gui_changed)
script.on_event(defines.events.on_gui_elem_changed, on_gui_elem_changed)
script.on_event(defines.events.on_gui_opened, on_gui_opened)
script.on_event(defines.events.on_gui_selection_state_changed, on_gui_changed)
script.on_event(defines.events.on_gui_switch_state_changed, on_gui_changed)
script.on_event(defines.events.on_gui_text_changed, on_gui_changed)
script.on_event(defines.events.on_gui_value_changed, on_gui_changed)


