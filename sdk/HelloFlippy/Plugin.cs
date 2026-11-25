// Hello Flippy: the smallest mod with its own FlippyOS app. A counter: the page asks, the mod answers.
using System.Text.Json;
using BepInEx;
using BepInEx.Unity.IL2CPP;

[BepInPlugin("com.example.helloflippy", "Hello Flippy", "1.0.0")]
[BepInDependency("com.flippyos.fih", BepInDependency.DependencyFlags.SoftDependency)] // FlippyOS loads first
public class HelloFlippy : BasePlugin
{
    const string AppId = "hello-flippy"; // the same id as your app in the developer portal
    int count;

    public override void Load()
    {
        bool ok = FlippyOSLink.AddApp(AppId, "Hello", FlippyOSLink.Bytes("icon.png"), FlippyOSLink.Text("app.html"), OnMessage);
        Log.LogInfo(ok ? "Hello is on Flippy's phone" : "FlippyOS isn't installed: no phone app");
    }

    // Everything app.html sends with window.flippyos(...) arrives here. The JSON you return goes to window.update(...).
    string OnMessage(string json)
    {
        using var doc = JsonDocument.Parse(json);
        switch (doc.RootElement.GetProperty("a").GetString())
        {
            case "ready": return State();          // the page just opened
            case "add": count++; return State();   // the button was pressed
            case "reset": count = 0; return State();
        }
        return null;
    }

    string State() => "{\"count\":" + count + "}";
}
