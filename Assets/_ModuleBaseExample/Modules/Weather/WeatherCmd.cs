using Fungus;
using ModuleBased.Dialogue.FungusPlugin;

namespace ModuleBased.Example {
    [CommandInfo("Modules",
        "WeatherCmd",
        "Set weather")]
    public class WeatherCmd : GenericCmd<IWeatherModule> { }
}