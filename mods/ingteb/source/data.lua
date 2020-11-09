local Constants = require("Constants")

data:extend(
  {
    {
      type = "custom-input",
      name = "ingteb-main-key",
      key_sequence = "CONTROL + I",
      action = "lua"
    },
    {
      type = "custom-input",
      name = Constants.ModName .. "-back-key",
      key_sequence = "BACKSPACE",
      consuming = "none"
    }
  }
)
