require('util')

data.raw["electric-pole"]["big-electric-pole"].maximum_wire_distance = 32;

-- data.raw["radar"]["radar"].max_distance_of_sector_revealed = 100;
-- data.raw["radar"]["radar"].max_distance_of_nearby_sector_revealed = 1;

if apm then
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_1', 'apm_wood_pellets', 15)
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_2', 'apm_wood_pellets', 0)
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_2', 'apm_wood_pellets', 0)
	apm.lib.utils.recipe.result.mod('apm_wood_pellets_2', 'apm_wood_pellets', 20)
	apm.lib.utils.recipe.result.add_with_probability('apm_wood_pellets_2', 'apm_wood_pellets', 0, 1, 0.5)
	apm.lib.utils.recipe.result.mod('apm_resin', 'apm_wood_pellets', 5)
	apm.lib.utils.recipe.result.mod('apm_resin', 'apm_resin', 5)
end

