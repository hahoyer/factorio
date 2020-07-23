local big_size = 64
local small_size = 32
local tiny_size = 24
local ModName = "research-queue-0-18-34"


data.raw["gui-style"].default["rqon-tool-selected-filter"] =
{
    type = "button_style",
    parent = "image_tab_selected_slot",
    width = 36,
    height = 36
}

data.raw["gui-style"].default["rqon-tool-inactive-filter"] =
{
    type = "button_style",
    parent = "image_tab_slot",
    width = 36,
    height = 36
}

data.raw["gui-style"].default["rqon-ingredient-sprite"] =
{
    type = "button_style",
    parent = "slot_button",
    size = tiny_size,
    scalable = true,
    align = "left",
}

data.raw["gui-style"].default["rqon-top-button"] =
{
    type = "button_style",
    parent = "button",
    width = small_size,
    height = small_size,
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0
}

data:extend(
{
    {
        type = "sprite",
        name = "rqon-text-view-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 0,
        y = 64
    },
    {
        type = "sprite",
        name = "rqon-completed-research-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 0,
        y = 96
    },
    {
        type = "sprite",
        name = "rqon-native-queue-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 0,
        y = 96,
        tint = {r = 0.5, g = 1, b = 0.4, a = 1}
    },
    {
        type = "sprite",
        name = "rqon-extend-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 0,
        y = 160
    },
    {
        type = "sprite",
        name = "rqon-compact-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 0,
        y = 128
    },
    {
        type = "sprite",
        name = "rqon-up-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 16,
        x = 32,
        y = 0
    },
    {
        type = "sprite",
        name = "rqon-down-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 16,
        x = 32,
        y = 16
    },
    {
        type = "sprite",
        name = "rqon-cancel-icon",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 32,
        y = 32
    }
})

data.raw["gui-style"].default["rqon-done-frame"] =
{
    type = "frame_style",
    width = big_size + 4,
    height = big_size + 4,
    scalable = false,
    align = "center",
    graphical_set =
    {
        border = 0,
        filename = "__core__/graphics/gui.png",
        width = 36,
        height = 36,
        x = 111,
        y = 108
    },
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    flow_style =
    {
        type = "flow_style",
        horizontal_spacing = 0,
        vertical_spacing = 0
    }
}

data.raw["gui-style"].default["rqon-inq-frame"] =
{
    type = "frame_style",
    width = big_size + 4,
    height = big_size + 4,
    scalable = false,
    align = "center",
    graphical_set =
    {
        border = 2,
        filename = "__core__/graphics/gui.png",
        width = 36,
        height = 36,
        x = 111,
        y = 72
    },
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    flow_style =
    {
        type = "flow_style",
        horizontal_spacing = 0,
        vertical_spacing = 0
    }
}

data.raw["gui-style"].default["rqon-available-frame"] =
{
    type = "frame_style",
    width = big_size + 4,
    height = big_size + 4,
    scalable = false,
    align = "center",
    graphical_set =
    {
        border = 2,
        filename = "__core__/graphics/gui.png",
        width = 36,
        height = 36,
        x = 75,
        y = 72
    },
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    flow_style =
    {
        type = "flow_style",
        horizontal_spacing = 0,
        vertical_spacing = 0
    }
}

data.raw["gui-style"].default["rqon-button"] =
{
    type = "button_style",
    font = "default",
    default_font_color = {r = 1, g = 1, b = 1},
    align = "center",
    scalable = false,
    left_click_sound =
    {
        {
            filename = "__core__/sound/gui-click.ogg",
            volume = 1
        }
    },
    width = big_size,
    height = big_size,
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    default_graphical_set =
    {
        border = 2,
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 96,
        y = 96
    },
    hovered_font_color = {r = 1, g = 1, b = 1},
    hovered_graphical_set =
    {
        border = 2,
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 0,
        y = 0
    },
    clicked_font_color = {r = 1, g = 1, b = 1},
    clicked_graphical_set =
    {
        border = 2,
        filename = "__core__/graphics/gui.png",
        width = 36,
        height = 36,
        x = 148,
        y = 0
    },
}

data.raw["gui-style"].default["rqon-dummy-button"] =
{
    type = "button_style",
    font = "default",
    default_font_color = {r = 1, g = 1, b = 1},
    align = "center",
    width = big_size,
    height = big_size,
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    default_graphical_set =
    {
        border = 2,
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 96,
        y = 96
    },
    hovered_font_color = {r = 1, g = 1, b = 1},
    hovered_graphical_set =
    {
        border = 2,
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        width = 32,
        height = 32,
        x = 96,
        y = 96
    },
    clicked_font_color = {r = 1, g = 1, b = 1},
    clicked_graphical_set =
    {
        border = 2,
        filename = "__core__/graphics/gui.png",
        width = 36,
        height = 36,
        x = 96,
        y = 96
    },
}

data.raw["gui-style"].default["rqon-small-dummy-button"] =
{
    type = "button_style",
    parent = "rqon-dummy-button",
    width = small_size,
    height = small_size
}

data:extend(
{
    {
        type = "font",
        name = "rqon-label-text",
        from = "default-bold",
        size = 14,
        border = true,
        border_color = {}
    },
    {
        type = "font",
        name = "rqon-small-label-text",
        from = "default-bold",
        size = 12,
        border = true,
        border_color = {}
    }
})

data.raw["gui-style"].default["rqon-label"] =
{
    type = "label_style",
    parent = "label",
    font = "rqon-label-text",
    scalable = true,
    width = big_size,
    height = big_size
}

data.raw["gui-style"].default["rqon-small-label"] =
{
    type = "label_style",
    parent = "rqon-label",
    font = "rqon-small-label-text",
    width = tiny_size,
    height = tiny_size
}

data.raw["gui-style"].default["rqon-flow"] =
{
    type = "horizontal_flow_style",
    horizontal_spacing = 0,
    vertical_spacing = 0,
    max_on_row = 0,
    resize_row_to_width = true,
    resize_to_row_height = true
}

data.raw["gui-style"].default["rqon-flow-vertical"] =
{
    type = "vertical_flow_style",
    horizontal_spacing = 0,
    vertical_spacing = 0,
    max_on_row = 0,
    resize_row_to_width = true,
    resize_to_row_height = true
}

data.raw["gui-style"].default["rqon-frame"] =
{
    type = "frame_style",
    font = "heading-2",
    font_color = {r = 1, g = 1, b = 1},
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    graphical_set =
    {
        type = "composition",
        filename = "__core__/graphics/gui.png",
        corner_size = {3, 3},
        position = {8, 0}
    },
    flow_style =
    {
        type = "flow_style",
        horizontal_spacing = 1,
        vertical_spacing = 1
    }
}

data.raw["gui-style"].default["rqon-up-button"] =
{
    type = "button_style",
    font = "default",
    align = "center",
    scalable = false,
    left_click_sound =
    {
        {
            filename = "__core__/sound/gui-click.ogg",
            volume = 1
        }
    },
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    width = small_size,
    height = small_size / 2
}

data.raw["gui-style"].default["rqon-down-button"] =
{
    type = "button_style",
    font = "default",
    align = "center",
    scalable = false,
    left_click_sound =
    {
        {
            filename = "__core__/sound/gui-click.ogg",
            volume = 1
        }
    },
    top_padding = 1,
    right_padding = 1,
    bottom_padding = 1,
    left_padding = 1,
    width = small_size,
    height = small_size / 2
}

data.raw["gui-style"].default["rqon-cancel-button"] =
{
    type = "button_style",
    font = "default",
    align = "center",
    scalable = false,
    left_click_sound =
    {
        {
            filename = "__core__/sound/gui-click.ogg",
            volume = 1
        }
    },
    top_padding = 1,
    right_padding = 1,
    bottom_padding = 1,
    left_padding = 1,
    width = small_size,
    height = small_size
}

data.raw["gui-style"].default["rqon-done-button"] =
{
    type = "button_style",
    font = "default-semibold",
    default_font_color = {r = 1, g = 1, b = 1},
    align = "center",
    left_click_sound =
    {
        {
            filename = "__core__/sound/gui-click.ogg",
            volume = 1
        }
    },
    horizontally_stretchable = "on",
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    default_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {0, 32}
    },
    hovered_font_color = {r = 1, g = 1, b = 1},
    hovered_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {8, 32}
    },
    clicked_font_color = {r = 1, g = 1, b = 1},
    clicked_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {16, 32}
    }
}

data.raw["gui-style"].default["rqon-inq-button"] =
{
    type = "button_style",
    font = "default-semibold",
    default_font_color = {r = 1, g = 1, b = 1},
    align = "center",
    left_click_sound =
    {
        {
            filename = "__core__/sound/gui-click.ogg",
            volume = 1
        }
    },
    horizontally_stretchable = "on",
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    default_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {0, 40}
    },
    hovered_font_color = {r = 1, g = 1, b = 1},
    hovered_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {8, 40}
    },
    clicked_font_color = {r = 1, g = 1, b = 1},
    clicked_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {16, 40}
    }
}

data.raw["gui-style"].default["rqon-available-button"] =
{
    type = "button_style",
    font = "default-semibold",
    default_font_color = {r = 1, g = 1, b = 1},
    align = "center",
    left_click_sound =
    {
        {
            filename = "__core__/sound/gui-click.ogg",
            volume = 1
        }
    },
    horizontally_stretchable = "on",
    top_padding = 0,
    right_padding = 0,
    bottom_padding = 0,
    left_padding = 0,
    default_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {0, 48}
    },
    hovered_font_color = {r = 1, g = 1, b = 1},
    hovered_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {8, 48}
    },
    clicked_font_color = {r = 1, g = 1, b = 1},
    clicked_graphical_set =
    {
        type = "composition",
        filename = "__" .. ModName .. "__/graphics/gui_elements.png",
        corner_size = {3, 3},
        position = {16, 48}
    }
}

data.raw["gui-style"].default["rqon-table1"] =
{
    type = "table_style",
    horizontal_spacing = 2,
    vertical_spacing = 2
}

data.raw["gui-style"].default["rqon-table2"] =
{
    type = "table_style",
    horizontal_spacing = 6,
    vertical_spacing = 6
}
