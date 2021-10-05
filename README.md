# ModuleBaseFramework
## Introduction
This is a framework for Unity as C# MVC. With the framework, it would help you to focus how to maintain your game logic without worrying about the initialization and dependence of other scripts.

## Quick Start
Coming soon...

## Architecture
### GameCore
集中管理所有Module的程序，並提供取得其他模組的API。
```csharp
public class UniGameCore : MonoBehaviour, IGameCore {
    private IGameCore _core;
    
    ...
    
    private void Awake() {
        _core.InitiailzeModules();
    }
    
    private void Start() {
        _core.StartModules();
    }
}
```

### GameModule
遊戲模組，將集中初始化於GameCore，所有遊戲邏輯都由Module處理和管理。
```csharp
public class WeatherModule : MonoBehaviour, IGameModule {
    public IEnumerable OnModuleInitiailize(IProgress<ProgressInfo> progress) { 
        // TODO: initiailize game module here
    }
    
    public void OnModuleStart() {
        // TODO: start game module here
        // may also call required module method
    }
}
```
### Data access layer
藉由實作IDao介面宣告類別為Data access object，並且自定義需要的邏輯介面，同GameModule的實作方式。
但考慮到大量或長時間載入的情境，我們必須調整或設計一套能夠多線程或協程的方式進行載入機制。
```csharp
public class FileDao : IDao, IFileDao{
    public string ReadJsonFile(string path){
        // TODO: something ...
    }
}

public interface IFileDao{
    string ReadJsonFile(string path);
}
```

### View
View有自己的初始化機制，在初始化階段透過系統提供的Module Collection提取需要的Module，並取得Module參照來註冊行為。View通常與MonoBehaviour一起使用，你可以繼承框架提供的UniView，UniView有內建的節點機制來輔助整理View物件的Hierarchy，並且UniView提供更完整的機制來處理View的生命週期；
```csharp
public class WeatherView : UniView {
    protected override OnBeginInitializeView() {
        // you may add other uniview here
    }
    
    protected override OnEndInitializeView() {
        // handle access of other child views
    }
    
    protected override OnDestroyView() {
        // custom your destroy mechanism
    }
}
```
如果你不需要過於複雜的View機制，亦可以自行宣告MonoBehaviour並實作IGameView，定義自己的View行為。
```csharp
public class CharacterView : MonoBehaviour, IGameView {
    ...

    public void InitializeView() { }
    
    public void ApplyView() { }
}
```

## Features
### Dependence injection
- **RequireModuleAttribute**
GameCore會主動注入指定有```[RequireModule]```標籤的Module(可Private或Public)，降低與其他模組的依賴性，注入的類別必須依賴介面而非類別。
注入行為會在Initialize後執行，如需使用其他模組功能，必須在Start的階段呼叫。
```csharp
public class WeatherModule : MonoBehaviour, IGameModule {
    [RequireModule]
    private IOtherModule _otherMod;
    
    public void OnModuleStart() {
        _otherMod.CallMethod();
    }
}
```
- **RequireDao**
GameCore會主動注入指定有```[RequireDao]```標籤的Dao，用法同```[RequireModule]```。
```csharp
public class WeatherModule : MonoBehaviour, IGameModule{
    [RequireDao]
    private IOtherDao _otherDao;
}
```

### Proxy & AOP (Aspect-oriented programming)
你可以藉由自訂Proxy來實現額外且解耦的邏輯，利用```ModuleBased.Proxy```庫中的方法與類別來實現，由最基本的```ProxyBase```與```ProxyBase<T>```來進行Proxy邏輯的擴充；
或是利用已經完成的```AOPProxy```來快速處理AOP架構，使用步驟如下：
1. 宣告介面並分別宣告class與proxy
2. 對欲使用Proxy的class用```ProxyAttribute```指定Proxy型別
3. 透過DefaultProxyFactory來產生封裝後的Proxy；或是交由GameCore產生</br>
AOPProxy可在指定的方法上附加須執行的Attribute，便可實作出自定義的Before, After, Around及Error呼叫；或是您也可以建立網路連線的Proxy來同步邏輯。

```csharp
[Proxy(typeof(CustomProxy))]
public class CustomModule : IGameModule, ICustom {
    ...
    // implement ICustom
    ...
}

public class CustomProxy : AOPProxy<CustomModule>, ICustom {
    public void CustomMethodA(int arg) {
        InvokeProxyMethod(arg);
    }
    
    public string CustomMethodB(int arg) {
        return (string)InvokeProxyMethod(arg);
    }
}
```
**Notice: RealProxy的GetTransparentProxy機制在「IL2CPP」的建置環境中無法正常運作，原因是IL2CPP無法支援```System.Reflection.Emit```中的方法。**

### Reactive extension
如一般Reactive的使用方式，可以像UniRx或其他任何Rx一樣使用本框架提供的方法。

## Future
- Repo, Service layer
- Initialize mechanism
- View modification
- Code generation
