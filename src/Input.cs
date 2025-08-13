using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MukiaEngine;

public static class Input
{
    public static event EventHandler<KeyboardKeyEventArgs>? OnKeyDown;
    public static event EventHandler<KeyboardKeyEventArgs>? OnKeyUp;

    [AllowNull]
    public static KeyboardState KeyboardState { get; set; }

    [AllowNull]
    public static MouseState MouseState { get; set; }

    public static float InputAxis(Keys neg, Keys pos)
    {
        bool negBool = KeyboardState.IsKeyDown(neg),
        posBool = KeyboardState.IsKeyDown(pos);

        if (negBool && !posBool) return -1;
        if (posBool) return 1;

        return 0;
    }

    internal static void SendKeyDown(object sender, KeyboardKeyEventArgs e)
    {
        OnKeyDown?.Invoke(sender, e);
    }

    internal static void SendKeyUp(object sender, KeyboardKeyEventArgs e)
    {
        OnKeyUp?.Invoke(sender, e);
    }
}
