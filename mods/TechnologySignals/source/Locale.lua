local result = {}

function _M.of_recipe(prototype)
    --- Get the locale of the given recipe.
    return util.resolver {
        main_product = function()
            local product = recipes.get_main_product(prototype)
            return product and _M.of(prototypes.find(product.name, product.type)) or {}
        end,
        name = function(self)
            return prototype.localised_name or self.main_product.name or key_of(prototype, "name", "recipe")
        end,
        description = function(self)
            return prototype.localised_description or key_of(prototype, "description", "recipe")
        end
    }
end

function result.of(prototype, type)
    --- Get the locale of the given prototype.
    if type ~= nil then
        prototype = prototypes.find(prototype, type)
    end
    local locale_type = prototypes.inherits(prototype.type, _M.localised_types)
    assert(locale_type, ("%s doesn't support localization"):format(prototype.type))

    local resolver = custom_resolvers[locale_type] or _M.of_generic
    return resolver(prototype, locale_type)
end

return result
