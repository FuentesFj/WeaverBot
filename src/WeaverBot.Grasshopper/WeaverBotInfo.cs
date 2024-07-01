using Grasshopper.Kernel;
using WeaverBot.Core;
using System.Drawing;

namespace WeaverBot.Grasshopper;

public class WeaverBotInfo : GH_AssemblyInfo
{
    public override string Name => Util.PluginName;
    public override Bitmap Icon => Util.GetBitmapIcon("icon.png","WeaverBot.Grasshopper",typeof(WeaverBotInfo));
    public override Guid Id => new("C1522934-0165-410E-A38E-38A3D4A702F2");
    public override string AssemblyVersion => Util.PluginVersion;
    public override string AuthorName => "Javier Fuentes Quijano";
    public override string AuthorContact => "fuentes.fj@hotmail.com";
    public override string Description => "WeaverBot Plugin. Developer: Javier Fuentes";
    public override GH_LibraryLicense License => GH_LibraryLicense.commercial;
}
