using System;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace FastPreview
{
  public sealed class FastPreviewAssemblyPriority : GH_AssemblyPriority
  {
    public override GH_LoadingInstruction PriorityLoad()
    {
      Instances.CanvasCreated += OnCanvasCreated;
      return GH_LoadingInstruction.Proceed;
    }

    private static void OnCanvasCreated(GH_Canvas canvas)
    {
      Instances.CanvasCreated -= OnCanvasCreated;
      FastPreviewController.Initialize();
      FastPreviewMenu.TryInstall();
    }
  }

  internal static class FastPreviewMenu
  {
    private static bool _installed;
    private static ToolStripMenuItem _parallelItem;
    private static ToolStripMenuItem _profilerItem;

    public static void TryInstall()
    {
      if (_installed) return;
      var editor = Instances.DocumentEditor;
      if (editor == null) return;

      var display = FindDisplayMenu(editor);
      if (display == null) return;

      _parallelItem = new ToolStripMenuItem("Parallel Preview")
      {
        Image = FastPreviewIcons.Parallel,
        ToolTipText = "Pre-compute Brep/Surface preview meshes in parallel after each solution "
                    + "so the first viewport draw is faster. Uses the document's mesh settings."
      };
      _parallelItem.Click += (s, e) => FastPreviewController.ToggleParallel();

      _profilerItem = new ToolStripMenuItem("Advanced Profiler")
      {
        Image = FastPreviewIcons.Profiler,
        ToolTipText = "Replace GH's profiler with one that also shows the first-frame display "
                    + "(preview-meshing) time."
      };
      _profilerItem.Click += (s, e) => FastPreviewController.ToggleProfiler();

      display.DropDownOpening += (s, e) =>
      {
        _parallelItem.Checked = FastPreviewController.Parallel;
        _profilerItem.Checked = FastPreviewController.Profiler;
      };

      display.DropDown.Items.Add(new ToolStripSeparator());
      display.DropDown.Items.Add(_parallelItem);
      display.DropDown.Items.Add(_profilerItem);

      _installed = true;
    }

    private static ToolStripMenuItem FindDisplayMenu(GH_DocumentEditor editor)
    {
      foreach (var strip in editor.Controls.OfType<MenuStrip>())
      {
        var item = FindByText(strip.Items, "Display");
        if (item != null) return item;
      }
      if (editor.MainMenuStrip != null)
      {
        var item = FindByText(editor.MainMenuStrip.Items, "Display");
        if (item != null) return item;
      }
      return null;
    }

    private static ToolStripMenuItem FindByText(ToolStripItemCollection items, string text)
    {
      foreach (ToolStripItem it in items)
        if (it is ToolStripMenuItem mi &&
            string.Equals(mi.Text?.Replace("&", ""), text, StringComparison.OrdinalIgnoreCase))
          return mi;
      return null;
    }
  }
}
