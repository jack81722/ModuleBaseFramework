# ModuleBaseFramework
Module-based framework of Unity

**GameCore** <br/>
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

**GameModule** <br/>
遊戲模組，將集中初始化於GameCore，所有遊戲邏輯都由Module處理和管理。
```csharp
public class WeatherModule : MonoBehaviour, IGameModule {
    public void OnModuleInitiailize() { 
        // TODO: initiailize game module here
    }
    
    public void OnModuleStart() {
        // TODO: start game module here
        // may also call required module method
    }
}
```

**RequireModuleAttribute** <br/>
GameCore會主動注入指定有```[RequireModule]```標籤的Module(可Private或Public)，降低與其他模組的依賴性，
注入行為會在Initialize後執行，如需使用其他模組功能，必須在Start的階段呼叫。
```csharp
public class WeatherModule : MonoBehaviour, IGameModule {
    [RequireModule]
    private OtherModule _otherMod;
    
    public void OnModuleStart() {
        _otherMod.CallMethod();
    }
}
```

# Features
## Dependence injection
## Proxy & AOP
## Reactive extension
## Data access layer


