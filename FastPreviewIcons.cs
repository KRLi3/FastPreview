using System;
using System.Drawing;
using System.Reflection;

namespace FastPreview
{
  internal static class FastPreviewIcons
  {
    private static Bitmap _assembly;
    private static Bitmap _parallel;
    private static Bitmap _profiler;

    public static Bitmap Assembly => _assembly ??= Load("FastPreview_24x24.png");
    public static Bitmap Parallel => _parallel ??= Load("FastPreview_16x16.png");
    public static Bitmap Profiler => _profiler ??= Load("Profiler_16x16.png");

    private static Bitmap Load(string fileName)
    {
      var asm = typeof(FastPreviewIcons).Assembly;
      var resourceName = asm.GetName().Name + ".Resources." + fileName;
      using var stream = asm.GetManifestResourceStream(resourceName);
      if (stream == null) return null;
      return new Bitmap(stream);
    }
  }
}
