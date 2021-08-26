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
        for (int i = 0; i < 2; i++)
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
            
            GetView<Text>("/TestView(Clone)_0/Id_0").text = "0";
            GetView<Text>("/TestView(Clone)_0/Name_0").text = "Item0";
            GetView<Text>("/TestView(Clone)_0/Date_0").text = "123";
            GetView<Text>("/TestView(Clone)_1/Id_0").text = "1";
            GetView<Text>("/TestView(Clone)_1/Name_0").text = "Item1";
            GetView<Text>("/TestView(Clone)_1/Date_0").text = "321";
                
            //Logger.Log(GetView<Text>("/TestView(Clone)_0/Text_0") == null);
            //Logger.Log(GetView<Image>("TestView(Clone)_0/Image_0") == null);
            //Logger.Log(GetView<Button>("TestView(Clone)_0/Button_0") == null);
        }
    }
}
