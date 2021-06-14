using ModuleBased.AOP.Attributes;
using ModuleBased.ForUnity;
using ModuleBased.Dialogue;
using UnityEngine;

namespace ModuleBased.Example {
    [ModuleItf(typeof(IWeatherModule))]
    public class WeatherModule : MonoBehaviour, IGameModule, IWeatherModule {
        public EWeatherState WeatherState;

        public ParticleSystem RainEffect;

        public delegate void SetWeatherHandler();

        [ModuleEvent]
        public event SetWeatherHandler OnSetWeather;

        #region -- IGameModule --
        public IGameModuleCollection Modules { get; set; }
        public ILogger Logger { get; set; }

        public void OnModuleInitialize() {
            WeatherState = EWeatherState.Sunny;
        }

        public void OnModuleStart() {

        }
        #endregion

        #region -- Unity APIs --
        private void Update() {
            if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                int current = (int)WeatherState;
                current = (current + 1) % 4;
                SetWeather((EWeatherState)current);
                OnSetWeather?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                int current = (int)WeatherState;
                current = (current + 3) % 4;
                SetWeather((EWeatherState)current);
                OnSetWeather?.Invoke();
            }
        }
        #endregion

        #region -- Cmds --
        [ModuleCmd]
        public void SetWeather(EWeatherState state) {
            WeatherState = state;
            Debug.Log($"Set weather : {state}");
            if (state == EWeatherState.Rain)
                RainEffect.Play();
        }

        [ModuleCmd]
        public void SetSunny() {
            SetWeather(EWeatherState.Sunny);
        }


        [ModuleCmd]
        public void SetRain() {
            SetWeather(EWeatherState.Rain);
            //RainEffect.Play();
        }

        [ModuleCmd]
        public void SetCloudy() {
            SetWeather(EWeatherState.Cloudy);
        }

        [ModuleCmd]
        public void SetWindy() {
            SetWeather(EWeatherState.Windy);
        }
        #endregion
    }

    [SerializeField]
    public enum EWeatherState : int{
        Sunny = 0,
        Rain = 1,
        Cloudy = 2,
        Windy = 3
    }

    public interface IWeatherModule {
        [SimpleLog]
        [ModuleCmd]
        void SetWeather(EWeatherState state);

        [SimpleLog]
        [ModuleCmd]
        void SetRain();

        [ModuleCmd]
        void SetSunny();

        [ModuleCmd]
        void SetCloudy();

        [ModuleCmd]
        void SetWindy();
    }
}