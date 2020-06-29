function any(array, value_in_array) --checks if any item in an array is true
    for _, v in pairs(array) do
        if v[value_in_array] then return true end
    end
    return false
end

function all(array, value_in_array) --checks if all items in an array are true
    for k, v in pairs(array) do
        if not v[value_in_array] then return false end
    end
    return true
end

-- check if any item[property_name] in array is mapped to a truthy value in map
function matches(array, property_name, map)
    for _, element in pairs(array) do
        if map[element[property_name]] then return true end
    end
    return false
end