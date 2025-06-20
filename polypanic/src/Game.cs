using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PolyPanic.Main;

class Game : GameWindow
{
    
    // This is the method that runs each frame.
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        // super onupdateframe
        base.OnUpdateFrame(e);

        // Post the update frame event to the event bus with the delta time.
        Program.eventBus.Post(new Bus.UpdateFrameEvent((float)e.Time));
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        // super onkeydown
        base.OnKeyDown(e);

        // Post the keyboard event to the event bus.
        Program.eventBus.Post(new Bus.KeyboardEvent(e.Key, Bus.KeyboardEvent.KeyAction.Pressed));    
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        //super onkeyup
        base.OnKeyUp(e);

        // Post the keyboard event to the event bus.
        Program.eventBus.Post(new Bus.KeyboardEvent(e.Key, Bus.KeyboardEvent.KeyAction.Released));
    }

    // This is the constructor that initializes the game window. It is required for OpenTK.
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
    {
        Size = (width, height),
        Title = title
    })
    {
    }
}