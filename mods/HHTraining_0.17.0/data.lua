function CreatePreset(order, frequency, stonefrequency, richness, size)
    local preset = {
        order = order,
        basic_settings =
        {
            autoplace_controls =
            {
                coal = { frequency = frequency, richness = richness, size = size },
                ["copper-ore"] = { frequency = frequency, richness = richness, size = size },
                ["crude-oil"] = { frequency = 1, richness = 2, size = 1 },
                ["enemy-base"] = { frequency = 0, size = "none" },
                ["iron-ore"] = { frequency = frequency, richness = richness, size = size },
                stone = { frequency = stonefrequency, richness = 1, size = size },
            },
			 cliff_settings={richness = 0}
        },
        advanced_settings =
        {
            enemy_evolution =
            {
                enabled = false
            },
  difficulty_settings=
  {
    research_queue_setting = "always"
  },

        }
	}

	if(hardCrafting) then
		preset.basic_settings.autoplace_controls["rich-iron-ore"] = preset.basic_settings.autoplace_controls["iron-ore"]
		preset.basic_settings.autoplace_controls["rich-copper-ore"] = preset.basic_settings.autoplace_controls["copper-ore"]
        preset.basic_settings.autoplace_controls["oil-sand"] = preset.basic_settings.autoplace_controls["crude-oil"]
	end
	return preset
end

local mapGenPresets = data.raw["map-gen-presets"].default

mapGenPresets["hw"] = CreatePreset("9z", "normal", "normal", "normal", "normal")
mapGenPresets["hwDense"] = CreatePreset("9y", "very-high", "normal", "very-good", "very-good")

data.raw["character"]["character"].build_distance = 10000
data.raw["character"]["character"].reach_distance = 10000
data.raw["character"]["character"].reach_resource_distance = 10000
data.raw["character"]["character"].drop_item_distance = 10000

data.raw["character"]["character"].inventory_size = 240


