using CommandLine;
using MukiaEngine;
using MukiaEngine.Graphics;
using MukiaEngine.NodeSystem;
using MukiaEngine.Physics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public static class Program
{
    private class CLIOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages")]
        public bool Verbose { get; set; }
        [Option('s', "save-on-quit", Required = false, HelpText = "Save the scene when quiting")]
        public string? SaveOnQuit { get; set; }
        [Option("no-singletons", Required = false, HelpText = "Load and save no singletons")]
        public bool NoSingletons { get; set; }
    }

    public static Window? GameWindow { get; set; }

    public static void RunWindow(bool verbose = false)
    {
        var nativeWindowSettings = new NativeWindowSettings()
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(1024, 1024),
            Title = "Zombie Survival",
            // This is needed to run on macos
            Flags = ContextFlags.ForwardCompatible,
        };

        if (verbose)
        {
            Console.WriteLine("Started Window");
        }

        using (GameWindow = new(GameWindowSettings.Default, nativeWindowSettings))
        {
            GameWindow.Run();
        }
    }

    public const string ProgramHelp = """
	demo - Loads and save the demo level
	load [path to scene] - Loads the scene from the file
	""";

    public static void CreateTestScene()
    {
        _ = Node.New<HoveringCamera>(null);

        RigidBody rigid = Node.New<RigidBody>(null, "awe-body");
        rigid.GlobalPosition = EVector3.Up * 50.0f;
        rigid.Mass = 0.5f;

        MeshContainer awesomeCube = Node.New<MeshContainer>(rigid, "awesome-cube");
        awesomeCube.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Cube);
        awesomeCube.Position += EVector3.Up;

        Collider collision0 = Node.New<Collider>(rigid, "awe-collision");
        collision0.ApplyCollisionShape(new CubeCollision());
        rigid.Collider = collision0;

        MeshContainer crateCube = Node.New<MeshContainer>(null, "crate-cube");
        crateCube.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Cube);
        crateCube.Texture0 = "container.png";

        Collider collision1 = Node.New<Collider>(crateCube, "crate-collision");
        collision1.ApplyCollisionShape(new CubeCollision());

    }

    public static void Main(string[] args)
    {
#if !DEBUG
        Parser.Default.ParseArguments<CLIOptions>(args)
        .WithParsed<CLIOptions>(o =>
        {
            string? cmd = args.ElementAtOrDefault(0);
            if (cmd is null)
            {
                Console.WriteLine(ProgramHelp);
                return;
            }

            using Tree tree = Tree.InitaliseTree(true);

            if (cmd == "demo")
            {
                CreateTestScene();
            }
            else if (cmd == "load")
            {
                string path = args[1];

                SceneLoadingFlags flags = SceneLoadingFlags.None;
                if (o.Verbose)
                {
                    flags |= SceneLoadingFlags.Verbose;
                }
                if (o.NoSingletons)
                {
                    flags |= SceneLoadingFlags.NoSingletons;
                }

                SceneHandler.LoadScene(tree, path, flags);
            }

            RunWindow(o.Verbose);

            if (o.SaveOnQuit is not null)
            {
                SceneSavingFlags flags = SceneSavingFlags.None;
                if (o.Verbose)
                {
                    flags |= SceneSavingFlags.Verbose;
                }
                if (o.NoSingletons)
                {
                    flags |= SceneSavingFlags.NoSingletons;
                }

                SceneHandler.SaveScene(Tree.GetCurrentTree(), o.SaveOnQuit, flags);
            }
        });
#else
        using Tree tree = Tree.InitaliseTree(true);
        CreateTestScene();
        RunWindow();
#endif
    }
}