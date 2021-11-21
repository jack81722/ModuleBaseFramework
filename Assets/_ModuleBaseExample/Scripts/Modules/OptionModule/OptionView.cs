using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example
{
    [Injectable(typeof(OptionView))]
    public class OptionView : MonoBehaviour
    {
        private IConfigModule _configMod;

        // slider
        // toggle...
        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private Toggle _toggle;

        [Inject]
        private void Init(IConfigModule configMod)
        {
            Debug.Log("Option init");
            _configMod = configMod;
            
            _configMod.TryCreate("slider", 0.5f);
            _configMod.TryCreate("toggle", true);

            _slider.onValueChanged.AddListener((v) => _configMod.Save<float>("slider", v));
            _toggle.onValueChanged.AddListener((v) => _configMod.Save<bool>("toggle", v));

            _configMod.Subcribe<float>("slider", (v) => _slider.value = v);
            _configMod.Subcribe<bool>("toggle", (v) => _toggle.isOn = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                _configMod.Save<float>("slider", _configMod.LoadOrDefault<float>("slider", 0.5f) + 0.1f);
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _configMod.Save<float>("slider", _configMod.LoadOrDefault<float>("slider", 0.5f) - 0.1f);
            }
        }
    }
}
