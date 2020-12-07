local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local UI = require("core.UI")
local class = require("core.class")

local DynamicElements = Dictionary:new()

local function CreateContentPanel(frame, headerSprites)
    local subFrame = frame.add {
        type = "frame",
        horizontal_scroll_policy = "never",
        direction = "vertical",
    }

    local headerFlow = subFrame.add {
        type = "flow",
        name = "headerFlow",
        direction = "horizontal",
        style = "ingteb-flow-centered",
    }

    headerFlow.add {
        type = "label",
        name = "headerSprites",
        caption = headerSprites,
        style = "ingteb-big-label",
    }

    subFrame.add {type = "line", direction = "horizontal"}

    return subFrame
end

local function CreateSprite(frame, target, sprite)
    local style = Helper.SpriteStyleFromCode(target and target.SpriteStyle)

    if not target then return frame.add {type = "sprite-button", sprite = sprite, style = style} end

    local tooltip = target.HelperText
    local sprite = target.SpriteName
    local number = target.NumberOnSprite
    local show_percent_for_small_numbers = target.UsePercentage

    if sprite == "fuel-category/chemical" then sprite = "chemical" end
    return frame.add {
        type = "sprite-button",
        tooltip = tooltip,
        sprite = sprite,
        number = number,
        show_percent_for_small_numbers = show_percent_for_small_numbers,
        style = style,
    }
end

local function RegisterTargetForGuiClick(result, target)
    global.Links[result.index] = target and target.CommonKey
    if target and (target.IsDynamic or target.HasLocalisedDescriptionPending) then
        DynamicElements:AppendForKey(target, result)
    end
    return result
end

local function CreateSpriteAndRegister(frame, target, sprite)
    local result = CreateSprite(frame, target, sprite)
    if target then RegisterTargetForGuiClick(result, target) end
    return result
end

local maximalCount = 6

local function DummyTiles(frame, count)
    for _ = 1, count do --
        frame.add {type = "sprite", style = "ingteb-un-button"}
    end
end

local function CreateLinePart(frame, target, count, isLeftSide)
    local scrollFrame = frame
    if target:Count() > count then
        scrollFrame = frame.add {
            type = "scroll-pane",
            direction = "horizontal",
            vertical_scroll_policy = "never",
            style = "ingteb-scroll-6x1",
        }
    end

    local subPanel = scrollFrame.add {
        type = "flow",
        direction = "horizontal",
        style = isLeftSide and "ingteb-flow-right" or nil,
    }

    target:Select(function(element) return CreateSpriteAndRegister(subPanel, element) end)

    if isLeftSide then return end

    DummyTiles(subPanel, count - target:Count())
end

local function CreateRecipeLine(frame, target, inCount, outCount)
    local subFrame = frame.add {type = "flow", direction = "horizontal"}

    CreateLinePart(subFrame, target.Input, inCount, true)

    local properties = subFrame.add {type = "flow", direction = "horizontal"}
    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}
    CreateSpriteAndRegister(
        properties, target.Technology
            or {SpriteName = "factorio", HelperText = {"ingteb-utility.initial-technology"}}
    )
    CreateSpriteAndRegister(properties, target)
    CreateSpriteAndRegister(properties, {SpriteName = "utility/clock", NumberOnSprite = target.Time})
    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}

    CreateLinePart(subFrame, target.Output, outCount, false)
end

local function CreateEffectLine(frame, target)
    CreateSpriteAndRegister(frame, target)
end

local function CreateCraftingGroupPanel(frame, target, category, inCount, outCount)
    assert(release or type(category) == "string")
    inCount = math.min(inCount, maximalCount)
    outCount = math.min(outCount, maximalCount)

    local workers = target[1].Database:GetCategory(category).Workers

    local columnCount = inCount + outCount + 3
    local workersCount = workers:Count()
    local lines = math.ceil(workersCount / columnCount)
    local potentialWorkerCount = lines * columnCount
    local dummiesRequired = potentialWorkerCount - workersCount
    local dummyColumnsLeft = math.ceil((dummiesRequired) / 2)

    local workersPanel = frame.add {
        type = "table",
        column_count = columnCount + 1,
        direction = "horizontal",
    }

    local position = 0
    workers:Select(
        function(worker)
            if position == 0 then
                workersPanel.add {type = "sprite", sprite = "utility/change_recipe"}
            end
            if lines == 1 and position == 0 then
                DummyTiles(workersPanel, dummyColumnsLeft)
                position = position + dummyColumnsLeft
            end
            CreateSpriteAndRegister(workersPanel, worker)
            position = position + 1
            if position >= columnCount then
                position = 0
                lines = lines - 1
            end
        end
    )

    frame.add {type = "line", direction = "horizontal"}

    if target:Count() < settings.player["ingteb_group-tab-threshold"].value then
        target:Select(function(recipe) CreateRecipeLine(frame, recipe, inCount, outCount) end)
    else
        local groups = target:ToGroup(
            function(value) return {Key = value.Group.name, Value = value} end
        ):ToArray()
        local groupPanel = groups:Count() > 1 and frame.add {type = "tabbed-pane"} or frame
        groups:Select(
            function(value)
                local group = value[1].Group
                local frame = groupPanel.add {type = "flow", direction = "vertical"}

                if groups:Count() > 1 then
                    local tab = groupPanel.add {
                        type = "tab",
                        caption = "[item-group=" .. group.name .. "]",
                        tooltip = group.localised_name,
                        style = "ingteb-medium-tab",
                    }
                    groupPanel.add_tab(tab, frame)
                end

                if value:Count() < settings.player["ingteb_subgroup-tab-threshold"].value then
                    value:Select(
                        function(recipe)
                            CreateRecipeLine(frame, recipe, inCount, outCount)
                        end
                    )
                else
                    local subGroups = value:ToGroup(
                        function(value)
                            return {Key = value.SubGroup.name, Value = value}
                        end
                    ):ToArray() --
                    local groupPanel = subGroups:Count() > 1 and frame.add {type = "tabbed-pane"}
                                           or frame
                    subGroups:Select(
                        function(value)
                            local group = value[1].SubGroup
                            local caption = group.name
                            if value[1] and value[1].Output[1] then
                                local main = value[1].Output[1]
                                caption = main.RichTextName
                            end
                            local subFrame = groupPanel.add {type = "flow", direction = "vertical"}

                            if subGroups:Count() > 1 then
                                local tab = groupPanel.add {
                                    type = "tab",
                                    caption = caption,
                                    tooltip = group.localised_name,
                                    style = "ingteb-medium-tab",
                                }
                                groupPanel.add_tab(tab, subFrame)
                            end

                            value:Select(
                                function(recipe)
                                    CreateRecipeLine(subFrame, recipe, inCount, outCount)
                                end
                            )
                        end
                    )
                end
            end
        )

    end

    frame.add {type = "line", direction = "horizontal"}
end

local function CreateCraftingGroupsPanel(frame, target, headerSprites)
    if not target or not target:Any() then return end
    assert(release or type(target:Top().Key) == "string")
    assert(release or target:Top().Value[1].object_name == "Recipe")

    local subFrame = CreateContentPanel(frame, headerSprites)

    local inCount = target:Select(
        function(group)
            return group:Select(function(recipe) return recipe.Input:Count() end):Max()
        end
    ):Max()

    local outCount = target:Select(
        function(group)
            return group:Select(function(recipe) return recipe.Output:Count() end):Max()
        end
    ):Max()

    target:Select(
        function(recipes, category)
            assert(release or type(category) == "string")
            CreateCraftingGroupPanel(subFrame, recipes, category, inCount, outCount)
        end
    )
end

local function CreateTechnologyPanel(frame, target, prerequisitesCount, enablesCount)
    assert(release or target.object_name == "Technology")

    local frame = frame.add {type = "flow", direction = "horizontal"}
    CreateLinePart(frame, target.Prerequisites, prerequisitesCount, true)
    frame.add {type = "sprite", sprite = "utility/go_to_arrow"}
    CreateSpriteAndRegister(frame, target)
    frame.add {type = "sprite", sprite = "utility/go_to_arrow"}
    CreateLinePart(frame, target.Enables, enablesCount, false)
end

local function CreateTechnologyEffectsPanel(frame, target, headerSprites)
    if not target or not target:Any() then return end
    assert(release or target[1].object_name == "Recipe" or target[1].object_name == "Bonus")

    local frame = CreateContentPanel(frame, headerSprites)

    local inCount = target:Select(function(recipe) return recipe.Input:Count() end):Max()

    local outCount = target:Select(function(recipe) return recipe.Output:Count() end):Max()

    target:Select(
        function(effekt)
            if effekt.object_name == "Recipe" then
                CreateRecipeLine(frame, effekt, inCount, outCount)
            else
                CreateEffectLine(frame, effekt)
            end
        end
    )
end

local function CreateTechnologiesPanel(frame, target, headerSprites)
    if not target or not target:Any() then return end
    assert(release or target:Top().object_name == "Technology")

    local subFrame = CreateContentPanel(frame, headerSprites)

    local prerequisitesCount = target:Select(
        function(technology) return technology.Prerequisites:Count() end
    ):Max()

    local enablesCount = target:Select(function(technology) return technology.Enables:Count() end)
        :Max()

    target:Select(
        function(technology)
            assert(release or technology.object_name == "Technology")
            CreateTechnologyPanel(subFrame, technology, prerequisitesCount, enablesCount)
        end
    )
end

local function CheckedTabifyColumns(frame, mainFrame, target, columnCount)
    local maximalColumCount = settings.player["ingteb_column-tab-threshold"].value
    if maximalColumCount == 0 then maximalColumCount = columnCount end

    if columnCount > maximalColumCount then
        local tabOrder = target.TabOrder
        if not tabOrder then
            tabOrder = Array:FromNumber(columnCount)
            target.TabOrder = tabOrder
        end

        tabOrder:Select(
            function(tabIndex, order)
                if order > maximalColumCount then
                    frame.caption --
                    = {
                        "",
                        frame.caption, --
                        " >>> [" .. mainFrame.children[tabIndex].headerFlow.headerSprites.caption
                            .. "]",
                    }
                    mainFrame.children[tabIndex].visible = false
                else
                    mainFrame.children[tabIndex].headerFlow.add {
                        type = "sprite-button",
                        sprite = "hide-this-column",
                        name = order,
                    }
                end
            end
        )
        global.Links[frame.index] = target.CommonKey

    end

end

local function UpdateGui(list, target, dataBase)
    target = dataBase:GetProxy(target.object_name, target.Name)
    local helperText = target.HelperText
    local number = target.NumberOnSprite
    local style = Helper.SpriteStyleFromCode(target.SpriteStyle)

    for _, guiElement in pairs(list) do
        if guiElement.valid then
            guiElement.tooltip = helperText
            guiElement.number = number
            guiElement.style = style
        end
    end
end

local function CreateRecipePanel(frame, target)
    if not target or not target.RecipeData then return end
    assert(release or target.object_name == "Recipe")

    local subFrame = CreateContentPanel(frame, target.RichTextName)

    CreateCraftingGroupPanel(
        subFrame, Array:new{target}, target.Category.Name, target.Input:Count(),
            target.Output:Count()
    )
end

local Presentator = {}

function Presentator:Close()
    DynamicElements = Dictionary:new() --
end

function Presentator:RefreshMainInventoryChanged(dataBase)
    DynamicElements --
    :Where(function(_, target) return target.object_name == "Recipe" end) --
    :Select(function(list, target) UpdateGui(list, target, dataBase) end) --
end

function Presentator:RefreshStackChanged(dataBase) end

function Presentator:RefreshResearchChanged(dataBase)
    DynamicElements --
    :Where(function(_, target) return target.object_name == "Technology" end) --
    :Select(function(list, target) UpdateGui(list, target, dataBase) end) --
end

function Presentator:new(frame, target)
    DynamicElements = Dictionary:new() --
    global.Links = {}
    frame.caption = target.LocalisedName

    local scrollframe = frame.add {
        type = "scroll-pane",
        horizontal_scroll_policy = "never",
        direction = "vertical",
        name = "frame",
    }

    target:SortAll()
    assert(
        release or not target.RecipeList or not next(target.RecipeList)
            or type(next(target.RecipeList)) == "string"
    )
    assert(
        release or not target.UsedBy or not next(target.UsedBy) or type(next(target.UsedBy))
            == "string"
    )
    assert(

       
            release or not target.CreatedBy or not next(target.CreatedBy)
                or type(next(target.CreatedBy)) == "string"
    )

    local mainFrame = scrollframe
    local columnCount --
    = (target.RecipeList and target.RecipeList:Any() and 1 or 0) --
    + (target.RecipeData and 1 or 0) --
          + (target.Prerequisites and target.Prerequisites:Any() and 1 or 0) --
          + (target.Effects and target.Effects:Any() and 1 or 0) --
          + (target.Enables and target.Enables:Any() and 1 or 0) --
          + (target.UsedBy and target.UsedBy:Any() and 1 or 0) --
          + (target.CreatedBy and target.CreatedBy:Any() and 1 or 0) --

    if columnCount > 1 then
        mainFrame = scrollframe.add {type = "frame", direction = "horizontal", name = "frame"}
    end

    if columnCount == 0 then
        local none = mainFrame.add {type = "frame", direction = "horizontal"}
        none.add {
            type = "label",
            caption = "[img=utility/crafting_machine_recipe_not_unlocked][img=utility/go_to_arrow]",
        }

        CreateSpriteAndRegister(none, target)

        none.add {
            type = "label",
            caption = "[img=utility/go_to_arrow][img=utility/crafting_machine_recipe_not_unlocked]",
        }

        return
    end

    CreateTechnologiesPanel(
        mainFrame, target.Prerequisites,
            "[img=utility/missing_icon][img=utility/go_to_arrow]" .. target.RichTextName
    )

    CreateTechnologyEffectsPanel(
        mainFrame, target.Effects, target.RichTextName .. "[img=utility/change_recipe]"
    )

    CreateTechnologiesPanel(
        mainFrame, target.Enables,
            target.RichTextName .. "[img=utility/go_to_arrow][img=utility/missing_icon]"
    )

    CreateCraftingGroupsPanel(
        mainFrame, target.RecipeList, target.RichTextName .. "[img=utility/change_recipe]"
    )

    CreateCraftingGroupsPanel(
        mainFrame, target.UsedBy,
            target.RichTextName .. "[img=utility/go_to_arrow][img=utility/missing_icon]"
    )

    CreateCraftingGroupsPanel(
        mainFrame, target.CreatedBy,
            "[img=utility/missing_icon][img=utility/go_to_arrow]" .. target.RichTextName
    )

    CheckedTabifyColumns(frame, mainFrame, target, columnCount)
end

return Presentator
