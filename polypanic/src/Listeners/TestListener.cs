using OpenTK.Mathematics;
using PolyPanic.Render.Font;

namespace PolyPanic.Debug
{
    class TestListener
    {

        [Bus.Subscribe]
        public void OnRenderFrame(Bus.UpdateFrameEvent e)
        {
            FontRenderer.RenderText("POLYPANIC qwertyuiopasdfghjklzxcvbnm1234567890", 50, 50, 0.5f, new Vector3(1.0f, 1.0f, 1.0f));
            FontRenderer.RenderText("FPS: " + (1.0f / e.DeltaTime).ToString("F2"), 50, 1000, 0.5f, new Vector3(1.0f, 0.0f, 0.0f));
        }
    }
}