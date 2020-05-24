local Constants = require("constants")
local Table = require("Table")
local Gui = require("gui")
local showLog = false

local function AddTechnology(result, technology, count)
  if not technology or technology.researched == true then
    return 
  end
  local prerequisites = Table:new(technology.prerequisites):Where(function(prerequisite)return not prerequisite.researched end)
  if prerequisites:Any() then
    for _, prerequisite in ipairs(prerequisites) do
      AddTechnology(result, prerequisite, count)
    end
    return 
  end
  result[technology.name] = (result[technology.name] or 0) + count
end

local function AddSignal(result, signal, force)
  if signal.signal.type ~= "virtual" then
    return
  end
  local technologyName = signal.signal.name:match("^technology%-(.*)$")
  local technology = force.technologies[technologyName]
  AddTechnology(result, technology, signal.count)
end

local function AddSignals(result, entity, wire)
  local network = entity.get_circuit_network(wire)
  local signals = network and network.signals or {}
  for index = 1, #signals do
    AddSignal(result, signals[index], entity.force)
  end
end

function GetGroupWithMaxValue(target)
  local result = Table:new {}

  local maxValue = nil
  for key, value in pairs(target) do
    if not maxValue or maxValue < value then
      maxValue = value
      result = Table:new {}
    end
    if maxValue == value then
      table.insert(result, key)
    end
  end

  return result
end

function GetTechnologyProgress(technologyName, force)
  local progress = force.get_saved_technology_progress(technologyName)
  if progress then
    return progress
  end
  if force.current_research and force.current_research.name == technologyName then
    return force.research_progress
  end
  return 0
end

function GetPrice(technologyName, force)
  local technology = force.technologies[technologyName]
  local progress = GetTechnologyProgress(technologyName, force)

  local ingredients =
    Table:new(technology.research_unit_ingredients):Select(
    function(ingredient)
      return ingredient.amount
    end
  ):Sum()

  local result = technology.research_unit_count * technology.research_unit_energy * ingredients * (1 - progress)
  if showLog then
    log(
      "Price: " ..
        technology.name ..
          " = " ..
            technology.research_unit_count ..
              "*" ..
                technology.research_unit_energy .. "*" .. ingredients .. "* (1 - " .. progress .. ")" .. " = " .. result
    )
  end
  return result
end

function GetCheapestGroup(target, force)
  if #target <= 1 then
    return target
  end

  local result = Table:new {}

  local minValue = nil
  for _, technologyName in ipairs(target) do
    local value = GetPrice(technologyName, force)

    if not minValue or minValue > value then
      minValue = value
      result = Table:new {}
    end
    if minValue == value then
      table.insert(result, technologyName)
    end
  end

  return result
end

local function AddResearchRequests(result, labControl)
  local entity = labControl.Entity
  local red = entity.circuit_connected_entities.red
  local green = entity.circuit_connected_entities.green
  if #red == 0 and #green == 0 then
    return
  end
  AddSignals(result, entity, defines.wire_type.red)
  AddSignals(result, entity, defines.wire_type.green)
end

local function GetRecentResearchRequests()
  local requests = Table:new {}
  for _, labControl in pairs(global.Labcontrols) do
    requests[labControl.Entity.force.index] = Table:new {}
  end

  for _, labControl in pairs(global.Labcontrols) do
    AddResearchRequests(requests[labControl.Entity.force.index], labControl)
  end

  return requests
end

local function HandleRequestForPlayer(force, requests)
  local all = GetGroupWithMaxValue(requests)
  local cheapest = GetCheapestGroup(all, force)

  local current = force.current_research
  local newRequest = nil
  for _, request in ipairs(cheapest) do
    if current and request == current.name then
      return
    end
    newRequest = request
  end
  if not newRequest then
    return
  end
  local oldProgress = force.research_progress
  force.cancel_current_research()
  force.add_research(newRequest)
  if showLog then
    log(
      "Changed research: " ..
        (current and (current.name .. "(" .. oldProgress .. ")") or "none") ..
          " -> " .. newRequest .. "(" .. force.research_progress .. ")"
    )
  end
end

local function CalculateSignals()
  local actors = global.Labcontrols
  if not actors or not Table.Any(actors) then
    return
  end
  local requestsForPlayers = GetRecentResearchRequests()
  for index, value in pairs(requestsForPlayers) do
    HandleRequestForPlayer(game.forces[index], value)
  end
end

local function on_tick()
  CalculateSignals()
end

local function OpenGui(player, entity)
  local data = global.Labcontrols[entity.unit_number]
  Gui.entity(
    entity,
    {
      Gui.section {
        name = "Labcontrol"
      }
    }
  ):open(player)
end

local function on_gui_opened(event)
  local entity = event.entity
  if not entity then
    return
  end
  if entity.name == "Labcontrol" then
    OpenGui(game.players[event.player_index], entity)
  end
end

local function on_gui_closed(event)
  local element = event.element
  if element and element.valid and element.name and element.name:match("^" .. Constants.ModName .. ":") then
    element.destroy()
  end
end

local function on_init()
  global.Labcontrols = global.Labcontrols or {}
end

local function on_built(event)
  local entity = event.created_entity or event.entity
  if entity and entity.name == "Labcontrol" then
    global.Labcontrols[entity.unit_number] = {
      Entity = entity
    }
  end
end

local function on_destroyed(event)
  local entity = event.entity
  if entity and entity.name == "Labcontrol" then
    global.Labcontrols[entity.unit_number] = nil
  end
end

local function on_gui_changed(event)
  local element = event.element
end

local function on_gui_elem_changed(event)
  local element = event.element
  if element and element.valid and element.name and element.name:match("^" .. Constants.ModName .. ":") then
    local gui_name, unit_number, elementPath = Gui.parse_entity_gui_name(element.name)
    if gui_name == "Labcontrol" then
      local parts = Gui.split(elementPath, ":")
      if parts[1] == "Labcontrol" then
        global.Labcontrols[unit_number][parts[2]] = element.elem_value
      end
    end
  end
end

local function on_gui_click(event)
  local element = event.element
  if element and element.valid and element.name and element.name:match("^" .. Constants.ModName .. ":") then
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
script.on_nth_tick(60, on_tick)

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
