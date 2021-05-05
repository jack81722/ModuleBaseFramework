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

# Fungus Plugin
**前置需求** <br/>
0. 使用前請先安裝Fungus (Git連結：https://github.com/snozbot/fungus)
1. 在Fungus.Command類別中加入方法 (此修改會將GenericCmd選擇的方法顯示在FlowChart內的Block的CommandList中，不修改也可以)
```
public virtual string CommandName() {
    return null;
}
```
3. 修改CommandListAdaptor (此修改會將GenericCmd選擇的方法顯示在FlowChart內的Block的CommandList中，不修改也可以)
```csharp
public class CommandListAdaptor{
    ...
    public void DrawItem(Rect position, int index, bool selected, bool focused) {
    ...
            // 原本的程式碼
            // commandName = commandInfoAttr.CommandName;
            
            // 改成
            string commandName = null;
            if (command != null)
                commandName = command.CommandName();
            if (string.IsNullOrEmpty(commandName)) {
                commandName = commandInfoAttr.CommandName;
            }
    ...
    }
}
```

**GenericCmd** <br/>
繼承自Fungus的Command，每個需要指令化的模組都必須宣告新Class並繼承。
```csharp
[CommandInfo(...)]
public class WeatherModCmd : GenericCmd<WeatherModule> { }
```

**GenericEvent** <br/>
繼承自Fungus的EventHandler，每個需要事件化的模組都必須宣告新Class並繼承。
```csharp
[EventHandlerInfo(...)]
public class WeatherModCmd : GenericEvent<WeatherModule> { }
```

**ModuleCmd** <br/>
將指定標籤的方法註冊進GenericCmd。
```csharp
public class WeatherModule : MonoBehaviour, IGameModule {
    [ModuleCmd("Rain")]
    public void SetRain() { ... }
    
    [ModuleCmd]   // Show "SetSunny" on inspector
    public void SetSunny() { ... }
    
    ...
}
```
**ModuleEvent** <br/>
將指定標籤的事件註冊進GenericEvent。
```csharp
public class WeatherModule : MonoBehaviour, IGameModule {
    public delegate void SetWeatherHandler();
    // can also use [ModuleEvent] to show default name on inspector
    [ModuleEvent("OnSetWeather")]      
    public event SetWeatherHandler OnSetWeather;
    
    ...
}
```

# Quick Start
1. 建立UniGameCore物件至Hierarchy，並在其物件底下建立其他模組。
![image](https://user-images.githubusercontent.com/34429337/117030784-8cf88780-ad32-11eb-9288-8a2dc4767863.png)

2. 實作FungusPlugin後即可在Fungus內進行編輯。
![image](https://user-images.githubusercontent.com/34429337/117031112-da74f480-ad32-11eb-8f15-c022d03689aa.png)


# Future Feature
1. ModuleCmd可以執行帶參數的方法，並於Fungus編輯時指定參數。
