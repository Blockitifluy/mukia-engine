using MukiaEngine.NodeSystem;
using MukiaEngine.Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public static class Program
{
	public static Window? GameWindow { get; set; }

	public static void RunWindow()
	{
		var nativeWindowSettings = new NativeWindowSettings()
		{
			ClientSize = new OpenTK.Mathematics.Vector2i(1024, 1024),
			Title = "Game_Name",
			// This is needed to run on macos
			Flags = ContextFlags.ForwardCompatible,
		};

		using (GameWindow = new(GameWindowSettings.Default, nativeWindowSettings))
		{
			GameWindow.Run();
		}
	}

	private static string LoadingScene = "scenes/default.scene";

	public static int Main(string[] args)
	{
		using Tree tree = Tree.InitaliseTree();
		SceneHandler.LoadScene(tree, LoadingScene);

		RunWindow();
		return 0;
	}
}