using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PolyPanic.Bus
{
    // This event is posted every frame.
    // It contains the delta time since the last frame.
    public class UpdateFrameEvent
    {
        public float DeltaTime { get; }

        public UpdateFrameEvent(float deltaTime)
        {
            DeltaTime = deltaTime;
        }
    }

    // This event is posted when a keyboard key is pressed or released.
    // It contains the key and the action (pressed or released).
    // Potential issue: Pressed is repeatedly called. There is no held. I can work around it when it becomes important. 
    public class KeyboardEvent
    {
        public enum KeyAction
        {
            Pressed,
            Released
        }

        public Keys key { get; }
        public KeyAction action { get; }

        public KeyboardEvent(Keys key, KeyAction action)
        {
            this.key = key;
            this.action = action;
        }
    }
    // This is the mouse move event.
    // It contains the mouse position.
    public class MouseMoveEvent
    {
        public float X { get; }
        public float Y { get; }

        public MouseMoveEvent(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
    // This is the mouse press event.
    // It contains the mouse button and the action (pressed or released).
    public class MouseButtonEvent
    {
        public enum ButtonAction
        {
            Pressed,
            Released
        }

        public MouseButton Button { get; }
        public ButtonAction Action { get; }

        public MouseButtonEvent(MouseButton button, ButtonAction action)
        {
            Button = button;
            Action = action;
        }
    }
    // This is the load game event.
    // It's posted only when the game is first loaded.
    public class LoadGameEvent
    {
        // Nothing to grab from here!
    }

    // Resize Window Event
    // Contains the new width and height of the window.
    public class ResizeWindowEvent
    {
        public int Width { get; }
        public int Height { get; }

        public ResizeWindowEvent(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}