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
        LineCurve? axis = null;
        var axisOptionSelection = new GetOption();
        axisOptionSelection.SetCommandPrompt("Define the axis of the structure selecting a line or 2 points:");
        int optPoints = axisOptionSelection.AddOption("Points");
        int optLine = axisOptionSelection.AddOption("Line");

        //Check if there is no selection from the user
        GetResult getResult = axisOptionSelection.Get();
        if (getResult != GetResult.Option)
        {
            RhinoApp.WriteLine("Please, select an option to generate the axis.");
            return Result.Cancel;
        }
        #region Axis.PointsOption
        if (axisOptionSelection.OptionIndex() == optPoints)
        {
            var result0 = RhinoGet.GetPoint("Select 1st Axis Point", false, out var point0);
            if (result0 != Result.Success)
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

        double contourInterval = 20;
        var rc = RhinoGet.GetNumber("Contour interval", false, ref contourInterval);
        if (rc != Result.Success || contourInterval <= 0)
        {
            RhinoApp.WriteLine("Please, introduce a correct contour interval");
            return Result.Cancel;
        }

        #region GenerateContours
        //Variables Required
        Vector3d axisVector = axis.PointAtEnd - axis.PointAtStart;
        double longestLength = 0;
        Line longestLineEdge = Line.Unset;

        //Generate an oriented Brep as a Bounding Box.
        var brep = WeaverBot.Core.Util.GenerateOrientedBrep(WeaverBotPanel.rhinoWeavingObjects, axis);
        //Find the edges of the Brep that are aligned to the contour axis and get the longest one.
        foreach (var edge in brep.Edges)
        {
            var edgeCurve = edge.EdgeCurve;
            if (edgeCurve == null)
            {
                continue;
            }
            Point3d start = edgeCurve.PointAtStart;
            Point3d end = edgeCurve.PointAtEnd;
            // Create a line from the start and end points
            Line edgeLine = new Line(start, end);
            // Get the direction vector of the edge
            Vector3d edgeDirection = edgeLine.Direction;
            //Check if it is parallel to the axis
            if (edgeDirection.IsParallelTo(axisVector) != 0)
            {
                double edgeLength = edgeLine.Length;
                if (edgeLength > longestLength)
                {

                    longestLength = edgeLength;
                    longestLineEdge = edgeLine;
                    //If the line of the boundingbox is in the opposite direction as the axis, flip the boundingBox line
                    if (edgeDirection.IsParallelTo(axisVector) == -1)
                    {
                        longestLineEdge.Flip();
                    }
                }
            }
        }

        //Generate the section planes



        var geometryBaseContourList = new List<GeometryBase>();
        foreach (var rhObject in WeaverBotPanel.rhinoWeavingObjects)
        {
            var rhGeometryBase = rhObject.Geometry;
            geometryBaseContourList.Add(rhGeometryBase);
        }

        WeaverBotPanel.rhinoWeavingObjects = new List<RhinoObject>();


        #endregion

        return Result.Success;
    }
}

