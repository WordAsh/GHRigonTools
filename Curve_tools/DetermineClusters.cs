using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MyGrasshopperAssembly1.Curve_tools
{
    public class DetermineClusters : GH_Component
    {

        public DetermineClusters()
          : base("DetermineClusters", "DC",
              "Determine how many clusters the input curves contain.",
              "RigonTools", "Curve")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves","C","List of curves to determine.",GH_ParamAccess.list);
            pManager.AddIntegerParameter("Index","I","The index of the clusters.",GH_ParamAccess.item,0);
            
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Count","N","The count of the clusters.",GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves","C","Curves of the index clusters.",GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Rhino.Geometry.Curve> crvs = new List<Rhino.Geometry.Curve>();
            int i = 0;
            if (!DA.GetDataList(0, crvs)) return;
            if (!DA.GetData(1, ref i)) return;

            var determiner=new DetermineClustersAlgorithm();
            determiner.SortAllCluster(crvs);
            int x = determiner.ClusterList.Count;
            var curves = determiner.ClusterList[i];

            DA.SetData(0,x);
            DA.SetDataList(1,curves);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_determineClusters;

        public override Guid ComponentGuid
        {
            get { return new Guid("9EFD36D4-8814-442D-A72C-573A015B6E87"); }
        }
    }
    public class DetermineClustersAlgorithm
    {
        //判定线有多少簇
        private  Dictionary<int, List<Curve>> _clusterList = new Dictionary<int, List<Curve>>();
        public  Dictionary<int, List<Curve>> ClusterList
        {
            get { return _clusterList; }
            set { _clusterList = value; }
        }
        public static bool IsTwoCrvsIntersected(Curve crv1, Curve crv2)
        {
            //检查两条线是否相交
            var tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(crv1, crv2, tol, 0.0);
            if (events.Count != 0) { return true; }
            return false;
        }
        public static List<Curve> DelDupCrvs(List<Curve> cluster, List<Curve> crvList)
        {
            //在线段列表中删除已经加入至簇的线
            foreach (var curve in cluster)
            {
                if (crvList.Contains(curve))
                    crvList.Remove(curve);
            }
            return crvList;

        }
        public static List<Curve> FirstSortCluster(List<Curve> crvs)
        {
            //对线列表进行初始筛选，将与其中一条线相交的线同这条线一起放入一个簇
            List<Curve> cluster = new List<Curve>
            {
                crvs[0]
            };
            crvs.RemoveAt(0);
            foreach (var crv in crvs)
            {
                if (IsTwoCrvsIntersected(cluster[0], crv))
                {
                    cluster.Add(crv);
                }
            }
            return cluster;
        }
        public  List<Curve> SortOneCluster(List<Curve> crvs, int i)
        {
            //选出一个线段簇
            var cluster = FirstSortCluster(crvs);
            var leftCrvs = DelDupCrvs(cluster, crvs);
            var newLeftCrvs = new List<Curve>();
            while (true)
            {
                var tempCluster = new List<Curve>();
                foreach (var leftCrv in leftCrvs)
                {
                    foreach (var crv in cluster)
                    {
                        if (IsTwoCrvsIntersected(crv, leftCrv))
                        {
                            tempCluster.Add(leftCrv);
                            break;
                        }
                    }
                    if (tempCluster.Count != 0) { break; }
                    else { continue; }
                }
                if (tempCluster.Count != 0)
                {
                    cluster.AddRange(tempCluster);
                    leftCrvs = DelDupCrvs(cluster, leftCrvs);
                }
                else
                {
                    ClusterList.Add(i, cluster);
                    newLeftCrvs = leftCrvs;
                    break;
                }
            }
            return newLeftCrvs;
        }
        public  void SortAllCluster(List<Curve> curves)
        {
            //筛选出所有线段簇
            List<Curve> leftCrvs = new List<Curve>();
            leftCrvs = curves;
            int i = 0;
            while (true)
            {

                leftCrvs = SortOneCluster(leftCrvs, i);
                if (leftCrvs.Count == 0)
                {
                    break;
                }
                else { i++; continue; }
            }
        }
    }
}