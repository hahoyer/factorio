local silo

local function DestroySilo()
    if silo.valid then
        silo.damage(2000, "neutral")
    else
        script.on_nth_tick(60, nil)
    end
end

local function on_rocket_launched(rocket)
    silo = rocket.rocket_silo
    script.on_event(defines.events.on_rocket_launched, nil)
    script.on_nth_tick(60, DestroySilo)
end

local function Launch()
    local surface = game.surfaces[1]
    local silo = surface.create_entity {name = "quick-rocket-silo", position = {x = 0, y = 0}, force = "player"}
    silo.auto_launch = true
    silo.energy = 10
    silo.rocket_parts = 100
    surface.create_entity {name = "small-electric-pole", position = {x = -6, y = 0}, force = "player"}
    surface.create_entity {name = "solar-panel", position = {x = -8, y = 0}, force = "player"}
    surface.create_entity {
        name = "inserter",
        position = {x = -5, y = 1},
        force = "player",
        direction = defines.direction.west
    }
    surface.spill_item_stack({x = -5, y = 1}, "satellite")
    script.on_event(defines.events.on_rocket_launched, on_rocket_launched)
    script.on_init(nil)
end

script.on_init(Launch)
