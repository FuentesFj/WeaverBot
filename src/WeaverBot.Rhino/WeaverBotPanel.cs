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
using System.Diagnostics;


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
    public static List <RhinoObject> rhinoWeavingObjects = new List <RhinoObject> ();
    public static RhinoObject? rhWeavingObject = null;


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
            Header = new Label { Text = "1.Geometry Information", Font = new Eto.Drawing.Font(FontFamilies.Sans, 10, FontStyle.Bold, FontDecoration.None) },
            Content = layout,
            Expanded = false
        };
        var expanderWeavingPatterns = new Expander
        {
            Header = new Label { Text = "2.Weaving Patterns", Font = new Eto.Drawing.Font(FontFamilies.Sans, 10, FontStyle.Bold, FontDecoration.None) },
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
                expanderWeavingPatterns,
                new Button(EventPluginInformation){Text="More Info"}
            }
        };
    }

    private void EventPluginInformation(object sender, EventArgs e)
    {
        PluginInformationForm pluginInformationForm = new PluginInformationForm();
        pluginInformationForm.ShowForm();
    }

    private void EventSelectGeometry(object sender, EventArgs e)
    {
        var selectionResult = RhinoGet.GetMultipleObjects("Select Breps to generate weaving layout:", false, ObjectType.Brep, out var objectRefs);
        if (selectionResult != Result.Success)
        {
            RhinoApp.WriteLine("Please, select correctly the Breps");
        }
        else
        {
            foreach (var objRef in objectRefs)
            {
                var selectedRHObject = objRef.Object();
                WeaverBotPanel.rhinoWeavingObjects.Add(selectedRHObject);
            }
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



public class PluginInformationForm : Form
{
    private static PluginInformationForm? instance;

    public PluginInformationForm()
    {
        Title = "Weavebot - Plugin Information";
        ClientSize = new Size(400, 300);
        Resizable = false;

        //Title & Version
        var title = new Label { Text = "WeaverBot Plugin", Font = new Eto.Drawing.Font(FontFamilies.Sans, 14, FontStyle.Bold) };
        var version = new Label { Text = $"Version: {WeaverBot.Core.Util.PluginVersion}" };


        //Logos
        var ddfLogo = new ImageView { Image = Util.GetEtoBitmapIcon("DDF_Logo.png", "WeaverBot.Rhino", typeof(WeaverBotPlugin)) };
        var kitLogo = new ImageView { Image = Util.GetEtoBitmapIcon("KIT_Logo.png", "WeaverBot.Rhino", typeof(WeaverBotPlugin)) };
        var gitHubLogo = new ImageView { Image = Util.GetEtoBitmapIcon("Github_Logo.png", "WeaverBot.Rhino", typeof(WeaverBotPlugin)) };
        //var instaLogo = new ImageView { Image = Util.GetEtoBitmapIcon("Insta_Logo.png", "WeaverBot.Rhino", typeof(WeaverBotPlugin)) };


        //AuthorLinkButton + Event:
        var authorLink = new LinkButton { Text = "Javier Fuentes Quijano" };
        authorLink.Click += (sender, e) => Process.Start(new ProcessStartInfo { FileName = "https://www.linkedin.com/in/javier-fuentes-quijano/", UseShellExecute = true });
        //StackLayout to combine LabelAuthor + AuthorLink
        var authorLayout = new StackLayout { Orientation = Orientation.Horizontal, Spacing = 0 };//To combine authorLayout & authorLink
        authorLayout.Items.Add(new Label { Text = "Developer:" });
        authorLayout.Items.Add(authorLink);
        //DDFButton + Event
        var ddfLink = new LinkButton { Text = "Digital Design & Fabrication Professorship" };
        ddfLink.Click += (sender, e) => Process.Start(new ProcessStartInfo { FileName = "https://www.ddf-kit.de/", UseShellExecute = true });
        //KitButton + Event
        var kitLink = new LinkButton { Text = "Karlsruhe Insitute of Technology" };
        ddfLink.Click += (sender, e) => Process.Start(new ProcessStartInfo { FileName = "https://www.kit.edu/english/", UseShellExecute = true });

        //GitHubButton+Event
        var gitHubLink = new LinkButton { Text = "GitHub Repository" };
        gitHubLink.Click += (sender, e) => Process.Start(new ProcessStartInfo { FileName = "https://github.com/FuentesFj/WeaverBot", UseShellExecute = true });
        //Instagram+Event
        //var instaLink = new LinkButton { Text = "Instagram profile" };
        //instaLink.Click += (sender, e) => Process.Start(new ProcessStartInfo { FileName = "https://www.instagram.com/ddf.kit/", UseShellExecute = true });


        //Text explaining;
        var text = new Label
        {
            Text = "WeaverBot plugin automates the design and\n" +
                   "fabrication of woven construction components\n" +
                   "using natural fibers. This plugin is part\n" +
                   "of the PhD thesis of Javier Fuentes for DDF\n" +
                   "proffeshorsip. Karlsruhe Institute of Technology"
        };


        var panelLayout = new PixelLayout();
        panelLayout.Add(title, 10, 10);
        panelLayout.Add(version, 20, 35);
        panelLayout.Add(authorLayout, 20, 70);
        panelLayout.Add(ddfLink, 20, 90);
        panelLayout.Add(kitLink, 20, 110);
        panelLayout.Add(gitHubLink, 15, 270);
        //panelLayout.Add(instaLink, 300, 275);
        panelLayout.Add(text, 20, 140);
        panelLayout.Add(ddfLogo, 270, 5);
        panelLayout.Add(kitLogo, 270, 80);
        panelLayout.Add(gitHubLogo, 45, 230);
        //panelLayout.Add(instaLogo, 300, 235);


        Content = panelLayout;
        //When closing the windows, instance = null.
        Closed += (sender, e) => instance = null;
    }
    /// <summary>
    /// Makes that only is possible to have 1 window and no multiple ones. If there is already a window
    /// open and the user press again the Button "More Info", then the window is bringed to the front.
    /// Otherwise, create a new instance of the window
    /// </summary>
    public void ShowForm()
    {
        if (instance == null)
        {
            instance = new PluginInformationForm();
            instance.Show();
        }
        else
        {
            instance.BringToFront();
        }
    }


}
