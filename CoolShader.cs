using Microsoft.Maui.Graphics;

namespace Phonebook;

public class CoolShader : GraphicsView
{
    private float _time = 0;

    public CoolShader()
    {
        Drawable = new ShaderDrawable(this);
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () =>
        {
            _time += 0.02f;
            Invalidate();
            return true;
        });
    }

    public float Time => _time;

    private class ShaderDrawable : IDrawable
    {
        private readonly CoolShader _parent;

        public ShaderDrawable(CoolShader parent)
        {
            _parent = parent;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var time = _parent.Time;
            var hue = (time * 0.1f) % 1.0f;

            var color1 = Color.FromHsla(hue, 0.8, 0.4, 1.0);
            var color2 = Color.FromHsla((hue + 0.3f) % 1.0f, 0.7, 0.5, 1.0);
            var color3 = Color.FromHsla((hue + 0.6f) % 1.0f, 0.6, 0.6, 1.0);

            // Исправленный способ создания градиента для ICanvas
            var gradient = new LinearGradientPaint
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new PaintGradientStop[]
                {
                    new PaintGradientStop(0.0f, color1),
                    new PaintGradientStop(0.5f, color2),
                    new PaintGradientStop(1.0f, color3)
                }
            };

            canvas.SetFillPaint(gradient, dirtyRect);
            canvas.FillRectangle(dirtyRect);
        }
    }
}