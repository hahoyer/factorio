local UI = {}

function UI.GetMouseCode(event)
    local result --
    = (event.alt and "A" or "-") --
    .. (event.control and "C" or "-") --
    .. (event.shift and "S" or "-") --
    .. " " --
    .. (event.button == defines.mouse_button_type.none and "-") --
          or (event.button == defines.mouse_button_type.left and "l") --
          or (event.button == defines.mouse_button_type.right and "r") --
          or (event.button == defines.mouse_button_type.middle and "m") --
    return result
end

function UI.IsMouseCode(event, pattern)
    if pattern:sub(1, 1) == "A" and not event.alt then return end
    if pattern:sub(1, 1) == "-" and event.alt then return end

    if pattern:sub(2, 2) == "C" and not event.control then return end
    if pattern:sub(2, 2) == "-" and event.control then return end

    if pattern:sub(3, 3) == "S" and not event.shift then return end
    if pattern:sub(3, 3) == "-" and event.shift then return end

    if pattern:sub(5, 5) == "-" and not (event.button == defines.mouse_button_type.none) then
        return
    end
    if pattern:sub(5, 5) == "l" and not (event.button == defines.mouse_button_type.left) then
        return
    end
    if pattern:sub(5, 5) == "r" and not (event.button == defines.mouse_button_type.right) then
        return
    end
    if pattern:sub(5, 5) == "m" and not (event.button == defines.mouse_button_type.middle) then
        return
    end

    return true
end

---@param headText LocalisedString can use \_\_1\_\_ = Alt \_\_2\_\_ = Control \_\_3\_\_ = Shift \_\_2\_\_ = Mouse1 \_\_5\_\_ = Mouse2 to replace texts for buttons or mouse buttons
---@return LocalisedString
function UI.GetHelpTextForButtonsACS12(headText)
    return {
        headText,
        {"control-keys.alt"},
        {"control-keys.control"},
        {"control-keys.shift"},
        {"control-keys.mouse-button-1-alt-1"},
        {"control-keys.mouse-button-2-alt-1"},
    }
end
function UI.GetHelpTextForButtons(helpTag, pattern)
    local result = {""}

    local function append(key)
        table.insert(result, {"control-keys." .. key})
        table.insert(result, " ")
    end

    if pattern:sub(1, 1) == "A" then append("alt") end
    if pattern:sub(2, 2) == "C" then append("control") end
    if pattern:sub(3, 3) == "S" then append("shift") end
    if pattern:sub(5, 5) == "l" then append("mouse-button-1-alt-1") end
    if pattern:sub(5, 5) == "r" then append("mouse-button-2-alt-1") end
    if pattern:sub(5, 5) == "m" then append("mouse-button-3-alt-1") end

    table.insert(result, "- ")
    table.insert(result, {helpTag})
    return {"ingteb-utility.blue", result}
end
return UI
