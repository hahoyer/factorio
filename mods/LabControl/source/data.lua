local Constants = require("Constants")

local entity = table.deepcopy(data.raw['constant-combinator']['constant-combinator'])
entity.name = Constants.ControllerName
entity.icon = Constants.GraphicsPath..'icons/monitor.png'
entity.minable.result = entity.name
entity.item_slot_count = 1
entity.sprites.north.layers[1].filename = Constants.GraphicsPath..'entity/monitor.png'
entity.sprites.south.layers[1].filename = Constants.GraphicsPath..'entity/monitor.png'
entity.sprites.east.layers[1].filename = Constants.GraphicsPath..'entity/monitor.png'
entity.sprites.west.layers[1].filename = Constants.GraphicsPath..'entity/monitor.png'
entity.sprites.north.layers[1].hr_version.filename = Constants.GraphicsPath..'entity/hires/monitor.png'
entity.sprites.west.layers[1].hr_version.filename = Constants.GraphicsPath..'entity/hires/monitor.png'
entity.sprites.south.layers[1].hr_version.filename = Constants.GraphicsPath..'entity/hires/monitor.png'
entity.sprites.east.layers[1].hr_version.filename = Constants.GraphicsPath..'entity/hires/monitor.png'


local item = table.deepcopy(data.raw['item']['constant-combinator'])
item.name = entity.name
item.icon = entity.icon
item.icon_size = entity.icon_size
item.icon_mipmaps = entity.icon_mipmaps
item.place_result = entity.name
item.subgroup = 'circuit-network'
item.order = 'c[combinators]-m[metalab]'
item.stack_size = 10

local recipe = table.deepcopy(data.raw['recipe']['constant-combinator'])
recipe.name = entity.name
recipe.result = entity.name
table.insert(data.raw['technology']['circuit-network'].effects, {type = 'unlock-recipe', recipe = entity.name})

data:extend({entity, item, recipe})
