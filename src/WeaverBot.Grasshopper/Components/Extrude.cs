using System.Drawing;
using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using WeaverBot.Core;

namespace WeaverBot.Grasshopper;

public class ExtrudeCurve : GH_Component
{
    public ExtrudeCurve() : base("Extrude Curve", "ExtrudeCurve", "Extrudes curves.", Util.PluginName, "Components") { }
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override Guid ComponentGuid => new("8D0F7963-ABE3-41F9-9F52-C4E1470C77A9");
    protected override Bitmap Icon => WeaverBotInfo.GetIcon();

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddCurveParameter("Curve", "C", "Curve to extrude", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddBrepParameter("Brep", "B", "Extruded brep.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      
    }
}
