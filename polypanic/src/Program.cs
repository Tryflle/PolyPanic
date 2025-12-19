using PolyPanic.Bus;
using PolyPanic.Debug;

namespace PolyPanic.Main;

class Program
{

    // Create the eventbus instance.
    public static EventBus eventBus = new EventBus();
    
    // Keep references for cleanup
    private static Render.Renderer? renderer;
    private static Render.Font.FontRenderer? fontRenderer;
    private static Render.Mesh.MeshRenderer? meshRenderer;

    // Entrypoint.
    public static void Main(string[] args)
    {
        try
        {
            // Create a new instance of the Game class and run it.
            using (Game game = new Game(800, 600, "PolyPanic"))
            {
                game.Run();
            }
        }
        finally
        {
            // Ensure proper cleanup of resources
            OnCleanup();
        }
        // Anything after this is runned after the game is closed. So, not much of a reason to touch it.
        Environment.Exit(0);
    }

    public static void OnInit()
    {
        // subscribe debug listener
        eventBus.Subscribe(new Debug.TestListener());
        // subscribe renderer
        renderer = new Render.Renderer();
        eventBus.Subscribe(renderer);
        // subscribe font renderer
        fontRenderer = new Render.Font.FontRenderer();
        eventBus.Subscribe(fontRenderer);
        // subscribe mesh renderer
        meshRenderer = new Render.Mesh.MeshRenderer();
        eventBus.Subscribe(meshRenderer);
    }

    public static void OnCleanup()
    {
        // Dispose all resources
        renderer?.Dispose();
        fontRenderer?.Dispose();
        meshRenderer?.Dispose();
    }
}