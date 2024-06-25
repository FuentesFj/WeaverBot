using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;
using RhinoUI = Rhino.UI;
using Rhino.UI.Controls;
using WeaverBot.Core;
using System.Net.NetworkInformation;
using Rhino.Input;
using Rhino.Commands;

namespace WeaverBot.Rhino;

[Guid("6504626A-EC4F-460F-BB0A-7D53979114BF")]
public class WeaverBotPanel : Panel
{
    //Geometry Information Properties
    private RadioButton horizontalRadioButton;
    private RadioButton pattern3DRadioButton;
    private GroupBox horizontalGroup;
    private GroupBox pattern3DGroup;

    //General Properties
    public static Guid PanelIdWeaverBot => typeof(WeaverBotPanel).GUID;
    public static RhinoObject rhWeavingObject = null;


    public WeaverBotPanel()
    {
        #region GeometryInfo
        //Initilialize RadioButtons
        horizontalRadioButton = new RadioButton { Text = "Horizontal" };
        pattern3DRadioButton = new RadioButton(horizontalRadioButton) { Text = "3D Pattern" };
        //Create & define Horizontall & Pattern 3D groups. 
        horizontalGroup = new GroupBox
        {
            Text = "Horizontal",
            Visible = false,
            Content = new RhinoPanelStackLayout
            {
                Items =
                {
                    new Button(EventSelectGeometry){Text="Select Geometry"},
                }
            }
        };
        pattern3DGroup = new GroupBox { Text = "3D Pattern", Visible = false };
        // Set up event handlers for radio buttons
        horizontalRadioButton.CheckedChanged += WeavingPatternRadioButton_CheckedChanged;
        pattern3DRadioButton.CheckedChanged += WeavingPatternRadioButton_CheckedChanged;
        //Layout
        var layout = new DynamicLayout { Padding = new Padding(10) };
        layout.AddSeparateRow("Weaving Pattern:", horizontalRadioButton, pattern3DRadioButton);
        layout.AddSeparateRow(horizontalGroup);
        layout.AddSeparateRow(pattern3DGroup);
        #endregion


        var expanderGeometryInformation = new Expander
        {
            Header = new Label { Text = "1. Geometry Information", Font = new Eto.Drawing.Font(FontFamilies.Sans, 10, FontStyle.Bold, FontDecoration.None) },
            Content = layout,
            Expanded = false
        };
        var expanderWeavingPatterns = new Expander
        {
            Header = new Label { Text = "2. Weaving Patterns", Font = new Eto.Drawing.Font(FontFamilies.Sans, 10, FontStyle.Bold, FontDecoration.None) },
            Expanded = false
        };



        Content = new RhinoPanelStackLayout
        {
            Padding = 10,
            Spacing = 10,
            Items =
            {
                new Label { Text = WeaverBot.Core.Util.PluginName,Font = new Eto.Drawing.Font(FontFamilies.Sans, 20, FontStyle.Bold, FontDecoration.None)},
                new Label { Text =$"v.{WeaverBot.Core.Util.PluginVersion}",Font = new Eto.Drawing.Font(FontFamilies.Sans, 8, FontStyle.Italic, FontDecoration.Underline)},
              
                expanderGeometryInformation,
                expanderWeavingPatterns
            }
        };
    }

    private void EventSelectGeometry(object sender, EventArgs e)
    {
        var selectionResult = RhinoGet.GetOneObject("Select a Brep to weave", false, ObjectType.Brep, out var objectRef);
        if (selectionResult != Result.Success)
        {
            RhinoApp.WriteLine("Please, select a single Brep");
        }
        else
        {
            var selectedRHObject = objectRef.Object();
            selectedRHObject = WeaverBotPanel.rhWeavingObject;
        }
    }

    private void WeavingPatternRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        if (horizontalRadioButton.Checked == true)
        {
            horizontalGroup.Visible = true;
            pattern3DGroup.Visible = false;
        }
        else if (pattern3DRadioButton.Checked == true)
        {
            horizontalGroup.Visible = false;
            pattern3DGroup.Visible = true;
        }
    }

}
