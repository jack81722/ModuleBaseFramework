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
            if(GUI.Button(new Rect(10, 10, 100, 20), "Click"))
            {
                module = MyGameCore.Singleton.Modules.GetModule<IMyModule>();
                module.SayHello();
                //string str = module.GetHelloStr(0, 3);
                //print(str);
            }
        }
    }
}