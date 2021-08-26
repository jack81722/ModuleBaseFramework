using ModuleBased.ForUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestView : UniView
{
    public Text mytext;
    public Image myimg;
    public Button mybtn;

    protected override void OnInitializeView()
    {
        Debug.Log("run");
        mytext = GetView<Text>("Text_0");
    }
}

