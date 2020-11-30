local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Database = require("ingteb.Database")
local UI = require("core.UI")

local function CreateSpriteAndRegister(frame, target)
    local result

    local tooltip = target and target.HelperText
    local sprite = target and target.SpriteName
    local number = target and target.NumberOnSprite
    local show_percent_for_small_numbers = target and target.UsePercentage
    local style = Helper.SpriteStyleFromCode(target and target.SpriteStyle)

    if target then
        result = frame.add {
            type = "sprite-button",
            tooltip = tooltip,
            sprite = sprite,
            number = number,
            show_percent_for_small_numbers = show_percent_for_small_numbers,
            style = style,
        }
    else
        result = frame.add {type = "sprite-button", style = style}
    end

    global.Current.Links[result.index] = target and target.ClickHandler
    if target and (target.IsDynamic or target.HasLocalisedDescriptionPending) then
        if target and target.object_name == "BonusSet" then --
            local s = 2 --
        end
        global.Current.Gui:AppendForKey(target, result)
    end
    return result
end

local maximalCount = 6

local function CreateRecipeLinePart(frame, target, count, isInput)
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
        style = isInput and "ingteb-flow-right" or nil,
    }

    target:Select(function(item) return CreateSpriteAndRegister(subPanel, item) end)

    if isInput then return end

    for _ = target:Count() + 1, count do --
        subPanel.add {type = "sprite", style = "ingteb-un-button"}
    end

end

local function CreateRecipeLine(frame, target, inCount, outCount)
    local subFrame = frame.add {type = "flow", direction = "horizontal"}

    CreateRecipeLinePart(subFrame, target.Input, math.min(inCount, maximalCount), true)

    local properties = subFrame.add {type = "flow", direction = "horizontal"}
    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}
    CreateSpriteAndRegister(properties, target.Technology)
    CreateSpriteAndRegister(properties, target)
    CreateSpriteAndRegister(properties, {SpriteName = "utility/clock", NumberOnSprite = target.Time})
    properties.add {type = "sprite", sprite = "utility/go_to_arrow"}

    CreateRecipeLinePart(subFrame, target.Output, math.min(outCount, maximalCount), false)
end

local function CreateCraftingGroupPanel(frame, target, category, inCount, outCount)
    assert(type(category) == "string")

    frame.add {type = "line", direction = "horizontal"}

    local workersPanel = frame.add {
        type = "flow",
        style = "ingteb-flow-centered",
        direction = "horizontal",
    }

    local workers = target[1].Database:GetCategory(category).Workers
    workers:Select(function(worker) return CreateSpriteAndRegister(workersPanel, worker) end)

    frame.add {type = "line", direction = "horizontal"}

    if target:Count() < settings.player["ingteb_group-tab-threshold"].value then
        target:Select(function(recipe) CreateRecipeLine(frame, recipe, inCount, outCount) end)
    else
        local groupPanel = frame.add {type = "tabbed-pane"}
        target:ToGroup(function(value) return {Key = value.Group.name, Value = value} end) --
        :Select(
            function(value)
                local group = value[1].Group
                local tab = groupPanel.add {
                    type = "tab",
                    caption = "[item-group=" .. group.name .. "]",
                    tooltip = group.localised_name,
                    style = "ingteb-tab",
                }
                local frame = groupPanel.add {type = "frame", direction = "vertical"}
                groupPanel.add_tab(tab, frame)

                if value:Count() < settings.player["ingteb_subgroup-tab-threshold"].value then
                    value:Select(
                        function(recipe)
                            CreateRecipeLine(frame, recipe, inCount, outCount)
                        end
                    )
                else
                    local groupPanel = frame.add {type = "tabbed-pane"}
                    local g = value:ToGroup(
                        function(value)
                            return {Key = value.SubGroup.name, Value = value}
                        end
                    ) --
                    g:Select(
                        function(value)
                            local group = value[1].SubGroup
                            local caption = group.name
                            if value[1] and value[1].Output[1] then
                                local main = value[1].Output[1]
                                caption = "[" .. main.SpriteType .. "=" .. main.Name .. "]"
                            end
                            local tab = groupPanel.add {
                                type = "tab",
                                caption = caption,
                                tooltip = group.localised_name,
                                style = "ingteb-tab",
                            }
                            local subFrame = groupPanel.add {type = "frame", direction = "vertical"}
                            groupPanel.add_tab(tab, subFrame)

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
    assert(type(next(target)) == "string")

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

    headerFlow.add {type = "label", name = "headerSprites", caption = headerSprites}

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
            assert(type(category) == "string")
            CreateCraftingGroupPanel(subFrame, recipes, category, inCount, outCount)
        end
    )
end

local function CreateMainPanel(frame, target)
    frame.caption = target.LocalisedName

    local scrollframe = frame.add {
        type = "scroll-pane",
        horizontal_scroll_policy = "never",
        direction = "vertical",
        name = "frame",
    }

    target:SortAll()
    assert(
        not target.RecipeList or not next(target.RecipeList) or type(next(target.RecipeList))
            == "string"
    )
    assert(not target.UsedBy or not next(target.UsedBy) or type(next(target.UsedBy)) == "string")
    assert(
        not target.CreatedBy or not next(target.CreatedBy) or type(next(target.CreatedBy))
            == "string"
    )

    local mainFrame = scrollframe
    local columnCount --
    = (target.RecipeList and target.RecipeList:Any() and 1 or 0) --
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

    CreateCraftingGroupsPanel(
        mainFrame, target.RecipeList, target.RichTextName .. "[img=factorio]"
    )

    CreateCraftingGroupsPanel(
        mainFrame, target.UsedBy,
            target.RichTextName .. "[img=utility/go_to_arrow][img=utility/missing_icon]"
    )

    CreateCraftingGroupsPanel(
        mainFrame, target.CreatedBy,
            "[img=utility/missing_icon][img=utility/go_to_arrow]" .. target.RichTextName
    )

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
                        " >>> [" .. mainFrame.children[tabIndex].headerFlow.headerSprites.caption .."]",
                    }
                    mainFrame.children[tabIndex].visible = false
                else
                    mainFrame.children[tabIndex].headerFlow.add {
                        type = "sprite-button",
                        sprite = "hide-this-column",
                        name =  order,
                    }
                end
            end
        )
        global.Current.Links[frame.index] = target

    end

end

local Gui = {Active = {}}

function Gui:FindTarget(player)
    assert(player)
    assert(self.Active.ingteb)
    assert(not self.Active.Selector)
    assert(not self.Active.Presentator)

    local function get()
        local cursor = player.cursor_stack
        if cursor and cursor.valid and cursor.valid_for_read then
            return Database:GetItem(cursor.name)
        end
        local cursor = player.cursor_ghost
        if cursor then return Database:GetItem(cursor.name) end

        local cursor = player.selected
        if cursor then
            local result = Database:GetEntity(cursor.name)
            if result.IsResource then
                return result
            else
                return result.Item
            end
        end

        local cursor = player.opened
        if cursor then

            local t = player.opened_gui_type
            if t == defines.gui_type.custom then return end
            if t == defines.gui_type.entity then return self.Entities[cursor.name] end

            assert()
        end
        -- local cursor = global.Current.Player.entity_copy_source
        -- assert(not cursor)

    end

    local result = get()
    return result
end

function Gui:ScanSelector(player)
    self.Active.ingteb = global.Current.Player.gui.top.ingteb
    self.Active.Selector = player.gui.screen.Selector
    self.Active.Presentator = player.gui.screen.Presentator
end

function Gui:CloseSelector(player)
    player.gui.screen.Selector.destroy()
    self.Active.Selector = nil
end

function Gui:ClosePresentator(player)
    player.gui.screen.Presentator.destroy()
    self.Active.Presentator = nil
end

function Gui:SelectTarget(player)
    Helper.ShowFrame(
        player, "Selector", function(frame)
            frame.caption = "select"
            frame.add {type = "choose-elem-button", elem_type = "signal"}
        end
    )
    self:ScanSelector(player)
end

function Gui:PresentTarget(player, target)
    assert(target.Prototype)
    Helper.ShowFrame(
        player, "Presentator", function(frame) return CreateMainPanel(frame, target) end
    )
    self:ScanSelector(player)
    return target
end

function Gui:OnMainButtonPressed(player)
    assert(self.Active.ingteb)
    assert(not self.Active.Selector or not self.Active.Presentator)

    if self.Active.Selector then
        self:CloseSelector(player)
    elseif self.Active.Presentator then
        self:ClosePresentator(player)
    else
        local target = self:FindTarget(player)
        if target then
            return self:PresentTarget(player, target)
        else
            self:SelectTarget(player)
        end
    end
end

function Gui:EnsureMainButton()
    local player = global.Current.Player -- todo: multiplayer
    if player.gui.top.ingteb == nil then
        assert(not self.Active.ingteb)

        global.Current.Player.gui.top.add {
            type = "sprite-button",
            name = "ingteb",
            sprite = "ingteb",
        }
    end
    self:ScanSelector(player)
end

function Gui:OnGuiClick(player, event)
    local element = event.element
    if element == Gui.Active.ingteb then
        return self:OnMainButtonPressed(player)
    elseif element == Gui.Active.Selector then
        return
    elseif element == Gui.Active.Presentator then
        return
    end

    if self.Active.Presentator then return self:OnGuiClickForPresentator(player, event) end
end

function Gui:UpdateTabOrder(tabOrder, dropIndex)
    local dropTabIndex = tabOrder[tonumber(dropIndex)]
    tabOrder:Remove(dropIndex)
    tabOrder:Append(dropTabIndex)
end

function Gui:OnGuiClickForPresentator(player, event)
    local target = global.Current.Links[event.element.index]
    if target and target.Prototype then
        if UI.IsMouseCode(event, "--- l") then return self:PresentTarget(player, target) end

        local order = target:GetHandCraftingOrder(event)
        if order then
            player.begin_crafting(order)
            return
        end

        local order = target:GetResearchOrder(event)
        if order then
            player.force.add_research(order.Technology)
            return
        end
    else
        local target = global.Current.Links[self.Active.Presentator.index]
         self:UpdateTabOrder(target.TabOrder, event.element.name)
         return self:PresentTarget(player, target)
    end
end

return Gui
