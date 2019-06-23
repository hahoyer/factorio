local mapGenPresets = data.raw["map-gen-presets"].default
mapGenPresets.default.default=false

data.raw.tool["automation-science-pack"].icon  = "__HHTraining__/graphics/science-pack-1.png";
data.raw.tool["logistic-science-pack"].icon  = "__HHTraining__/graphics/science-pack-2.png";
data.raw.item["electronic-circuit"].icon  = "__HHTraining__/graphics/electronic-circuit.png";
data.raw.item["advanced-circuit"].icon  = "__HHTraining__/graphics/advanced-circuit.png";
data.raw.item["processing-unit"].icon  = "__HHTraining__/graphics/processing-unit.png";

data.raw["electric-pole"]["big-electric-pole"].maximum_wire_distance = 32;

-- data.raw["radar"]["radar"].max_distance_of_sector_revealed = 100;
-- data.raw["radar"]["radar"].max_distance_of_nearby_sector_revealed = 1;

if apm then
	require('util')
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_1', 'apm_wood_pellets', 15)
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_2', 'apm_wood_pellets', 0)
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_2', 'apm_wood_pellets', 0)
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_2', 'apm_wood_pellets', 20)
	apm.lib.utils.recipe.result.add_with_probability('apm_wood_pellets_2', 'apm_wood_pellets', 0, 1, 0.5)
	apm.lib.utils.recipe.result.mod('apm_resin', 'apm_wood_pellets', 5)
	apm.lib.utils.recipe.result.mod('apm_resin', 'apm_resin', 5)
end