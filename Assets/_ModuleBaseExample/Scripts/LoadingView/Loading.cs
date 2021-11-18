using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour, IProgress<float>
{
    [SerializeField]
    private Image barValue;

    public void Report(float value)
    {
        barValue.fillAmount = value;
    }
}
