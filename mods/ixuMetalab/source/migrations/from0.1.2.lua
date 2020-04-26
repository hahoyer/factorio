local Constants = require('constants')

local function fixLabsGlobal()
    global.Labs = game.surfaces[1].find_entities_filtered{name="lab"}
end

local function fixMonitorsGlobal()
    if global.monitor then 
        global.Monitors = global.monitor  
        global.monitor = nil
    end
    if not global.Monitors then global.Monitors = {} end
    local targets = game.surfaces[1].find_entities_filtered{name="metalab-monitor"}
    for _, entity in ipairs(targets) do
        local monitor = global.Monitors[entity.unit_number]
        if not monitor then 
            monitor = 
            {
                LabsSignal= {type=Constants.LabsSignal.type, name=Constants.LabsSignal.name},
                InactiveLabsSignal = {type=Constants.InactiveLabsSignal.type,name=Constants.InactiveLabsSignal.name}
            } 
        end
        monitor.Entity = entity
        global.Monitors[entity.unit_number] = monitor
    end
end

fixLabsGlobal()
fixMonitorsGlobal()

local MOD_NAME = Constants.ModName

game.players[1].print{Constants.ModName.."-message.migration-successful", "0.1.2"}