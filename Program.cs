using MukiaEngine;
using MukiaEngine.Graphics;
using MukiaEngine.NodeSystem;
using MukiaEngine.Physics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public static class Program
{
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
        bool verbose = false,
        noSingletons = false;

        string? saveOnQuitPath = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "-v":
                case "--verbose":
                    verbose = true;
                    break;
                case "-s":
                case "--save-on-quit":
                    {
                        if (i == args.Length - 1)
                        {
                            throw new ArgumentException("--save-on-quit arguement has no path");
                        }
                        saveOnQuitPath = args[i + 1];
                        break;
                    }
                case "--no-singletons":
                    noSingletons = true;
                    break;
                default:
                    break;
            }
        }

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
            if (verbose)
            {
                flags |= SceneLoadingFlags.Verbose;
            }
            if (noSingletons)
            {
                flags |= SceneLoadingFlags.NoSingletons;
            }

            SceneHandler.LoadScene(tree, path, flags);
        }
        else
        {
            throw new ArgumentException($"Invalid command: {cmd}");
        }

        RunWindow(verbose);

        if (saveOnQuitPath is not null)
        {
            SceneSavingFlags flags = SceneSavingFlags.None;
            if (verbose)
            {
                flags |= SceneSavingFlags.Verbose;
            }
            if (noSingletons)
            {
                flags |= SceneSavingFlags.NoSingletons;
            }

            SceneHandler.SaveScene(Tree.GetCurrentTree(), saveOnQuitPath, flags);
        }
#else
        using Tree tree = Tree.InitaliseTree(true);
        CreateTestScene();
        RunWindow();
#endif
    }
}