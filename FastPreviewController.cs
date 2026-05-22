using System;
using System.IO;
using System.Reflection;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace FastPreview
{
  public static class FastPreviewController
  {
    private static bool _initialized;
    private static bool _parallel;
    private static bool _profiler;
    private static bool _loaded;
    private static GH_Document _boundDoc;

    private static string SettingsPath
    {
      get
      {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return dir == null ? null : Path.Combine(dir, "FastPreview.settings.txt");
      }
    }

    public static bool Parallel
    {
      get { EnsureLoaded(); return _parallel; }
      private set { EnsureLoaded(); _parallel = value; Save(); }
    }

    public static bool Profiler
    {
      get { EnsureLoaded(); return _profiler; }
      private set { EnsureLoaded(); _profiler = value; Save(); }
    }

    private static void EnsureLoaded()
    {
      if (_loaded) return;
      _loaded = true;
      try
      {
        var path = SettingsPath;
        if (path == null || !File.Exists(path)) return;
        foreach (var line in File.ReadAllLines(path))
        {
          var i = line.IndexOf('=');
          if (i <= 0) continue;
          var key = line.Substring(0, i).Trim();
          var val = line.Substring(i + 1).Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
          if (key == "Parallel") _parallel = val;
          else if (key == "Profiler") _profiler = val;
        }
      }
      catch { }
    }

    private static void Save()
    {
      try
      {
        var path = SettingsPath;
        if (path == null) return;
        File.WriteAllText(path, $"Parallel={_parallel}\nProfiler={_profiler}\n");
      }
      catch { }
    }

    public static void Initialize()
    {
      if (_initialized) return;
      _initialized = true;

      var canvas = Instances.ActiveCanvas;
      if (canvas != null) canvas.DocumentChanged += (s, e) => Bind(e.NewDocument);

      if (Profiler) { DualProfilerWidget.Install(); FrameProfiler.Enable(); }
      Bind(canvas?.Document);
    }

    public static void ToggleParallel() => SetParallel(!Parallel);
    public static void ToggleProfiler() => SetProfiler(!Profiler);

    public static void SetParallel(bool on)
    {
      Parallel = on;
      RefreshNow();
    }

    public static void SetProfiler(bool on)
    {
      Profiler = on;
      if (on) { DualProfilerWidget.Install(); FrameProfiler.Enable(); }
      else { FrameProfiler.Disable(); DualProfilerWidget.Uninstall(); }
      RefreshNow();
    }

    private static void Bind(GH_Document doc)
    {
      if (ReferenceEquals(_boundDoc, doc)) return;
      if (_boundDoc != null) _boundDoc.SolutionEnd -= OnSolutionEnd;
      _boundDoc = doc;
      if (doc == null) return;
      doc.SolutionEnd += OnSolutionEnd;

      if (doc.SolutionDepth == 0 && doc.SolutionSpan > TimeSpan.Zero)
        RefreshNow();
    }

    private static void OnSolutionEnd(object sender, GH_SolutionEventArgs e)
    {
      if (Parallel) PreviewMerger.Prewarm(_boundDoc);
      if (Profiler) FrameProfiler.StartWindow();
    }

    private static void RefreshNow()
    {
      if (_boundDoc == null) return;
      if (Parallel) PreviewMerger.Prewarm(_boundDoc);
      if (Profiler)
      {
        FrameProfiler.StartWindow();
        if (!Parallel) Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();
      }
    }
  }
}
