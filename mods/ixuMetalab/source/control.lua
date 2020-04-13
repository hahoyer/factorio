local Gui = require('gui')
local Table = require('Table')


local function Names(lab, name)
  local inventory = lab.get_inventory(defines.inventory.lab_input)
  if Table.Any(inventory, function(slot) return slot.valid_for_read and slot.name == name and slot.count > 0 end)
  then return 0
  else return 1
  end
end

local function CountNotFoundIngredients(inactivelabs, name)
  local ingredients = Table.Select(inactivelabs, function(lab) return Names(lab,name) end)
  return Table.Sum(ingredients)
end

local function GetInactiveLabs(labs)
  return Table.Where(labs, function(lab) return lab.status == defines.entity_status.missing_science_packs end)
end

local function CollectCurrentResearchIngredients()
  local labs = game.surfaces[1].find_entities_filtered{name="lab"}
  local inactiveLabs = GetInactiveLabs(labs)
  local currentResearch = game.forces.player.current_research
  
  local result = {}
  if currentResearch == nil then return result end
  local ingredients = currentResearch.research_unit_ingredients
  for _, ingredient in ipairs(ingredients) do
    result[ingredient.name] = CountNotFoundIngredients(inactiveLabs, ingredient.name)
  end
  result = {MissingIngredients = result}

  result.Labs = #labs 
  result.InactiveLabs = #inactiveLabs
  return result
end

local function SetSignals(metalab, signals)
  local behavior = metalab.get_control_behavior()
  for index = 1,behavior.signals_count do behavior.set_signal(index, nil) end

  local configuration = global.monitor[metalab.unit_number]
  local index = 0

  if signals.MissingIngredients then
    for name, count in pairs(signals.MissingIngredients) do
      if count > 0 then
        index = index+1
        behavior.set_signal(index, {signal={type="item", name=name}, count=count})
      end
    end
  end

  if signals.Labs and signals.Labs > 0 and configuration.LabsSignal then
    index = index+1
    behavior.set_signal(index, {signal=configuration.LabsSignal, count=signals.Labs})
  end

  if signals.InactiveLabs and signals.InactiveLabs > 0 and configuration.InactiveLabsSignal then
    index = index+1
    behavior.set_signal(index, {signal=configuration.InactiveLabsSignal, count=signals.InactiveLabs})
  end
end

local function CalculateSignals()
  local metaLab = game.surfaces[1].find_entities_filtered{name="metalab-monitor"}
  if not Table.Any(metaLab) then return end
  local signals = CollectCurrentResearchIngredients()
  Table.Select(metaLab, function(behavior)return SetSignals(behavior, signals) end)
  local cc = metaLab
end

local nextTick = 0

local function on_tick(a)
  if a.tick < nextTick then return end
  nextTick = a.tick + 60

  CalculateSignals()
end

local function OpenGui(player, entity)
  local data = global.monitor[entity.unit_number]
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
	if element and element.valid and element.name and element.name:match("^metalab:") then
		element.destroy()
	end
end

local function on_init()
  global.monitor = global.monitor or {}
end

local function on_load()
local x = 0
end

local function on_built(event)
  local entity = event.created_entity or event.entity
  if entity and entity.name == "metalab-monitor" then
    global.monitor[entity.unit_number] =
    {
      LabsSignal= {type="virtual", name="signal-green"},
      InactiveLabsSignal = {type="item",name="lab"}
    }
  end

end

local function on_destroyed(event)
	local entity = event.entity
end

local function on_gui_changed(event)
	local element = event.element
end

local function on_gui_elem_changed(event)
	local element = event.element
  if element and element.valid and element.name and element.name:match('^metalab:') then
    local gui_name, unit_number, elementPath = Gui.parse_entity_gui_name(element.name)
    if gui_name == 'metalab-monitor' then
      local parts = Gui.split(elementPath, ":")
      if parts[1] == "Monitor" then
        global.monitor[unit_number][parts[2]] = element.elem_value
      end
    end
  end
end

local function on_gui_click(event)
	local element = event.element
  if element and element.valid and element.name and element.name:match('^metalab:') then
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
script.on_load(on_load)
script.on_event(defines.events.on_tick,on_tick)

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


