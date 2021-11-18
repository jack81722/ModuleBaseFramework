using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGroupWindowView : WindowView
{
    [SerializeField]
    CanvasGroup _canvasGroup;
    [SerializeField]
    Button _btnClose;

    private void Start()
    {
        _btnClose.onClick.AddListener(Close);
    }


    protected override void OnOpen()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }

    protected override void OnClose()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}
