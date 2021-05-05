﻿using ModuleBased.FungusPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example {
    public class WeatherModule : MonoBehaviour, IGameModule {
        public EWeatherState WeatherState;

        public ParticleSystem RainEffect;

        public delegate void SetWeatherHandler();
        [ModuleEvent]
        public event SetWeatherHandler OnSetWeather;

        #region -- IGameModule --
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
        public void SetWeather(EWeatherState state) {
            WeatherState = state;
            Debug.Log($"Set weather : {state}");
        }

        [ModuleCmd]
        public void SetSunny() {
            SetWeather(EWeatherState.Sunny);
        }


        [ModuleCmd]
        public void SetRain() {
            SetWeather(EWeatherState.Rain);
            RainEffect.Play();
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
}