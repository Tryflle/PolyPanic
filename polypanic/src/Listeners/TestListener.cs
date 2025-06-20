namespace PolyPanic.Debug
{
    class TestListener
    {
        // This is a DEBUG listener. It is used to test events.
        // Working events:
        // - UpdateFrameEvent
        // - KeyboardEvent
        // Broken events:
        // NONE!!!!!
        
        [Bus.Subscribe]
        public void OnKeyboardEvent(Bus.KeyboardEvent e)
        {
            Console.WriteLine($"Keyboard Event: Key = {e.key}, Action = {e.action}");
        }
    }
}