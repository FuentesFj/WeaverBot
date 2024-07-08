
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Drawing;



namespace WeaverBot.Core
{
    public static class Util
    {
        public static readonly string PluginName = "WeaverBot";
        public static readonly string PluginVersion = "0.01";



        public static System.Drawing.Bitmap GetBitmapIcon(string imageIconName, string nameOfCSharpProject, Type AssemblyClass)
        {
            var assembly = AssemblyClass.Assembly;
            using var stream = assembly.GetManifestResourceStream($"{nameOfCSharpProject}.{imageIconName}");
            if (stream == null)
            {
                throw new ArgumentException($"Resource {imageIconName} not found in assembly {assembly.FullName}");
            }
            Bitmap bitmap = new(stream);
            return new(bitmap);
        }
        public static System.Drawing.Icon GetIcon(string iconName, string nameOfCSharpProject, Type AssemblyClass)
        {
            var assembly = AssemblyClass.Assembly;
            using var stream = assembly.GetManifestResourceStream($"{nameOfCSharpProject}.{iconName}");
            if (stream == null)
            {
                throw new ArgumentException($"Resource {iconName} not found in assembly {assembly.FullName}");
            }
            return new Icon(stream);
        }

        public static Eto.Drawing.Bitmap GetEtoBitmapIcon(string imageIconName, string nameOfCSharpProject, Type AssemblyClass)
        {
            var assembly = AssemblyClass.Assembly;
            using var stream = assembly.GetManifestResourceStream($"{nameOfCSharpProject}.{imageIconName}");
            if (stream == null)
            {
                throw new ArgumentException($"Resource {imageIconName} not found in assembly {assembly.FullName}");
            }
            var bitmap = new Eto.Drawing.Bitmap(stream);
            return new(bitmap);
        }

        public static Brep GenerateOrientedBrep(List<RhinoObject> objects, LineCurve axis)
        {

            BoundingBox worldOrientedBbox = new BoundingBox();
            Vector3d vectorAxis = axis.PointAtEnd - axis.PointAtStart;

            //Obtain the oriented plane and ensure Y Axis of the plane == Z World Axis
            Plane orientedPlane = new Plane(axis.PointAtStart, vectorAxis);
            double rotationAngle = Vector3d.VectorAngle(orientedPlane.YAxis, Vector3d.ZAxis, orientedPlane);
            Transform rotationTransform = Transform.Rotation(rotationAngle, vectorAxis, orientedPlane.Origin);
            orientedPlane.Transform(rotationTransform);

            //Obtain the transformation to orient objects into the YZ Plane

            Transform orientedPlaneToWolrdPlane_Transformation = Transform.PlaneToPlane(orientedPlane, Plane.WorldYZ);
            Transform worldPlaneToOrientedPlane_Transformation = Transform.PlaneToPlane(Plane.WorldYZ, orientedPlane);

            foreach (var obj in objects)
            {
                var objGeo = obj.Geometry;
                objGeo.Transform(orientedPlaneToWolrdPlane_Transformation);
                //RhinoDoc.ActiveDoc.Objects.Add(objGeo);
                var partialWorldBox = objGeo.GetBoundingBox(true);
                worldOrientedBbox.Union(partialWorldBox);
            }
          
            var brep = worldOrientedBbox.ToBrep();
            var transformResult=brep.Transform(worldPlaneToOrientedPlane_Transformation);
            if (transformResult != true)
            {
                RhinoApp.WriteLine("Brep couldn't be transformed");
            }
            return brep;
        }



    }
}
