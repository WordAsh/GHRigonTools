using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace MyGrasshopperAssembly1.Curve_tools
{
    public class DelOverlapCurves : GH_Component
    {

        public DelOverlapCurves()
          : base("DelOverlapCurves", "DOC",
              "Delete overlapped curves.",
              "RigonTools", "Curve")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves","C","Overlapped curves to delete.",GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves","C","Cleaned curves.",GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Rhino.Geometry.Curve> crvs=new List<Rhino.Geometry.Curve>();
            if (!DA.GetDataList(0,crvs)) return;


            var cleanedCrvs = DelOverlapCrvs.CleanCrvs(crvs);
            DA.SetDataList(0, cleanedCrvs);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_delOverlapCurves;

        public override Guid ComponentGuid
        {
            get { return new Guid("52CCF809-C344-410A-8C4B-8B54B9375EDE"); }
        }
    }
    internal class JoinTwoOverlapCurves
    {
        //合并两条有重叠的曲线
        public static bool IsTwoCrvsOverlap(Rhino.Geometry.Curve crv1, Rhino.Geometry.Curve crv2)
        {
            //检查两条线是否重叠
            var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var crv11 = crv1.ToNurbsCurve();
            var crv22 = crv2.ToNurbsCurve();
            var events = Intersection.CurveCurve(crv11, crv22, tol, tol);
            if (events.Count != 0 && events[0].IsOverlap)
            {
                return true;
            }
            else { return false; }

        }

        public static Rhino.Geometry.Curve MergeCurves(Rhino.Geometry.Curve shortCrv, Rhino.Geometry.Curve longCrv)
        {
            //去除重合线，并组合曲线
            var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var sPt = shortCrv.PointAtStart;
            var ePt = shortCrv.PointAtEnd;
            bool result1 = longCrv.ClosestPoint(sPt, out double t1);
            var crvPt1 = longCrv.PointAt(t1);
            bool result2 = longCrv.ClosestPoint(ePt, out double t2);
            var crvPt2 = longCrv.PointAt(t2);

            if (crvPt1.DistanceTo(sPt) < tol && crvPt2.DistanceTo(ePt) < tol)
            {
                return longCrv;
            }
            else
            {
                var lines = new List<Rhino.Geometry.Curve>();
                if (crvPt1.DistanceTo(sPt) < tol)
                {
                    var subLine = longCrv.Split(t1)[0];
                    if (IsTwoCrvsOverlap(subLine, shortCrv) == false)
                    {
                        lines.Add(subLine);
                        lines.Add(shortCrv);
                        return Rhino.Geometry.Curve.JoinCurves(lines, tol)[0];
                    }
                    else
                    {
                        lines.Add(longCrv.Split(t1)[1]);
                        lines.Add(shortCrv);
                        return Rhino.Geometry.Curve.JoinCurves(lines, tol)[0];
                    }
                }
                else
                {
                    var subLine = longCrv.Split(t2)[0];
                    if (IsTwoCrvsOverlap(subLine, shortCrv) is false)
                    {
                        lines.Add(subLine);
                        lines.Add(shortCrv);
                        return Rhino.Geometry.Curve.JoinCurves(lines, tol)[0];
                    }
                    else
                    {
                        lines.Add(longCrv.Split(t2)[1]);
                        lines.Add(shortCrv);
                        return Rhino.Geometry.Curve.JoinCurves(lines, tol)[0];
                    }
                }
            }
        }
        public static Rhino.Geometry.Curve MergeTwoOverlapCurve(Rhino.Geometry.Curve crv1, Rhino.Geometry.Curve crv2)
        {
            //将两条重合线合并为一条线，并返回这条线
            var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            if (crv1.GetLength() > crv2.GetLength())
            {
                return MergeCurves(crv2, crv1);
            }
            else if (crv1.GetLength() == crv2.GetLength())
            {
                return MergeCurves(crv1, crv2);
            }
            else
            {
                return MergeCurves(crv1, crv2);
            }
        }
    }

    internal class DelOverlapCrvs 
    {
        //删除一组曲线中有重叠部分的线，并将删除后的线合并
        public static List<Rhino.Geometry.Curve> CleanCrvs(List<Rhino.Geometry.Curve> crvs)
        {
            //去除一个列表中的重叠部分的线
            if (crvs.Count == 1)
            {
                return crvs;
            }
            else
            {
                int x = crvs.Count;
                var list1 = crvs.Take(x / 2).ToList();
                var list2 = crvs.Skip(x / 2).ToList();
                var crvs1 = CleanCrvs(list1);
                var crvs2 = CleanCrvs(list2);

            Excute:
                foreach (var crvi in crvs1)
                {
                    foreach (var crvj in crvs2)
                    {
                        if (JoinTwoOverlapCurves.IsTwoCrvsOverlap(crvi, crvj))
                        {
                            var newCrv = JoinTwoOverlapCurves.MergeTwoOverlapCurve(crvi, crvj);
                            crvs2.Remove(crvj);
                            crvs2.Add(newCrv);
                            crvs1.Remove(crvi);
                            goto Excute;
                        }
                    }
                    crvs2.Add(crvi);
                }
                return crvs2;
            }
        }
    }
}