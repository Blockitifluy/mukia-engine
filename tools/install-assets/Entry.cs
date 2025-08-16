using MukiaEngine.NodeSystem;
using MukiaEngine.Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public static class Program
{
	// Window were the game is displayed
	public static Window? GameWindow { get; set; }

	public static void RunWindow()
	{
		// Do not touch unless you don't know what you're doing
		var nativeWindowSettings = new NativeWindowSettings()
		{
			ClientSize = new OpenTK.Mathematics.Vector2i(1024, 1024),
			Title = "Game_Name", // Change to your desired name
								 // This is needed to run on macos
			Flags = ContextFlags.ForwardCompatible,
		};

		using (GameWindow = new(GameWindowSettings.Default, nativeWindowSettings))
		{
			GameWindow.Run();
		}
	}

	// Path to default scene
	private static string LoadingScene = "scenes/default.scene";

	public static int Main(string[] args)
	{
		// Creates new tree
		using Tree tree = Tree.InitaliseTree(true);

		// Loads the scene from LoadingScene
		SceneHandler.LoadScene(tree, LoadingScene);

		RunWindow(); // Starts rendering

		return 0;
	}
}