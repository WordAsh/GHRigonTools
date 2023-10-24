using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace MyGrasshopperAssembly1.Curve
{
    public class Spiral : GH_Component
    {
        public Spiral()
          : base("Create spiral", "ASpi",
            "Create spiral",
            "RigonTools", "Curve")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

            pManager.AddPlaneParameter("Plane", "P", "Base plane for spiral", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Inner Radius", "R0", "Inner radius for spiral", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Outer Radius", "R1", "Outer radius for spiral", GH_ParamAccess.item, 10.0);
            pManager.AddIntegerParameter("Turns", "T", "Number of turns between radii", GH_ParamAccess.item, 10);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

            pManager.AddCurveParameter("Spiral", "S", "Spiral curve", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane plane = Plane.WorldXY;
            double radius0 = 0.0;
            double radius1 = 0.0;
            int turns = 0;

            if (!DA.GetData(0, ref plane)) return;
            if (!DA.GetData(1, ref radius0)) return;
            if (!DA.GetData(2, ref radius1)) return;
            if (!DA.GetData(3, ref turns)) return;

            if (radius0 < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Inner radius must be bigger than or equal to zero");
                return;
            }
            if (radius1 <= radius0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Outer radius must be bigger than the inner radius");
                return;
            }
            if (turns <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Spiral turn count must be bigger than or equal to one");
                return;
            }


            Rhino.Geometry.Curve spiral = CreateSpiral(plane, radius0, radius1, turns);

            DA.SetData(0, spiral);
        }

        Rhino.Geometry.Curve CreateSpiral(Plane plane, double r0, double r1, int turns)
        {
            Line l0 = new Line(plane.Origin + r0 * plane.XAxis, plane.Origin + r1 * plane.XAxis);
            Line l1 = new Line(plane.Origin - r0 * plane.XAxis, plane.Origin - r1 * plane.XAxis);

            Point3d[] p0;
            Point3d[] p1;

            l0.ToNurbsCurve().DivideByCount(turns, true, out p0);
            l1.ToNurbsCurve().DivideByCount(turns, true, out p1);

            PolyCurve spiral = new PolyCurve();

            for (int i = 0; i < p0.Length - 1; i++)
            {
                Arc arc0 = new Arc(p0[i], plane.YAxis, p1[i + 1]);
                Arc arc1 = new Arc(p1[i + 1], -plane.YAxis, p0[i + 1]);

                spiral.Append(arc0);
                spiral.Append(arc1);
            }

            return spiral;
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_spiral;

        public override Guid ComponentGuid => new Guid("9ae98c86-4b99-4bfd-9492-d5527cadf5fd");
    }
}