local Result = {}

function Result.EnsureLayerdIcons(value)
    if not value.icons then
        value.icons = {}
    end
    if value.icon then
        table.insert(value.icons, {icon = value.icon, icon_size = value.icon_size, icon_mipmaps = value.icon_mipmaps})
        value.icon = nil
        value.icon_size = nil
        value.icon_midmaps = nil
    end
end

function Result.GetLayerdIcons(value)
    local result = value.icons or {}
    if value.icon then
        table.insert(result, {icon = value.icon, icon_size = value.icon_size, icon_mipmaps = value.icon_mipmaps})
    end
    return result
end

local function ScaleSingleIcon(target, value)
    local scale = target.scale or 1
    scale = (target.scale or 1) * value
    if scale == 1 then
        scale = nil
    end

    local shift = target.shift or {0, 0}
    shift = {shift[1] * value, shift[2] * value}
    if shift[1] == 0 and shift[2] == 0 then
        shift = nil
    end

    return {
        icon = target.icon,
        icon_size = target.icon_size,
        icon_mipmaps = target.icon_mipmaps,
        tint = target.tint,
        scale = scale,
        shift = shift
    }
end

local function ShiftSingleIcon(target, value)
    local shift = target.shift or {0, 0}
    shift = {
        shift[1] + value[1] * target.icon_size,
        shift[2] + value[2] * target.icon_size
    }
    if shift[1] == 0 and shift[2] == 0 then
        shift = nil
    end
    return {
        icon = target.icon,
        icon_size = target.icon_size,
        icon_mipmaps = target.icon_mipmaps,
        tint = target.tint,
        scale = target.scale,
        shift = shift
    }
end

function Result.TransformIcon(target, scale, shift)
    local result = {}
    for _, value in ipairs(target) do
        value = ScaleSingleIcon(value, scale)
        value = ShiftSingleIcon(value, shift)
        table.insert(result, value)
    end
    return result
end

function Result.CombineIcons(targets)
    local result = {}
    for _, target in ipairs(targets) do
        for _, icon in ipairs(target) do
            table.insert(result, icon)
        end
    end
    return result
end

return Result