local Constants = require("Constants")

local big_size = 64
local small_size = 32
local tiny_size = 24

data:extend(
    {
        {type = "custom-input", name = Constants.Key.Main, key_sequence = "H"},
        {type = "custom-input", name = Constants.Key.Back, key_sequence = "mouse-button-4"},
        {type = "custom-input", name = Constants.Key.Fore, key_sequence = "mouse-button-5"},

        {
            type = "sprite",
            name = "factorio",
            filename = "__core__/graphics/factorio-icon.png",
            size = 64,
            scale = 0.5,
        },
        {
            type = "sprite",
            name = "ingteb",
            filename = "__" .. Constants.ModName .. "__/thumbnail.png",
            size = 64,
            scale = 0.5,
        },
        {
            type = "sprite",
            name = "hide-this-column",
            filename = "__core__/graphics/cancel.png",
            size = 64,
            scale = 0.5,
        },
    }
)

data.raw["utility-sprites"].default.factorio = {
    filename = "__core__/graphics/factorio.icon",
    priority = "medium",
    size = 32,
    flags = {"icon"},
}

data.raw["gui-style"].default["ingteb-flow-centered"] =
    {type = "horizontal_flow_style", horizontally_stretchable = "on", horizontal_align = "center"}

data.raw["gui-style"].default["ingteb-scroll-6x1"] =
    {
        type = "scroll_pane_style", --
        parent = "scroll_pane",
        width = 43 * 6,
    }

data.raw["gui-style"].default["ingteb-flow-right"] =
    { --
        type = "horizontal_flow_style", --
        horizontally_stretchable = "on",
        horizontal_align = "right",
    }

local default_glow_color = {225, 177, 106, 255}
local default_dirt_color = {15, 7, 3, 100}
local red_color = {1, 0, 0, 100}

local function offset_by_2_rounded_corners_glow(tint_value)
    return {
        position = {240, 736},
        corner_size = 16,
        tint = tint_value,
        top_outer_border_shift = 4,
        bottom_outer_border_shift = -4,
        left_outer_border_shift = 4,
        right_outer_border_shift = -4,
        draw_type = "outer",
    }
end

local function sprite17(x, y) return {border = 4, position = {x * 17, y * 17}, size = 16} end
data.raw["gui-style"].default["ingteb-light-button"] =
    {
        type = "button_style",
        parent = "button",
        draw_shadow_under_picture = true,
        size = 40,
        padding = 0,
        default_graphical_set = {
            base = sprite17(0, 1),
            -- shadow = offset_by_2_rounded_corners_glow(default_dirt_color)
        },
        hovered_graphical_set = {
            base = sprite17(2, 1),
            -- shadow = offset_by_2_rounded_corners_glow(default_dirt_color),
            -- glow = offset_by_2_rounded_corners_glow(default_glow_color)
        },
        clicked_graphical_set = {
            base = sprite17(3, 1),
            shadow = offset_by_2_rounded_corners_glow(default_dirt_color),
        },
        selected_graphical_set = {
            base = sprite17(2, 1),
            shadow = offset_by_2_rounded_corners_glow(default_dirt_color),
        },
        selected_hovered_graphical_set = {
            base = sprite17(2, 1),
            shadow = offset_by_2_rounded_corners_glow(default_dirt_color),
            glow = offset_by_2_rounded_corners_glow(default_glow_color),
        },
        selected_clicked_graphical_set = {
            base = sprite17(3, 1),
            shadow = offset_by_2_rounded_corners_glow(default_dirt_color),
        },
        pie_progress_color = {0.98, 0.66, 0.22, 0.5},
    }

data.raw["gui-style"].default["ingteb-un-button"] = {type = "image_style", size = 40}
data.raw["gui-style"].default["ingteb-tab"] = {type = "tab_style", size = 40}

data:extend{{type = "font", name = "ingteb-font18", from = "default", size = 18}}

data:extend{{type = "font", name = "ingteb-font24", from = "default", size = 24}}

data:extend{{type = "font", name = "ingteb-font32", from = "default", size = 32}}

data.raw["gui-style"].default["ingteb-big-tab"] = {type = "tab_style", font = "ingteb-font32"}

data.raw["gui-style"].default["ingteb-medium-tab"] =
    {type = "tab_style", font = "ingteb-font18", size = 40}

data.raw["gui-style"].default["ingteb-big-label"] = {type = "label_style", font = "ingteb-font24"}
