local function TransformPosition(position)
    if var.Direction == 0 then
        return position.x, position.y
    end
    if var.Direction == 1 then
        return -position.y, position.x
    end
    if var.Direction == 2 then
        return -position.x, -position.y
    end
    if var.Direction == 3 then
        return position.y, -position.x
    end
end

local function TransformSize(size)
    if (var.Direction % 2) == 0 then
        return size.x, size.y
    end
    return size.y, size.x
end

local function RunBluePrint(functionCode, position, size)
    out["construction-robot"] = nil
    out["deconstruction-planner"] = nil
    out["signal-X"] = nil
    out["signal-Y"] = nil
    out["signal-W"] = nil
    out["signal-H"] = nil
    out["signal-R"] = nil

    if not functionCode then
        return
    end

    if position then
        out["signal-X"], out["signal-Y"] = TransformPosition(position)
    end
    if size then
        out["signal-W"], out["signal-H"] = TransformSize(size)
    end

    if functionCode == -1 then
        out["deconstruction-planner"] = -1
    else
        out["signal-R"] = var.Direction
        out["construction-robot"] = functionCode
    end
end

local function CenterCorrection()
    return math.floor(var.Direction / 2)
end

local function GetLongDelay()
    local result = green["signal-D"]
    if not result or result == 0 then
        result = 300
    end
    return result
end

local function LabelIndex(instructions, target)
    for index = 1, #instructions do
        local instruction = instructions[index]
        if instruction.Labels then
            for labelIndex = 1, #instruction.Labels do
                if instruction.Labels[labelIndex] == target then
                    return index
                end
            end
        end
    end
    error("Unknown label " .. target)
end

local function Execute(instructions)
    if not var.State then
        var.State = 1
    end

    local instruction = instructions[var.State]
    local nextLabel = instruction.Instruction()
    if nextLabel then
        var.State = LabelIndex(instructions, nextLabel)
    else
        var.State = var.State + 1
    end
    out["mlc"] = var.State
    if var.State > #instructions then
        error("Unknown instruction " .. var.State)
    end
end

local sizeToRemove = {x = 4, y = 31}

Execute {
    {
        Instruction = function()
            var.Position = 0
            var.Direction = (green["signal-R"] or 0) % 4
        end
    },
    {
        Labels = {"start"},
        Instruction = function()
            RunBluePrint(1, {x = var.Position, y = 0})
        end
    },
    {
        Instruction = function()
            RunBluePrint(nil)
        end
    },
    {
        Labels = {"mining"},
        Instruction = function()
            delay = GetLongDelay()
        end
    },
    {
        Instruction = function()
            if green["signal-0"] == 0 then
                return "mining"
            end
        end
    },
    {
        Instruction = function()
            RunBluePrint(-1, {x = var.Position + CenterCorrection() + sizeToRemove.x / 2, y = 0}, sizeToRemove)
        end
    },
    {
        Instruction = function()
            RunBluePrint(nil)
            var.Position = var.Position + 4
            return "start"
        end
    }
}
