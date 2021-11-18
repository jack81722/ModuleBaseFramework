using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    [Injectable(typeof(IStoreModule))]
    public class StoreModule : UniGameModule, IStoreModule
    {
        [Inject]
        private IInventoryModule _inventory;
        [Inject]
        private IInventoryModule inventoryProp { get; set; }

        private void OnGUI()
        {
            if(GUI.Button(new Rect(10, 10, 100, 20), "Click"))
            {
                Debug.Log($"Inventory is null? {_inventory == null}");
                Debug.Log($"Inventory is null? {inventoryProp == null}");
            }
        }
    }

    public interface IStoreModule
    {

    }
}