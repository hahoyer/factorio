require("util")

function PatchMapPresets()
    local mapGenPresets = data.raw["map-gen-presets"].default
    for key,value in pairs(mapGenPresets) do
        log("before: "..key .. "=" .. serpent.block(value))
        PatchMapPreset(mapGenPresets, key)
        log("after: "..key .. "=" .. serpent.block(value))
    end
end

function PatchMapPreset(target, key)
    if key == "type" or key == "name" then return end
    if key == "default" then
        key = "ixu_default"
        target[key] = { order = target["default"].order }
        target["default"].order = "~~~~~"
    end
    EnsureTableForKey(target[key], "basic_settings")
    EnsureTableForKey(target[key].basic_settings, "autoplace_controls")
    EnsureTableForKey(target[key].basic_settings.autoplace_controls, "enemy-base")
    EnsureTableForKey(target[key], "advanced_settings")
    EnsureTableForKey(target[key].advanced_settings, "difficulty_settings")

    target[key].basic_settings.autoplace_controls["enemy-base"].size = "none"
    target[key].basic_settings.cliff_settings = { richness = 0 }
    target[key].advanced_settings.difficulty_settings.research_queue_setting = "always"
end

function EnsureTableForKey(target, key)
    if target[key] == nil then target[key] = {}
    end
end
