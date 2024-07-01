
using Rhino.Geometry;
using System.Drawing;



namespace WeaverBot.Core
{
    public static class Util
    {
        public static readonly string PluginName = "WeaverBot";
        public static readonly string PluginVersion = "0.01";



        public static System.Drawing.Bitmap GetBitmapIcon(string imageIconName,string nameOfCSharpProject, Type AssemblyClass)
        {
            var assembly = AssemblyClass.Assembly;
            using var stream = assembly.GetManifestResourceStream($"{nameOfCSharpProject}.{imageIconName}");
            if (stream == null)
            {
                throw new ArgumentException($"Resource {imageIconName} not found in assembly {assembly.FullName}");
            }
            Bitmap bitmap = new(stream);
            return new (bitmap);
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




    }
}
