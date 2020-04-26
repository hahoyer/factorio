local colors = {
  "off",
  "white",
  "blue",
  "red",
  "green",
  "yellow"
}

data:extend{
  {
    type = "int-setting",
    name = "bottleneck-signals-per-tick",
    setting_type = "runtime-global",
    default_value = 40,
    maximum_value = 2000,
    minimum_value = 1,
    order = "bottleneck-01",
  },
  {
    type = "string-setting",
    name = "bottleneck-color-for-ok",
    setting_type = "runtime-global",
    default_value = "off",
    order = "bottleneck-02",
    allowed_values = colors,
  },
  {
    type = "string-setting",
    name = "bottleneck-color-for-empty",
    setting_type = "runtime-global",
    default_value = "red",
    order = "bottleneck-03",
    allowed_values = colors,
  },
  {
    type = "string-setting",
    name = "bottleneck-color-for-full",
    setting_type = "runtime-global",
    default_value = "blue",
    order = "bottleneck-04",
    allowed_values = colors
  },
  {
    type = "string-setting",
    name = "bottleneck-color-for-disabled",
    setting_type = "runtime-global",
    default_value = "white",
    order = "bottleneck-05",
    allowed_values = colors
  },
}


