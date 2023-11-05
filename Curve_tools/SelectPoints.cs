using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MyGrasshopperAssembly1.Curve_tools
{
    public class SelectPoints : GH_Component
    {
        public SelectPoints()
          : base("SelectPoints", "SP",
              "Select points on curve or close to curve",
              "RigonTools", "Curve")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to be analysed", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Points to be analysed", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "T", "Allowable distance tolerance", GH_ParamAccess.item,0.001);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("PointsA", "P", "Points that meet the requirements", GH_ParamAccess.list);
            pManager.AddPointParameter("PointsB","P","Point that do not meet the requirements",GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> crvs= new List<Curve>();
            List<Point3d> pts= new List<Point3d>();
            double tol = 0.0;

            if (!DA.GetDataList(0, crvs)) return;
            if(!DA.GetDataList(1,pts)) return;
            if(!DA.GetData(2,ref tol)) return;

            if (tol < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tolerance must be bigger or equal to zero.");
                return;
            }

            double t;
            List<Point3d> pts1 = new List<Point3d>();
            List<Point3d> pts2 = new List<Point3d>();
            foreach (var crv in crvs)
            {
                foreach (Point3d pt in pts)
                {
                    var result = crv.ClosestPoint(pt, out t);
                    var ptOnCrv = crv.PointAt(t);
                    var d = ptOnCrv.DistanceTo(pt);
                    if (d <= tol)
                    {
                        if (!pts1.Contains(pt))
                            pts1.Add(pt);
                    }
                }
            }
            foreach (var pt in pts)
            {
                if(!pts1.Contains(pt))
                    pts2.Add(pt);
            }
            DA.SetDataList(0, pts1);
            DA.SetDataList(1, pts2);
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_selectPts;
        public override Guid ComponentGuid
        {
            get { return new Guid("7D6A8BD6-B898-408C-B61E-1A82943BD056"); }
        }


    }
}