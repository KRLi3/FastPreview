using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace FastPreview
{
  public static class PreviewMerger
  {
    private static readonly Dictionary<Type, MethodInfo> _createMeshMethods = new();

    public static double LastPrewarmMilliseconds { get; private set; } = -1;

    public static void Prewarm(GH_Document doc)
    {
      if (doc == null) return;
      var mp = doc.PreviewCurrentMeshParameters();
      if (mp == null) return;

      var targets = new List<IGH_Goo>();
      foreach (var obj in doc.Objects)
      {
        if (obj is not IGH_PreviewObject pv || pv.Hidden || !pv.IsPreviewCapable) continue;
        if (obj is IGH_ActiveObject ao && ao.Locked) continue;
        foreach (var p in EnumerateOutputParams(obj))
          foreach (var goo in p.VolatileData.AllData(true))
            if (NeedsPrewarm(goo))
              targets.Add(goo);
      }

      if (targets.Count == 0) { LastPrewarmMilliseconds = 0; return; }

      var sw = System.Diagnostics.Stopwatch.StartNew();
      Parallel.ForEach(targets, goo => TryCreatePreviewMesh(goo, mp));
      sw.Stop();
      LastPrewarmMilliseconds = sw.Elapsed.TotalMilliseconds;

      doc.ExpirePreview(false);
      Rhino.RhinoDoc.ActiveDoc?.Views.Redraw();
      Grasshopper.Instances.RedrawCanvas();
    }

    private static IEnumerable<IGH_Param> EnumerateOutputParams(IGH_DocumentObject obj)
    {
      switch (obj)
      {
        case IGH_Component comp:
          foreach (var p in comp.Params.Output) yield return p;
          break;
        case IGH_Param param:
          yield return param;
          break;
      }
    }

    private static bool NeedsPrewarm(IGH_Goo goo)
    {
      if (goo is not IGH_PreviewMeshData pmd) return false;
      if (goo is GH_Mesh) return false;
      return pmd.GetPreviewMeshes() == null;
    }

    private static void TryCreatePreviewMesh(IGH_Goo goo, MeshingParameters mp)
    {
      var type = goo.GetType();
      MethodInfo mi;
      lock (_createMeshMethods)
      {
        if (!_createMeshMethods.TryGetValue(type, out mi))
        {
          mi = type.GetMethod(
            "CreatePreviewMeshes",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            binder: null,
            types: new[] { typeof(MeshingParameters) },
            modifiers: null);
          _createMeshMethods[type] = mi;
        }
      }

      if (mi == null) return;
      try { mi.Invoke(goo, new object[] { mp }); }
      catch { }
    }
  }
}
