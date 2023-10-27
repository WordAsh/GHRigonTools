using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MyGrasshopperAssembly1.Curve_tools;
using System.Linq;

namespace MyGrasshopperAssembly1.Brep_tools
{
    public class ProjectOutline : GH_Component
    {
        public ProjectOutline()
          : base("LinearProjectOutline", "LPO",
              "Get multiple breps's linear outline on given plane",
              "RigonTools", "Brep")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Breps", "B", "Breps to be projected.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P", "Projected plane.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Outline", "C", "The outline of the breps on the given plane.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> breps = new List<Brep>();
            Plane plane = Plane.Unset;

            if (!DA.GetDataList(0, breps)) return;
            if(!DA.GetData(1,ref plane)) return;

            //得到投影线
            var projector = new Projector(breps);
            var crvs = projector.ProjectBreps(plane);
            //清理投影线
            var crvCleaner=new CurvesCleaner(crvs);
            var cleanedCrvs = crvCleaner.CleanCrvs();
            //得到外轮廓线
            var outline = crvCleaner.CreateCurvesRegionUnion(cleanedCrvs, plane);

            DA.SetData(0,outline);
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_linearProjectOutline;

        public override Guid ComponentGuid
        {
            get { return new Guid("F1A18983-2E0D-4409-A95B-9E65C47F8005"); }
        }
    }

    internal class Projector
    {
        public List<Brep> breps { get; set; }
        public Projector(List<Brep> breps)
        {
            this.breps = breps;
        }
        public List<Curve> SetBrepEdgesToPlane(Brep brep,Plane plane)
        {
            //将一个brep边缘线投影至给定平面，并返回投影线
            List<Curve> crvList = new List<Curve>();
            var crvs = brep.DuplicateEdgeCurves();
            foreach(var curve in crvs)
            {
                var projectedCrv = Curve.ProjectToPlane(curve, plane);
                if (projectedCrv != null)
                {
                    crvList.Add(projectedCrv);
                }
            }
            return crvList;
        }
        public List<Curve> ProjectBreps(Plane plane)
        {
            //将体量映射至给定平面并得到投影线
            List<Curve> projectCrvs= new List<Curve>();
            foreach (var brep in this.breps)
            {
                var crvs = SetBrepEdgesToPlane(brep,plane);
                projectCrvs.AddRange(crvs);
            }
            return projectCrvs;
        }
    }

    internal class CurvesCleaner
    {
        public List<Curve> crvs { get; set; }
        public CurvesCleaner(List<Curve> crvs)
        {
            this.crvs = crvs;
        }
        public Curve CreateCurvesRegionUnion(List<Curve> crvs,Plane plane)
        {
            //获得一组平面曲线的最外轮廓线
            var regions = Curve.CreateBooleanRegions(crvs, plane,true,0.001);
            var outline = regions.RegionCurves(0)[0];
            return outline;
        }

        public  List<Curve> CleanCrvs()
        {
            //清理一组线，删除重合部分线并将其组合
            List<Curve> crvs = new List<Curve>();
            var curves = DelOverlapCrvs.CleanCrvs(this.crvs);
            var cleanedCrvs = Curve.JoinCurves(curves);
            foreach (var curve in cleanedCrvs)
            {
                crvs.Add(curve);
            }
            return crvs;
        }
    }
}