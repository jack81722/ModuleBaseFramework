using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModuleBased.Example.GameFlow
{
    [InjectableFactory(typeof(IObservable<LevelView>), EContractScope.Transient)]
    public class LevelModule : SceneFactory<LevelView>, IFactory
    {
        protected override string SceneName => "Level";
    }
}