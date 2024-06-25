using System.Drawing;
using System.Runtime.InteropServices;
using Rhino.PlugIns;
using Rhino.UI;
using WeaverBot.Core;

[assembly: Guid("01BFB14B-CBC2-495C-816C-FD458B7AE5CE")]
[assembly: PlugInDescription(DescriptionType.Address, "")]
[assembly: PlugInDescription(DescriptionType.Country, "")]
[assembly: PlugInDescription(DescriptionType.Email, "fuentes.fj@hotmail.com")]
[assembly: PlugInDescription(DescriptionType.Phone, "")]
[assembly: PlugInDescription(DescriptionType.Fax, "")]
[assembly: PlugInDescription(DescriptionType.Organization, "")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "")]
[assembly: PlugInDescription(DescriptionType.WebSite, "www.fjfuentes.com")]
[assembly: PlugInDescription(DescriptionType.Icon, "WeaverBot.Rhino.icon.ico")]

namespace WeaverBot.Rhino;

public class WeaverBotPlugin : PlugIn
{
    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

    protected override LoadReturnCode OnLoad(ref string errorMessage)
    {
        Panels.RegisterPanel(this, typeof(WeaverBotPanel), Util.PluginName, GetIcon("icon.ico"), PanelType.PerDoc);
        //Panels.OpenPanel(typeof(WeaverBotPanel));
        return LoadReturnCode.Success;
    }

    static Icon GetIcon(string iconName)
    {
        var assembly = typeof(WeaverBotPlugin).Assembly;
        using var stream = assembly.GetManifestResourceStream($"WeaverBot.Rhino.{iconName}");
        return new(stream);
    }
}
