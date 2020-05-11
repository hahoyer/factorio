Constants = require("Constants")

local result = {}

local function Clone(orig)
	local orig_type = type(orig)
	local copy
	if orig_type == 'table' then
		copy = {}
		for orig_key, orig_value in next, orig, nil do
				copy[Clone(orig_key)] = Clone(orig_value)
		end
		setmetatable(copy, Clone(getmetatable(orig)))
	else -- number, string, boolean, etc
		copy = orig
	end
	return copy
end

local function GetEntity()
  local template =
  {
    type = "container",
    name = "lumberjack",
    inventory_size = 12,
    icon = Constants.GraphicsPath.."icons/lumberjack.png",
    icon_size = 32,
    flags = {"placeable-neutral","player-creation"},
    minable = {mining_time = 0.3, result = "lumberjack"},
    max_health = 50,
    corpse = "big-remnants",
    collision_box = {{-1.8, -1.8}, {1.8, 1.8}},
    selection_box = {{-1.8, -1.8}, {1.8, 1.8}},
    picture = {
        filename = Constants.GraphicsPath.."entity/lumberjack.png",
        priority = "extra-high",
        width = 260,
        height = 240,
        scale_y = 2,
        scale_x = 6,
        shift = {0.40625, -0.71875},
          },

    close_sound = {filename = "__base__/sound/wooden-chest-close.ogg",volume = 0.8},
    open_sound = {filename = "__base__/sound/wooden-chest-open.ogg",volume = 0.8},
    dying_explosion = "wooden-chest-explosion",
    vehicle_impact_sound = {
      {
          filename = "__base__/sound/car-wood-impact.ogg",
          volume = 0.5
      },
      {
          filename = "__base__/sound/car-wood-impact-02.ogg",
          volume = 0.5
      },
      {
          filename = "__base__/sound/car-wood-impact-03.ogg",
          volume = 0.5
      },
      {
          filename = "__base__/sound/car-wood-impact-04.ogg",
          volume = 0.5
      },
      {
          filename = "__base__/sound/car-wood-impact-05.ogg",
          volume = 0.5
      }
    }
  }
  return template
end

local function GetItem()
  return
  {
    type = "item",
    name = "lumberjack",
    icon = Constants.GraphicsPath .. "icons/lumberjack.png",
    icon_size = 32,
    subgroup = "extraction-machine",
    place_result = "lumberjack",
    order = "a[items]- [lumberjack]",
    stack_size = 10
  }
end

local function GetRecipe()
  return
    {
      type = "recipe",
      name = "lumberjack",
      ingredients ={{"wood", 12}},
      result = "lumberjack",
      subgroup = "extraction-machine",
      order = "a[items]- [lumberjack]"
    }
end

local function GetAnimation()
  return
    {
      type = "animation",
      name = "lumberjack",
      filename = Constants.GraphicsPath.."entity/lumberjack.animation.png",
      priority = "extra-high",
      width = 260,
      height = 240,
      frame_count = 12,
      line_length = 6,
      shift = {0.40625, -0.71875},
      animation_speed = 0.1
  }
  end

function result.RegisterData()
    data:extend
    ({
      GetEntity(),
      GetRecipe(),
      GetItem(),
      GetAnimation()
    })
end

local LumberJack = {}

function LumberJack:new(target)
  if not target then target = {} end
  setmetatable(target,self)
  self.__index = self
  return target
end

function LumberJack:IsPassive()
  if self.Entity.to_be_deconstructed("player") then return true end
  
  local sleepTime = self.SleepTime or 0
  self.SleepTime = (sleepTime + 1) % Constants.ChoppingTreshold
  return sleepTime > 0
end

function LumberJack:FindTree()
  while self.Range < Constants.WorkingRange.Maximum do
    local searchArea = {
        {self.Entity.position.x-self.Range,self.Entity.position.y-self.Range},
        {self.Entity.position.x+self.Range,self.Entity.position.y+self.Range}
    }

    local result = self.Entity.surface.find_entities_filtered{type="tree",limit=1,area=searchArea}[1]
    if result then return result end

    self.Range = self.Range + 1
  end
end

function CountWood(tree)
  local treeProducts = tree.prototype.mineable_properties.products
  if not treeProducts then return 0 end

  local result = 0
  for _,product in pairs(treeProducts) do
    if product.name == "wood" and product.amount then
      result = result + product.amount
    end
  end

  return result
end

function LumberJack:SetIsActive(value)
  rendering.set_animation_speed(self.AnimationId,  value and 1 or 0 )
end

function LumberJack:OnTick()
  if self:IsPassive() then return end

  self:SetIsActive(false)
  local tree = self:FindTree()
  if not tree then
    self.Entity.order_deconstruction("player")
    return
  end

  local newWood = {name="wood", count=CountWood(tree)}
  if newWood.count > 0 then
    local inventory = self.Entity.get_output_inventory()
    local free = inventory.get_insertable_count(newWood.name)
    if free < newWood.count then
      return
    end

    self:SetIsActive(true)
    inventory.insert(newWood)
  end

  tree.die()
end

local function UnregisterTickHandler()
  script.on_nth_tick(Constants.ChoppingFrequency,nil)
end

local function OnTick()
  for index,lumberJack in pairs(global.LumberJacks) do
    if not lumberJack.Entity.valid then
      table.remove(global.LumberJacks, index)
    else
      LumberJack:new(lumberJack):OnTick()
    end
  end
  if #global.LumberJacks == 0 then
      global.LumberJacks = nil
      UnregisterTickHandler()
  end
end

local function RegisterTickHandler()
  if global.LumberJacks then
      script.on_nth_tick(Constants.ChoppingFrequency,OnTick)
  end
end

local function OnBuilt(entity)
    if entity.name == "lumberjack" then
        if not global.LumberJacks then
            global.LumberJacks = {}
            RegisterTickHandler()
        end
        local animationId = rendering.draw_animation{animation="lumberjack",surface=entity.surface, target=entity}
        local lumberJack =
        {
          Entity=entity,
          Range=Constants.WorkingRange.Start,
          AnimationId = animationId
        }
        global.LumberJacks[#global.LumberJacks+1] = lumberJack

    end
end

function result.RegisterControlHandling()
  script.on_load(function()
    if global.LumberJacks then
        RegisterTickHandler()
    end
  end)

  script.on_event(
      {
          defines.events.on_built_entity,
          defines.events.on_robot_built_entity
      },
      function(event) OnBuilt(event.created_entity) end
  )
end

return result
