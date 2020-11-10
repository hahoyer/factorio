local Constants = require("Constants")

data:extend(
  {
    {
      type = "custom-input",
      name = Constants.Key.Main,
      key_sequence = "CONTROL + I",
      action = "lua"
    },
    {
      type = "custom-input",
      name = Constants.Key.Back,
      key_sequence = "BACKSPACE",
      consuming = "none"
    }
  }
)
