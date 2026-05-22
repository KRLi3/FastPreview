using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Gradient;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace FastPreview
{
  public sealed class DualProfilerWidget : GH_Widget
  {
    private readonly List<RectangleF> _boxes = new();
    private static bool _visible = true;

    public override string Name => "Profiler";
    public override string Description => "Per-object compute time plus a single global display (preview-mesh) time.";
    public override string TooltipText => "Compute ms per object; one global display ms.";

    private static Bitmap _icon;
    public override Bitmap Icon_24x24 => _icon ??= new GH_ProfilerWidget().Icon_24x24;

    public override bool Visible { get => _visible; set => _visible = value; }

    public override bool Contains(Point pt_control, PointF pt_canvas)
    {
      foreach (var box in _boxes)
        if (box.Contains(pt_canvas)) return true;
      return false;
    }

    private static bool NativeProfilerOn => Instances.Settings.GetValue("Widget.Profiler.Show", false);
    private static int NativeThresholdMs => Instances.Settings.GetValue("Widget.Profiler.TimeThreshold", 5);

    public override void Render(GH_Canvas canvas)
    {
      _boxes.Clear();
      if (!_visible || !NativeProfilerOn) return;
      if (canvas.Viewport.Zoom <= GH_Viewport.ZoomDefault * 0.5f) return;

      var doc = canvas.Document;
      if (doc == null) return;

      int alpha = GH_Canvas.ZoomFadeLow;
      if (alpha == 0) return;

      canvas.SetSmartTextRenderingHint();
      var font = GH_FontServer.StandardBold;

      var grad = new GH_Gradient();
      grad.AddGrip(0.0, Color.FromArgb(alpha, 80, 200, 120));
      grad.AddGrip(0.5, Color.FromArgb(alpha, 255, 165, 0));
      grad.AddGrip(1.0, Color.FromArgb(alpha, 255, 60, 40));

      var span = doc.ObjectSpan;
      double spanTicks = span == TimeSpan.Zero ? 0 : span.Ticks;

      foreach (var obj in doc.Objects)
      {
        if (obj is not IGH_Component comp) continue;
        if (comp.Locked) continue;

        var rec = obj.Attributes.Bounds;
        if (!canvas.Viewport.IsVisible(ref rec, 50f)) continue;

        double computeMs = comp.ProcessorTime.TotalMilliseconds;
        if (computeMs <= NativeThresholdMs) continue;

        string text = $"{computeMs:0.00}ms";
        double frac = spanTicks > 0
          ? Math.Min(1.0, Math.Max(0.0, comp.ProcessorTime.Ticks / spanTicks))
          : 0.0;

        var size = GH_FontServer.MeasureString(text, font);
        float x = rec.Left;
        float y = rec.Top - size.Height;
        var at = new PointF(x, y);

        using (var brush = new SolidBrush(Color.FromArgb(alpha, grad.ColourAt(frac))))
          canvas.Graphics.DrawString(text, font, brush, at);

        _boxes.Add(new RectangleF(x, y, size.Width, size.Height));
      }

      DrawFrameReadout(canvas, alpha);
    }

    private static void DrawFrameReadout(GH_Canvas canvas, int alpha)
    {
      double ms = FastPreviewController.Parallel
        ? PreviewMerger.LastPrewarmMilliseconds
        : FrameProfiler.LastFrameMilliseconds;
      string label = ms < 0 ? "Display: measuring…" : $"Display: {ms:0.00} ms";

      var g = canvas.Graphics;
      var saved = g.Transform;
      g.ResetTransform();

      var font = GH_FontServer.StandardAdjusted;
      var size = g.MeasureString(label, font);
      var pad = 6f;
      var rect = new RectangleF(8f, 8f, size.Width + 2 * pad, size.Height + 2 * pad);

      using (var bg = new SolidBrush(Color.FromArgb(Math.Min(220, alpha + 60), 40, 40, 40)))
      using (var fg = new SolidBrush(Color.FromArgb(Math.Min(255, alpha + 80), 245, 245, 245)))
      {
        g.FillRectangle(bg, rect);
        g.DrawString(label, font, fg, rect.X + pad, rect.Y + pad);
      }

      g.Transform = saved;
    }

    public static void Install()
    {
      var canvas = Instances.ActiveCanvas;
      if (canvas?.Widgets == null) return;

      bool alreadyOurs = false;
      for (int i = canvas.Widgets.Count - 1; i >= 0; i--)
      {
        var w = canvas.Widgets[i];
        if (!string.Equals(w.Name, "Profiler", StringComparison.OrdinalIgnoreCase)) continue;
        if (w is DualProfilerWidget) { alreadyOurs = true; continue; }
        canvas.Widgets.RemoveAt(i);
      }

      if (!alreadyOurs)
      {
        var widget = new DualProfilerWidget { Owner = canvas };
        canvas.Widgets.Add(widget);
      }
      Instances.RedrawCanvas();
    }

    public static void Uninstall()
    {
      var canvas = Instances.ActiveCanvas;
      if (canvas?.Widgets == null) return;

      bool removed = false;
      for (int i = canvas.Widgets.Count - 1; i >= 0; i--)
      {
        if (canvas.Widgets[i] is DualProfilerWidget)
        {
          canvas.Widgets.RemoveAt(i);
          removed = true;
        }
      }

      if (removed)
      {
        bool hasNative = false;
        foreach (var w in canvas.Widgets)
          if (string.Equals(w.Name, "Profiler", StringComparison.OrdinalIgnoreCase)) { hasNative = true; break; }
        if (!hasNative)
          canvas.Widgets.Add(new GH_ProfilerWidget { Owner = canvas });
      }
      Instances.RedrawCanvas();
    }
  }
}
