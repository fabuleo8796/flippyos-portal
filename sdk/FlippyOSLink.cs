// FlippyOSLink.cs: puts your mod's app on the Home Screen of FlippyOS (Flippy's phone).
// Copy this file into your mod as it is. It doesn't need FlippyOS.dll: when FlippyOS isn't installed,
// everything here does nothing and returns false, so your mod still works on its own.
//
// Also add this line above your plugin class, so FlippyOS loads before your mod:
//   [BepInDependency("com.flippyos.fih", BepInDependency.DependencyFlags.SoftDependency)]
//
// Guide: https://fabuleo8796.github.io/flippyos-portal/#guide
using System;
using System.IO;
using System.Reflection;

public static class FlippyOSLink
{
    static Type api;

    static Type Api
    {
        get
        {
            if (api != null) return api;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType("FlippyOS.Api", false);
                if (t != null) return api = t;
            }
            return null;
        }
    }

    /// True when FlippyOS is installed (and already loaded).
    public static bool Installed => Api != null;

    /// Puts your app on the Home Screen. id = your app's id in the App Store (like "hello-flippy"), name = up to 24
    /// letters, icon = a square PNG/JPG (or null), html = your app's whole page, onMessage = what the page sends you
    /// (return JSON to answer it, or null). Returns true when it worked.
    public static bool AddApp(string id, string name, byte[] icon, string html, Func<string, string> onMessage)
    {
        var m = Api?.GetMethod("RegisterAppHtml", new[] { typeof(string), typeof(string), typeof(byte[]), typeof(string), typeof(Func<string, string>) });
        return m != null && m.Invoke(null, new object[] { id, name, icon, html, onMessage }) is bool ok && ok;
    }

    /// Sends JSON to your app's page (it arrives in window.update) while the player has it open.
    public static void Send(string id, string json) =>
        Api?.GetMethod("Push", new[] { typeof(string), typeof(string) })?.Invoke(null, new object[] { id, json });

    /// True while the player has your app open on the phone.
    public static bool IsOpen(string id) =>
        Api?.GetMethod("IsOpen", new[] { typeof(string) })?.Invoke(null, new object[] { id }) is bool open && open;

    /// A file you embedded in your .dll (EmbeddedResource with LogicalName = the file's name), as bytes or text.
    public static byte[] Bytes(string name)
    {
        using var s = typeof(FlippyOSLink).Assembly.GetManifestResourceStream(name);
        if (s == null) return null;
        using var ms = new MemoryStream();
        s.CopyTo(ms);
        return ms.ToArray();
    }

    public static string Text(string name)
    {
        var b = Bytes(name);
        return b == null ? null : System.Text.Encoding.UTF8.GetString(b);
    }
}
