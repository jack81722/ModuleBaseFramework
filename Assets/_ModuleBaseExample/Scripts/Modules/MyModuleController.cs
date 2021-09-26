using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    public class MyModuleController : MonoBehaviour
    {

        private IMyModule module;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 100, 20), "Click 1"))
            {
                module = UniGameCore.Singleton.Modules.GetModule<IMyModule>();
                module.SayHello();

            }
            if (GUI.Button(new Rect(10, 40, 100, 20), "Click 2"))
            {
                module = UniGameCore.Singleton.Modules.GetModule<IMyModule>();
                string str = module.GetHelloStr(0, 3);
                print(str);
            }
        }
    }
}