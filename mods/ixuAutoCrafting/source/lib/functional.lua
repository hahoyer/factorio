function map(l,f)
    local i = 1
    if type(l) == 'function' then
        return function()
            local q = l()
            if q ~= nil then
                local r = f(q, i, l)
                i = i + 1
                return r
            end
        end
    end
    local r = {}
    for i = 1, #l do
        r[i] = f(l[i], i, l)
    end
    return r
end
function filter(l,f)
    if type(l) == 'function' then
        return function()
            local q = l()
            while q ~= nil do
                if f(q) then return q end
                q = l()
            end
        end
    end
    local r = {}
    for i = 1, #l do
        if f(l[i], i, l) then
            table.insert(r, l[i])
        end
    end
    return r
end
function fnn(l)
    return filter(l, notNil)
end
function notNil(q)
    return q ~= nil
end
function range(from, to, step)
    step = step or 1
    return function()
        local r = from
        from = from + step
        if r <= to then return r end
    end
end
-- Gives an iterator over items in list l which are first filtered by function f and then mapped over by function m.
-- f(c,i,a) and m(c,i,a) are given 3 parameters: (current_element, index_of_element, array)
function list_iter_filt_map(l,f,m)
    if m == nil then
        m = function(q) return q end
    end
    local i = 0
    local n = #l
    return function()
        while i < n do
            i = i + 1
            if f == nil or f(l[i], i, l) then
                return m(l[i], i, l)
            end
        end
    end
end

-- Flattens an iterator over iterators (or list of iterators) over elements to an iterator over elements.
-- Basically it turns {{1,2},{3,4,5},{},{6},{7,8}} into {1,2,3,4,5,6,7,8} but for iterators instead of lists.
function iterlist_iter(il)
    local i = 1
    if type(il) == 'function' then
        local it = il()
        return function()
            while it ~= nil do
                local it_val = it()
                if it_val ~= nil then return it_val end
                it = il()
            end
        end
    end
    return function()
        while i <= #il do
            local el = il[i]()
            if el ~= nil then
                return el
            end
            i = i + 1
        end
    end
end

return {map = map, filter = filter, fnn = fnn, notNil = notNil, range = range, list_iter_filt_map = list_iter_filt_map, iterlist_iter = iterlist_iter}