using Fungus;
using ModuleBased.FungusPlugin;

namespace ModuleBased.Example {
    [CommandInfo("Modules",
        "WeatherCmd",
        "Set weather")]
    public class WeatherCmd : GenericCmd<WeatherModule> { }
}