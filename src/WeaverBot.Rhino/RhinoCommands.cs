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
using Rhino.Geometry.Intersect;
using System.Security.Cryptography;

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
        Result r1 = InformationInput(out var contourInterval);
        if (r1 != Result.Success)
        {
            WeaverBotPanel.rhinoWeavingObjects = new List<RhinoObject>();
            return r1;
        }

        Result r2 = GenerateAxisForStructure(out var axis);

        if (r2 != Result.Success)
        {
            WeaverBotPanel.rhinoWeavingObjects = new List<RhinoObject>();
            return r2;
        }

        //Generate an oriented Brep as a BoundingBox
        var orientedBrep = WeaverBot.Core.Util.GenerateOrientedBrep(WeaverBotPanel.rhinoWeavingObjects, axis);
        //Generate the Contours of the geometry and extract the biggest one.
        var r3 = GenerateContours(orientedBrep, axis, contourInterval, out Brep mainSectionBrep, out Guid mainSectionBrepGuid);
        if (r3 != Result.Success)
        {
            WeaverBotPanel.rhinoWeavingObjects = new List<RhinoObject>();
            return r3;
        }
        //Create the view
        var r4 = GenerateViews(mainSectionBrep,mainSectionBrepGuid);
        if (r4 != Result.Success)
        {
            WeaverBotPanel.rhinoWeavingObjects = new List<RhinoObject>();
            return r4;
        }


        WeaverBotPanel.rhinoWeavingObjects = new List<RhinoObject>();
        return Result.Success;
    }
    private Result InformationInput(out double contourInterval)
    {
        //Selection of Objects***********************************************************
        contourInterval = 60;
        var selectionResult = RhinoGet.GetMultipleObjects("Select Breps to generate weaving layout:", false, ObjectType.Brep, out var objectRefs);
        if (selectionResult != Result.Success)
        {
            RhinoApp.WriteLine("Please, select correctly the Breps");
            return Result.Cancel;
        }
        else
        {
            foreach (var objRef in objectRefs)
            {
                var selectedRHObject = objRef.Object();
                WeaverBotPanel.rhinoWeavingObjects.Add(selectedRHObject);
            }
        }
        //Contour Intervals***************************************************************
        var rc = RhinoGet.GetNumber("Contour interval", false, ref contourInterval);
        if (rc != Result.Success || contourInterval <= 0)
        {
            RhinoApp.WriteLine("Please, introduce a correct contour interval");
            return Result.Cancel;
        }
        return Result.Success;
    }
    private Result GenerateAxisForStructure(out LineCurve? axis)
    {
        axis = null;
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
        return Result.Success;
    }
    private Result GenerateContours(Brep orientedBrepAsBoundingBox, LineCurve axis, double contourInterval, out Brep mainSectionBrep, out Guid mainSectionBrepGuid)
    {

        #region General Variables
        var sectionPlanes = new List<Plane>();
        var sectionContours = new List<Curve>();
        var sectionBreps = new List<Brep>();
        var sectionSurfaces = new List<Surface>();
        var brepList = new List<Brep>();
        double maxArea = 0;
        mainSectionBrep = null;
        mainSectionBrepGuid = Guid.Empty;


        var joinedBrepList = new List<Brep>();
        var joinedBrep = new Brep();
        Vector3d axisVector = axis.PointAtEnd - axis.PointAtStart;
        double longestLength = 0;
        Line longestLineEdge = Line.Unset;
        Vector3d longestLineEdgeVector = Vector3d.Unset;

        #endregion

        #region Brep & Bounding Box
        //Find the edges of the Brep that are aligned to the contour axis and get the longest one.
        foreach (var edge in orientedBrepAsBoundingBox.Edges)
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

        //Transform RhinoObject to Breps *****************************************
        var initialBrepContourList = new List<Brep>();
        //Try to transform the Rhino Objects
        foreach (var rhObject in WeaverBotPanel.rhinoWeavingObjects)
        {
            var brep = Brep.TryConvertBrep(rhObject.Geometry);

            if (brep == null)
            {
                RhinoApp.WriteLine("Rhino fails to transform one selected Object to Brep. Please, be sure all the elements are produced with clean geometry");
                return Result.Cancel;
            }
            initialBrepContourList.Add(brep);
        }




        //BooleanUnion of Breps **************************************************
        if (initialBrepContourList.Count < 2)
        {
            joinedBrep = joinedBrepList[0];
        }
        else
        {
            var brepArray = Brep.CreateBooleanUnion(initialBrepContourList, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            joinedBrepList = brepArray.ToList();

            if (joinedBrepList.Count > 1 || joinedBrepList == null)
            {
                RhinoApp.WriteLine("Elements of your structure are not adjacent or there is a problem to generate a single Brep out of them.Please be sure " +
                                    "your geometry can be transformed to a single brep");
                return Result.Cancel;
            }
            joinedBrep = joinedBrepList[0];
        }
        //Merge All Coplanar Faces **********************************************
        if (!joinedBrep.MergeCoplanarFaces(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance))
        {
            RhinoApp.WriteLine("MergeAllCoplanarFaces couldn't be done but the program will continue");
        }


        #endregion

        #region Sections + Find 1 Brep with the biggest area
        //Generate the section planes *****************************************************************
        longestLineEdgeVector = longestLineEdge.PointAtLength(longestLineEdge.Length) - longestLineEdge.PointAtLength(0.00);
        longestLineEdgeVector.Unitize();
        for (double d = 0; d <= longestLength; d += contourInterval)
        {
            Plane contourPlane = new Plane((longestLineEdge.PointAtLength(0.00) + longestLineEdgeVector * d), longestLineEdgeVector);
            sectionPlanes.Add(contourPlane);
        }
        //Intersect the planes with the Brep.
        foreach (var plane in sectionPlanes)
        {
            var tester = Intersection.BrepPlane(joinedBrep, plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out var curves, out var intersectionPoints);
            sectionContours.AddRange(curves);
        }
        var curvesJoined = Curve.JoinCurves(sectionContours, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToList();
        sectionBreps = Brep.CreatePlanarBreps(curvesJoined, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).ToList();

        //Find the biggest Brep intersection and get the surface to establish the camera perpendicular later.

        foreach (var brep in sectionBreps)
        {
            if (brep.GetArea() > maxArea)
            {
                maxArea = brep.GetArea();
                mainSectionBrep = brep;             
            }
        }

        #endregion
        //Add the Surface to work with into Rhino(temporary).Later I will delete it using the Guid.
        ObjectAttributes objectAttributes = new ObjectAttributes();
        objectAttributes.ObjectColor = Color.Red;
        objectAttributes.ColorSource = ObjectColorSource.ColorFromObject;
        objectAttributes.Name = "MainSectionSurface";

        mainSectionBrepGuid = RhinoDoc.ActiveDoc.Objects.Add(mainSectionBrep, objectAttributes);

        RhinoDoc.ActiveDoc.Views.Redraw();
        return Result.Success;
    }
    private Result GenerateViews(Brep mainSectionBrep, Guid mainSectionBrepGuid)
    {
        RhinoDoc doc = RhinoDoc.ActiveDoc;
        //Get the surface from the brep and then get the plane
        Surface surfaceFromMainSectionBrep = mainSectionBrep.Faces[0];

        // Get the plane on the surface
        Plane surfacePlane;
        if (!surfaceFromMainSectionBrep.TryGetPlane(out surfacePlane))
        {
            RhinoApp.WriteLine("Failed to get a plane from the surface.");
            return Result.Failure;
        }

        // Create a new view and set its projection
        var surfaceView = doc.Views.Add("Surface View", DefinedViewportProjection.Top, new System.Drawing.Rectangle(0, 0, 800, 600), true);
        if (surfaceView == null)
        {
            RhinoApp.WriteLine("Failed to create a new view.");
            return Result.Failure;
        }

        // Set the construction plane on the surface
        surfaceView.ActiveViewport.SetConstructionPlane(surfacePlane);

        // Orient the view perpendicular to the surface
        surfaceView.ActiveViewport.SetCameraLocation(surfacePlane.Origin + surfacePlane.ZAxis, true);
        surfaceView.ActiveViewport.SetCameraDirection(-surfacePlane.ZAxis, true);
        surfaceView.ActiveViewport.CameraUp = surfacePlane.YAxis;

        // Hide all objects except the mainSectionBrep

        
        foreach (var obj in doc.Objects)
        {
            if (obj.Id != mainSectionBrepGuid)
            {
                obj.Attributes.Visible = false;
                obj.CommitChanges();
            }
        }
        doc.Views.Redraw();
        // Adjust the zoom to fit the surface in the view
        surfaceView.ActiveViewport.ZoomBoundingBox(mainSectionBrep.GetBoundingBox(true));
        return Result.Success;
    }



    public class Weaverbot_Trial : Command
    {
        public override string EnglishName => nameof(Weaverbot_Trial);

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoGet.GetOneObject("Select the brep", false, ObjectType.Brep, out var rhObjectRef);
            var selectedBrep = rhObjectRef.Object().Geometry as Brep;

            RhinoGet.GetMultipleObjects("Select the planes", false, ObjectType.Surface, out var surfaceObjectRefArray);
            var brepList = surfaceObjectRefArray.ToList().Select(x => (Brep)x.Object().Geometry).ToList();

            var planeList = new List<Plane>();

            foreach (var brep in brepList)
            {
                foreach (var face in brep.Faces)
                {
                    var tester = face.UnderlyingSurface().TryGetPlane(out var plane);
                    planeList.Add(plane);
                }
            }

            //Intersect the planes with the Brep and create the surfaces to intersect them.
            var intersectionCurves = new List<Curve>();
            foreach (var plane in planeList)
            {
                Intersection.BrepPlane(selectedBrep, plane, doc.ModelAbsoluteTolerance, out var curves, out var intersectionPoints);
                intersectionCurves.AddRange(curves);
            }
            var curvesJoined = Curve.JoinCurves(intersectionCurves, doc.ModelAbsoluteTolerance);
            var brepSurfacesList = new List<Brep>();
            foreach (var joinedCurve in curvesJoined)
            {
                if (joinedCurve.IsClosed)
                {
                    brepSurfacesList.AddRange(Brep.CreatePlanarBreps(joinedCurve, doc.ModelAbsoluteTolerance));
                }
            }


            //Find the biggest surface intersection.
            double maxArea = 0;
            Brep mainSectionBrep = null;

            foreach (var brep in brepSurfacesList)
            {
                // brep.Faces;



            }



            foreach (var brepSurface in brepSurfacesList)
            {
                if (brepSurface.GetArea() > maxArea)
                {
                    maxArea = brepSurface.GetArea();
                    mainSectionBrep = brepSurface;
                };
                //Temporary to check everything is working
                doc.Objects.Add(brepSurface);
                doc.Views.Redraw();
            }

            //Find a camera that is perp.to the surface and open in a new window
            var centroid = AreaMassProperties.Compute(mainSectionBrep).Centroid;

            return Result.Success;
        }
    }


    public class Weaverbot_Selection : Command
    {
        public override string EnglishName => nameof(Weaverbot_Selection);

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoGet.GetMultipleObjects("Select objects", false, ObjectType.Brep, out var objects);

            foreach (var obj in objects)
            {
                RhinoApp.WriteLine($"The object is {obj.Object().Geometry.ToString()}");
            }
            return Result.Success;
        }
    }
}

