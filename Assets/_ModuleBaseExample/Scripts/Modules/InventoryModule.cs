using ModuleBased.ForUnity;
using ModuleBased.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    [Injectable(typeof(IInventoryModule))]
    public class InventoryModule : UniGameModule, IInventoryModule
    {
        protected override IEnumerator OnInitializingModule(IProgress<ProgressInfo> progress)
        {
            var start = DateTime.Now;
            while ((DateTime.Now - start).TotalSeconds < 3)
            {
                yield return new WaitForSeconds(3);
            }
        }
    }

    public interface IInventoryModule
    {

    }
}