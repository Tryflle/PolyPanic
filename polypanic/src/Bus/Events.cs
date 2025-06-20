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
}