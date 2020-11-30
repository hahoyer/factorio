local Constants = require("Constants")

data:extend {
	{
		type = "int-setting",
		name = "ingteb_column-tab-threshold",
		setting_type = "runtime-per-user",
		default_value = 3,
		minimum_value = 0,
		order = "a"
	},
	{
		type = "int-setting",
		name = "ingteb_group-tab-threshold",
		setting_type = "runtime-per-user",
		default_value = 10,
		minimum_value = 0,
		order = "b"
	},
	{
		type = "int-setting",
		name = "ingteb_subgroup-tab-threshold",
		setting_type = "runtime-per-user",
		default_value = 10,
		minimum_value = 0,
		order = "c"
	},
}
