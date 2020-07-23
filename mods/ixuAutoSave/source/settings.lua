local Constants = require('Constants')
data:extend({
  {
    name = Constants.GlobalPrefix,
    type = "string-setting",
    default_value = "",
    setting_type = "runtime-global",
    order = "0100",
    allow_blank = true
  },
  {
    name = Constants.Frequency,
    type = "string-setting",
    default_value = "1:00:00",
    setting_type = "runtime-global",
    order = "0300",
    allow_blank = true
  },
  {
    name = Constants.EnterPrefixOnInit,
    type = "bool-setting",
    setting_type = "runtime-global",
    default_value = true,
    order = "0200"
  }
})
