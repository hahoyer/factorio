local result = 
{
    ModName = "ixuAutoSave",
    MinFrequencyTicks = 60,
}

result.GlobalPrefix = result.ModName .. "_" .. "GlobalPrefix"
result.Frequency = result.ModName .. "_" .."Frequency"
result.EnterPrefixOnInit = result.ModName .. "_" .."EnterPrefixOnInit"

result.GraphicsPath = "__"..result.ModName.."__/graphics/"
return result
