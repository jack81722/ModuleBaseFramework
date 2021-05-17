using Fungus;
using ModuleBased.Dialogue.FungusPlugin;

namespace ModuleBased.Example {
    [EventHandlerInfo("Modules",
        "WeatherEvent",
        "Event handler of weather module")]
    public class WeatherEvent : GenericEvent<WeatherModule> { }
}