using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace MyGrasshopperAssembly1.Curve_tools
{
    public class GetBorderChangedSegments : GH_Component
    {
        public GetBorderChangedSegments()
          : base("GetBorderChangedSegments", "GBCS",
              "Get the border curve changed segments",
              "RigonTools", "Curve")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("PreCurve","C1","The curve before changed.",GH_ParamAccess.item);
            pManager.AddCurveParameter("PostCurve","C2","The curve after changed.",GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Segs1", "S1","Changed segments on pre-curve.",GH_ParamAccess.list);
            pManager.AddCurveParameter("Segs2", "S2", "Changed segments on post-curve.", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv1 = null;
            Curve crv2 = null;
            if (!DA.GetData(0, ref crv1)) return;
            if (!DA.GetData(1, ref crv2)) return;

            var getter = new GetSegments();
            var preCrvs = getter.GetCrvSegments(crv1, crv2);
            var changedCrvs = getter.GetCrvSegments(crv2, crv1);

            DA.SetDataList(0,preCrvs);
            DA.SetDataList(1,changedCrvs);
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_getBorderChangedSegments;

        public override Guid ComponentGuid
        {
            get { return new Guid("97DF0E03-7263-46DC-8A17-42C95708D090"); }
        }
    }

    internal class JudgeChanged
    {
        public static bool IsTwoCrvsOverlap(Curve c1,Curve c2)
        {
            //检查两条线是否重叠
            var crv1=c1.ToNurbsCurve();
            var crv2=c2.ToNurbsCurve();
            var events = Intersection.CurveCurve(crv1, crv2, 0.001, 0.001);
            if (events != null)
                return events[0].IsOverlap;
            return false;
        }

        public static List<Interval> JoinContinueInternal(List<Interval> intervals)
        {
            // 检查接缝点是否位于重合部分的内部，若位于内部，将位于重合部分内部的接缝点两侧的t值区间进行合并
            if (intervals.Count == 1)
                return intervals;
            else 
            {
                //合并连续区间
                for (int i = 0; i < intervals.Count-1; i++)
                {
                    if (intervals[i].T1 == intervals[i + 1].T0)
                    {
                        var interval = Interval.FromUnion(intervals[i], intervals[i+1]);
                        intervals.RemoveAt(i);
                        intervals.Insert(i, interval);
                        break;
                    }
                }
                return intervals;
            }
        }
        public List<Interval> GetTIntervals(Curve preCrv,Curve changedCrv)
        {
            //得到轮廓线发生变动位置的t值区间
            List<Interval> t_intervals1= new List<Interval>();
            List<Interval> t_intervals2 = new List<Interval>();
            //先得到未发生变动的t值区间
            foreach (var i in Intersection.CurveCurve(preCrv,changedCrv,0.001,0.001))
            {
                t_intervals1.Add(i.OverlapA);
            }
            var t_intervals=JoinContinueInternal(t_intervals1);

            //构造发生变动的t值区间
            //检查，解决接缝点位于变动部分内部的情况
            if (t_intervals.Count == 1)
            {
                var interval = new Interval(t_intervals[0].T1, t_intervals[0].T0);
                //若得到的t值区间长度与原轮廓线相同，说明未发生变化，否则变动区间为interval
                if (Math.Abs(interval.Length) != Math.Abs(preCrv.Domain.Length))
                    t_intervals2.Add(interval);
            }
            else 
            {
                for (int j = 0; j < t_intervals.Count - 1; j++)
                {
                    var interval = new Interval(t_intervals[j].T1, t_intervals[j+1].T0);
                    t_intervals2.Add(interval);
                }
            }
            if(t_intervals2 != null)
                return t_intervals2;//返回原轮廓线上发生变动的t值区间
            else return null;
        }
    }

    internal class GetSegments
    {
        public List<Curve> GetCrvSegments(Curve preCrv,Curve changedCrv)
        { 
            List<double> t_values= new List<double>();
            List<Curve> segments= new List<Curve>();
            var judger = new JudgeChanged();
            //得到原轮廓线发生变动的t值区间,若未发生变动则打印未变化
            var intervals = judger.GetTIntervals(preCrv,changedCrv);
            if (intervals != null)
            {
                //根据t值对原轮廓线进行分割
                foreach (var interval in intervals)
                {
                    var min = interval.T0;
                    var max = interval.T1;
                    t_values.Add(min);
                    t_values.Add(max);
                }
                var crvs = preCrv.Split(t_values);
                foreach (var crv in crvs)
                {
                    //如果分割后的片段与改动后轮廓线没有重合，则该片段的端点为发生变动位置
                    if (!JudgeChanged.IsTwoCrvsOverlap(crv, changedCrv))
                        segments.Add(crv);
                }
                return segments;
            }
            else return null;
        }
    }
}