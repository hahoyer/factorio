using System.Linq;
using hw.Helper;
using ManageModsAndSaveFiles;

namespace Test
{
    sealed class ProfilerParser
    {
        public ProfilerParser(string configurationName)
        {
            var configuration = MmasfContext
                .Instance
                .UserConfigurations
                .Single(context=> context.Name == configurationName);

            var y = new LogfileWatcher(configuration.Path);

            4.Seconds().Sleep();

            var yy = y.Lines.Where(line => line.LineData is ScriptLine).ToArray();
        }

        public void Format() {}
    }
}