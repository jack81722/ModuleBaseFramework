using ModuleBased.ForUnity;
using ModuleBased.Node;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMacroView : UniView
{
    public TestView Prefab;

    protected override void OnInitializeView()
    {
        TestView view;
        for (int i = 0; i < 10; i++)
        {
            view = Instantiate(Prefab);
            AddNode(view);
        }
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(10, 10, 100, 20), "TryGetView"))
        {
            //foreach(var node in this.Traversal())
            //{
            //    Debug.Log(node.PathName);
            //}
            Logger.Log(GetView<Text>("/TestView(Clone)_0/Text_0") == null);
            Logger.Log(GetView<Image>("TestView(Clone)_0/Image_0") == null);
            Logger.Log(GetView<Button>("TestView(Clone)_0/Button_0") == null);
        }
    }
}
