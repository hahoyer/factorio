require("util")

function PatchMapPresets()
    if settings.startup.ixuMatchMapPreset_ForceNoEnemies.value 
    or settings.startup.ixuMatchMapPreset_ForceNoCliffs.value 
    or settings.startup.ixuMatchMapPreset_ForceResearchQueue.value 
    then
        local mapGenPresets = data.raw["map-gen-presets"].default
        for key,value in pairs(mapGenPresets) do
            PatchMapPreset(mapGenPresets, key)
        end
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

    if settings.startup.ixuMatchMapPreset_ForceNoEnemies.value then
    target[key].basic_settings.autoplace_controls["enemy-base"].size = "none"
    end
    if settings.startup.ixuMatchMapPreset_ForceNoCliffs.value then
        target[key].basic_settings.cliff_settings = { richness = 0 }
    end
    if settings.startup.ixuMatchMapPreset_ForceResearchQueue.value then
        target[key].advanced_settings.difficulty_settings.research_queue_setting = "always"
    end
end

function EnsureTableForKey(target, key)
    if target[key] == nil then target[key] = {}
    end
end
