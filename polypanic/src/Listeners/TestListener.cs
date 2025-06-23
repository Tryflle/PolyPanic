using PolyPanic.Render.Font;

namespace PolyPanic.Debug
{
    class TestListener
    {
        [Bus.Subscribe]
        public void OnRenderFrame(Bus.UpdateFrameEvent e)
        {
            // Test font renderer (it works)
            FontRenderer.RenderText("POLYPANIC qwertyuiopasdfghjklzxcvbnm1234567890", 50, 50, 0.5f, new OpenTK.Mathematics.Vector3(1.0f, 1.0f, 1.0f));
        }
    }
}