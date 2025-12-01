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

    private class ShaderDrawable : IDrawable
    {
        private readonly CoolShader _parent;

        public ShaderDrawable(CoolShader parent)
        {
            _parent = parent;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var time = _parent._time;
            var hue = (time * 0.1f) % 1.0f;
            var color1 = Color.FromHsla(hue, 0.8, 0.4, 1.0);
            var color2 = Color.FromHsla(hue + 0.3f, 0.7, 0.5, 1.0);
            var color3 = Color.FromHsla(hue + 0.6f, 0.6, 0.6, 1.0);
            var gradient = new LinearGradientBrush(
                new Point(0, 0),
                new Point(1, 1),
                new GradientStop[] {
                    new GradientStop(color1, 0.0f),
                    new GradientStop(color2, 0.5f),
                    new GradientStop(color3, 1.0f)
                });
            canvas.SetFillPaint(gradient, dirtyRect);
            canvas.FillRectangle(dirtyRect);
        }
    }
}