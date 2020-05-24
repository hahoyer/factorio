data:extend({
  {
    type = "custom-input",
    name = "ixuAutoCrafting-increase",
    key_sequence = "U",
    consuming = "none"
  },{
    type = "custom-input",
    name = "ixuAutoCrafting-decrease",
    key_sequence = "J",
    consuming = "none"
  },{
    type = "sound",
    name = "ixuAutoCrafting-core-crafting_finished",
    filename = "__core__/sound/crafting-finished.ogg",
    volume = 1
  },{
    type = "shortcut",
    name = "ixuAutoCrafting-toggle",
    action = "lua",
    toggleable = true,
    icon =
    {
      filename = "__ixuAutoCrafting__/graphics/icon/shortcut-toggle.png",
      priority = "extra-high-no-scale",
      size = 144,
      scale = 0.2,
      flags = {"icon"}
    },
  },
})