local icons = {
  "none",
  "none small",
  "alert",
  "alert small",
  "cross",
  "cross small",
  "minus",
  "minus small",
  "pause",
  "pause small",
  "stop",
  "stop small",
  "alert3D",
  "alert3D small",
  "cross3D",
  "cross3D small",
  "minus3D",
  "minus3D small",
  "pause3D",
  "pause3D small",
  "stop3D",
  "stop3D small"
}

local result = {}

function result.get(status_name)

local settings_values = {
  ["working"] = {
    color = settings.global["bottleneck-color-for-ok"].value,
    icon = "none"
  },
  ["no_power"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["no_fuel"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["no_input_fluid"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["no_research_in_progress"] = {
    color = settings.global["bottleneck-color-for-disabled"].value,
    icon = "pause small"
  },
  ["no_minable_resources"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "stop"
  },
  ["low_input_fluid"] = {
    color = settings.global["bottleneck-color-for-full"].value,
    icon = "alert"
  },
  ["low_power"] = {
    color = settings.global["bottleneck-color-for-full"].value,
    icon = "alert"
  },
  ["disabled_by_control_behavior"] = {
    color = settings.global["bottleneck-color-for-disabled"].value,
    icon = "stop small"
  },
  ["disabled_by_script"] = {
    color = settings.global["bottleneck-color-for-disabled"].value,
    icon = "stop small"
  },
  ["fluid_ingredient_shortage"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["full_output"] = {
    color = settings.global["bottleneck-color-for-full"].value,
    icon = "pause"
  },
  ["item_ingredient_shortage"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["item_production_overload"] = {
    color = settings.global["bottleneck-color-for-full"].value,
    icon = "pause"
  },
  ["missing_required_fluid"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["missing_science_packs"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["waiting_for_source_items"] = {
    color = settings.global["bottleneck-color-for-empty"].value,
    icon = "alert"
  },
  ["waiting_for_space_in_destination"] = {
    color = settings.global["bottleneck-color-for-full"].value,
    icon = "pause"
  },
}
return settings_values[status_name] or {color = "off", icon = "none"}

end 

return result
