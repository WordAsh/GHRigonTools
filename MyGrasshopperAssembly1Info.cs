using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace MyGrasshopperAssembly1
{
    public class MyGrasshopperAssembly1Info : GH_AssemblyInfo
    {
        public override string Name => "RigonTools";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("e0078240-08a8-4309-8c3f-4a850c0638d2");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}