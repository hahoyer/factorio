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

data.raw["gui-style"].default[Constants.GuiStyle.CenteredFlow] = {
  type = "horizontal_flow_style",
  horizontally_stretchable = "on",
  right_padding = "0",
  left_padding = "0",
  top_padding = "0",
  bottom_padding = "0",
  horizontal_align = "center",
  color = {r = 0.9, b = 0, g = 0, a = 0}
}
