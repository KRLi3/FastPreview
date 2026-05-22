using System;
using Rhino.Display;

namespace FastPreview
{
  public sealed class FrameProfiler
  {
    private static FrameProfiler _instance;

    private const double WindowMs = 4000;
    private System.Diagnostics.Stopwatch _window;
    private double _maxMs;
    private System.Diagnostics.Stopwatch _sw;

    public static double LastFrameMilliseconds { get; private set; } = -1;

    public static void Enable()
    {
      if (_instance != null) return;
      _instance = new FrameProfiler();
      DisplayPipeline.PreDrawObjects += _instance.OnPreDraw;
      DisplayPipeline.PostDrawObjects += _instance.OnPostDraw;
    }

    public static void Disable()
    {
      if (_instance == null) return;
      DisplayPipeline.PreDrawObjects -= _instance.OnPreDraw;
      DisplayPipeline.PostDrawObjects -= _instance.OnPostDraw;
      _instance = null;
      LastFrameMilliseconds = -1;
    }

    public static void StartWindow()
    {
      if (_instance == null) return;
      _instance._maxMs = 0;
      _instance._window = System.Diagnostics.Stopwatch.StartNew();
    }

    private void OnPreDraw(object sender, DrawEventArgs e)
    {
      if (_window == null) return;
      _sw = System.Diagnostics.Stopwatch.StartNew();
    }

    private void OnPostDraw(object sender, DrawEventArgs e)
    {
      if (_window == null || _sw == null) return;
      _sw.Stop();
      double ms = _sw.Elapsed.TotalMilliseconds;
      _sw = null;

      if (ms > _maxMs)
      {
        _maxMs = ms;
        LastFrameMilliseconds = _maxMs;
        Grasshopper.Instances.RedrawCanvas();
      }

      if (_window.Elapsed.TotalMilliseconds >= WindowMs)
        _window = null;
    }
  }
}
