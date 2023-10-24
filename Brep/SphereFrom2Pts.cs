using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MyGrasshopperAssembly1.Geometry
{
    public class SphereFrom2Pts : GH_Component
    {
        public SphereFrom2Pts()
          : base("Sphere From 2Points", "sfp",
              "Create sphere from 2 points",
              "RigonTools", "Brep")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point1", "P1", "First point of the sphere", GH_ParamAccess.item);
            pManager.AddPointParameter("Point2","P2","Second point of the sphere",GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Sphere", "S", "Sphere Brep", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d p1 = Point3d.Unset;
            Point3d p2 = Point3d.Unset;

            if (!DA.GetData(0, ref p1)) return;
            if (!DA.GetData(1, ref p2)) return;

            Sphere sphere = CreateSphereFrom2Pts(p1, p2);

            DA.SetData(0, sphere);
        }

        public Sphere CreateSphereFrom2Pts(Point3d pt1, Point3d pt2)
        {
            //从两点建立球
            var center = pt1 + (pt2 - pt1) / 2;
            var d = pt1.DistanceTo(pt2);
            return new Sphere(center, d / 2);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_sphere2pts ;
        public override Guid ComponentGuid
        {
            get { return new Guid("CB9BC56C-AE75-4147-821F-55160EA9F880"); }
        }
    }
}