using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace PolyPanic.Main;

class Game : GameWindow
{

    // These methods are all overrides of the GameWindow class methods. What they do is self explanatory.
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        // super onupdateframe
        base.OnUpdateFrame(e);

        // Clears screen
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // Post the update frame event to the event bus with the delta time.
        Program.eventBus.Post(new Bus.UpdateFrameEvent((float)e.Time));

        // reverses the two buffers because the docs said so.
        SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        // super onframebufferresize
        base.OnFramebufferResize(e);

        // Set the viewport to the new size of the framebuffer.
        GL.Viewport(0, 0, e.Width, e.Height);
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

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        //super onmousemove
        base.OnMouseMove(e);

        // Post the mouse move event to the event bus.
        Program.eventBus.Post(new Bus.MouseMoveEvent(e.X, e.Y));
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        //super onmousedown
        base.OnMouseDown(e);
        // Post the mouse button event to the event bus.
        Program.eventBus.Post(new Bus.MouseButtonEvent(e.Button, Bus.MouseButtonEvent.ButtonAction.Pressed));
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        //super onmouseup
        base.OnMouseUp(e);
        // Post the mouse button event to the event bus.
        Program.eventBus.Post(new Bus.MouseButtonEvent(e.Button, Bus.MouseButtonEvent.ButtonAction.Released));
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        //super onresize
        base.OnResize(e);

        int x = e.Width;
        int y = e.Height;

        // Post event
        Program.eventBus.Post(new Bus.ResizeWindowEvent(x, y));
    }

    protected override void OnLoad()
    {
        //super onload
        base.OnLoad();
        // call oninit method for cleanliness
        Program.OnInit();
        // Post the loadgameevent to the event bus.
        Program.eventBus.Post(new Bus.LoadGameEvent());
        // Grab cursor
        CursorState = CursorState.Grabbed;
        // Clear window background color.
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    }

    // This is the constructor that initializes the game window. It is required for OpenTK.
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
    {
        Size = (width, height), // why is there a warning for deprecation... this is literally whats in the docs... ;-;
        Title = title
    })
    {
    }

    public void GetSize(out int width, out int height)
    {
        width = Size.X;
        height = Size.Y;
    }
}