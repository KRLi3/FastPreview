using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace FastPreview
{
  public class FastPreviewInfo : GH_AssemblyInfo
  {
    public override string Name => "FastPreview";
    public override Bitmap Icon => FastPreviewIcons.Assembly;
    public override string Description => "Parallel preview-mesh pre-warming and a compute/display profiler for Grasshopper.";
    public override Guid Id => new Guid("a9bdc006-fd75-4293-9cf1-9fc9f2cfffe3");
    public override string AuthorName => "KR";
    public override string AuthorContact => "krli980817@gmail.com";
    public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
  }
}
