local Constants = require("Constants")

data:extend(
  {
    {
      type = "custom-input",
      name = Constants.Key.Main,
      key_sequence = "H"
    },
    {
      type = "custom-input",
      name = Constants.Key.Back,
      key_sequence = "mouse-button-4"
    },
    {
      type = "custom-input",
      name = Constants.Key.Fore,
      key_sequence = "mouse-button-5"
    }
  }
)
