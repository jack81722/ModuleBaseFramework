using ModuleBased.Example.MainPage;
using ModuleBased.Example.Scenes;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System;

namespace ModuleBased.Example.GameFlow
{
    [InjectableFactory(typeof(IObservable<MainPageView>), EContractScope.Transient)]
    public class MainPageModule : SceneFactory<MainPageView>
    {
        protected override string SceneName => "MainPage";
    }
}