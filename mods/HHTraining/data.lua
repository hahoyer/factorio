function provideMilk()


data:extend(
        {
            {
                type = "fluid",
                name = "milk",
                default_temperature = 15,
                max_temperature = 100,
                heat_capacity = "0.2KJ",
                base_color = { r = 0.9, g = 0.9, b = 0.9 },
                flow_color = { r = 0.9, g = 0.9, b = 0.9 },
                icon = "__HHTraining__/graphics/icons/milk.png",
                icon_size = 32,
                order = "a[fluid]-a[milk]"
            },

            {
                type = "autoplace-control",
                name = "milk",
                richness = true,
                order = "b-m",
                category = "resource"
            },
            {
                type = "noise-layer",
                name = "milk"
            },
            {
                type = "resource",
                name = "milk",
                icon = "__HHTraining__/graphics/icons/milk.png",
                icon_size = 32,
                flags = { "placeable-neutral" },
                category = "basic-fluid",
                order = "a-b-a",
                infinite = true,
                highlight = true,
                minimum = 60000,
                normal = 300000,
                infinite_depletion_amount = 10,
                resource_patch_search_radius = 12,
                tree_removal_probability = 0.7,
                tree_removal_max_distance = 32 * 32,
                minable = {
                    mining_time = 1,
                    results = {
                        {
                            type = "fluid",
                            name = "milk",
                            amount_min = 10,
                            amount_max = 10,
                            probability = 1
                        }
                    }
                },
                collision_box = { { -1.4, -1.4 }, { 1.4, 1.4 } },
                selection_box = { { -0.5, -0.5 }, { 0.5, 0.5 } },
                stage_counts = { 0 },
                stages = {
                    sheet = {
                        filename = "__HHTraining__/graphics/entity/milk/milk.png",
                        priority = "extra-high",
                        width = 75,
                        height = 61,
                        frame_count = 4,
                        variation_count = 1
                    }
                },
                map_color = { r = 0.9, g = 0.9, b = 0.9 },
                map_grid = false
            },
        }
)

resource_generator.setup_resource_autoplace_data("milk", {
    name = "milk",
    order = "c",
    base_density = 8.2,
    base_spots_per_km2 = 1.8,
    random_probability = 1 / 48,
    random_spot_size_minimum = 1,
    random_spot_size_maximum = 1,
    additional_richness = 220000,
    has_starting_area_placement = true,
    regular_rq_factor_multiplier = 1
}
)

end

data.raw["character"]["character"].build_distance = 10000
data.raw["character"]["character"].reach_distance = 10000
data.raw["character"]["character"].reach_resource_distance = 10000
data.raw["character"]["character"].drop_item_distance = 10000

data.raw["character"]["character"].inventory_size = 240

-- provideMilk()