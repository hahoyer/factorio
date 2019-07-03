require('util')

data.raw.tool["automation-science-pack"].icon  = "__HHTraining__/graphics/science-pack-1.png";
data.raw.tool["logistic-science-pack"].icon  = "__HHTraining__/graphics/science-pack-2.png";
data.raw.item["electronic-circuit"].icon  = "__HHTraining__/graphics/electronic-circuit.png";
data.raw.item["advanced-circuit"].icon  = "__HHTraining__/graphics/advanced-circuit.png";
data.raw.item["processing-unit"].icon  = "__HHTraining__/graphics/processing-unit.png";

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

function CreatePreset(order, frequency, stonefrequency, richness, size)
	local preset = {
		order = order,
		basic_settings = {
			autoplace_controls = {
				coal = { frequency = frequency, richness = richness, size = size },
				["copper-ore"] = { frequency = frequency, richness = richness, size = size },
				["crude-oil"] = { frequency = 1, richness = 2, size = 1 },
				["enemy-base"] = { frequency = 0, size = "none" },
				["iron-ore"] = { frequency = frequency, richness = richness, size = size },
				stone = { frequency = stonefrequency, richness = 1, size = size },
			},
			cliff_settings = { richness = 0 }
		},
		advanced_settings = {
			enemy_evolution = {
				enabled = false
			},
			difficulty_settings = {
				research_queue_setting = "always"
			},

		}
	}

    if settings.startup["hardcrafting-rich-ores"]
            and settings.startup["hardcrafting-rich-ores"].value == true
    then
            preset.basic_settings.autoplace_controls["rich-iron-ore"] = preset.basic_settings.autoplace_controls["iron-ore"]
            preset.basic_settings.autoplace_controls["rich-copper-ore"] = preset.basic_settings.autoplace_controls["copper-ore"]
            preset.basic_settings.autoplace_controls["oil-sand"] = preset.basic_settings.autoplace_controls["crude-oil"]
    end
	return preset
end

local mapGenPresets = data.raw["map-gen-presets"].default
mapGenPresets.default.default=false

mapGenPresets["hw"] = CreatePreset("a-z", "normal", "normal", "normal", "normal")
mapGenPresets["hwDense"] = CreatePreset("a-y", "very-high", "normal", "very-good", "very-good")

