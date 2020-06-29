require("util")

local silo = table.deepcopy(data.raw["rocket-silo"]["rocket-silo"])

silo.name = "quick-rocket-silo"
silo.localized_name = "rocket-silo"
silo.energy_usage = "0.01kW"
silo.idle_energy_usage = "0.01KW"
silo.lamp_energy_usage = "0.01KW"
silo.active_energy_usage = "0.01KW"
silo.minable = nil

data:extend({silo})
