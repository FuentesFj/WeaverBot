using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using System.Drawing;
using Rhino.UI;
using Rhino.Input.Custom;

namespace WeaverBot.Rhino;
public class WeaverBotRun : Command
{
    public override string EnglishName => nameof(WeaverBotRun);

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        var panelId = WeaverBotPanel.PanelIdWeaverBot;

        if (Panels.IsPanelVisible(panelId) == false)
        {
            Panels.OpenPanel(panelId);
        }
        else
        {
            Panels.ClosePanel(panelId);
        }
        return Result.Success;
    }
}

public class HellloWorldWeaverBot : Command
{
    public override string EnglishName => nameof(HellloWorldWeaverBot);

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        RhinoApp.WriteLine("Hello world WeaverbotPlugin!");
        PluginInformationForm pluginInformationForm = new PluginInformationForm();
        pluginInformationForm.ShowForm();
        return Result.Success;
    }
}

public class WeaverBot_Horizontal : Command
{
    public override string EnglishName => nameof(WeaverBot_Horizontal);

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        //Selection of Breps
        var selectionResult = RhinoGet.GetMultipleObjects("Select Breps to generate weaving layout:", false, ObjectType.Brep, out var objectRefs);
        if (selectionResult != Result.Success)
        {
            RhinoApp.WriteLine("Please, select correctly the Breps");
            WeaverBotPanel.rhinoWeavingObjects = new List<RhinoObject>();
        }
        else
        {
            foreach (var objRef in objectRefs)
            {
                var selectedRHObject = objRef.Object();
                WeaverBotPanel.rhinoWeavingObjects.Add(selectedRHObject);
            }
        }
        
        #region Axis
        LineCurve axis = null;
        var axisOptionSelection = new GetOption();
        axisOptionSelection.SetCommandPrompt("Define the axis of the structure selecting a line or 2 points:");
        int optPoints = axisOptionSelection.AddOption("Points");
        int optLine = axisOptionSelection.AddOption("Line");

        //Check if there is no selection from the user
        GetResult getResult = axisOptionSelection.Get();
        if(getResult!= GetResult.Option)
        {
            RhinoApp.WriteLine("Please, select an option to generate the axis.");
            return Result.Cancel;
        }
        #region Axis.PointsOption
        if (axisOptionSelection.OptionIndex() == optPoints)
        {
            var result0=RhinoGet.GetPoint("Select 1st Axis Point",false, out var point0);
            if(result0 != Result.Success)
            {
                RhinoApp.WriteLine("Please, select the points to generate the axis.");
                return Result.Cancel;
            }
            var result1 = RhinoGet.GetPoint("Select 1st Axis Point", false, out var point1);
            if (result1 != Result.Success)
            {
                RhinoApp.WriteLine("Please, select the points to generate the axis.");
                return Result.Cancel;
            }
            axis = new LineCurve(point0, point1);
        }
        #endregion Axis.PointsOption
        #region Axis.LineOption
        if (axisOptionSelection.OptionIndex() == optLine)
        {
           var resultLineSelection = RhinoGet.GetLine(out var selectionline);
            if (resultLineSelection != Result.Success)
            {
                RhinoApp.WriteLine("Please, select a line");
                return Result.Cancel;
            }

            LineCurve linecurve = new LineCurve(selectionline);
            axis = linecurve;
        }
        #endregion Axis.LineOption
        #endregion Axis
        
       
        RhinoDoc.ActiveDoc.Objects.Add(axis);
        doc.Views.Redraw();

       // foreach (var objRef in WeaverBotPanel.rhinoWeavingObjects)
        {
            //




        }

        //WeaverBotPanel.rhinoWeavingObjects




        return Result.Success;
    }
}

