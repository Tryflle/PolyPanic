using PolyPanic.Bus;

namespace PolyPanic.Main;

class Program
{

    // Create the eventbus instance.
    public static EventBus eventBus = new EventBus();


    // Entrypoint.
    public static void Main(string[] args)
    {
        // DEBUG
        eventBus.Subscribe(new Debug.TestListener());
        // Create a new instance of the Game class and run it.
        using (Game game = new Game(800, 600, "PolyPanic"))
        {
            game.Run();
        }
        // Anything after this is runned after the game is closed. So, not much of a reason to touch it.
    }
}