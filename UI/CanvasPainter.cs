using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media.Animation;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.GUI.Canvas;

namespace MyGrasshopperAssembly1.UI
{
    public class CanvasPainter : GH_Component
    {
        public CanvasPainter()
          : base("Canvas Painter", "Canvas",
              "Change the color of your canvas.",
              "RigonTools", "UI")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("BackColor", "B", "Back color of canvas", GH_ParamAccess.item, GH_Skin.canvas_back);
            pManager.AddColourParameter("EdgeColor","E","Edge color of canvas",GH_ParamAccess.item, GH_Skin.canvas_edge);
            pManager.AddColourParameter("GridColor","G","Grid color of canvas",GH_ParamAccess.item, GH_Skin.canvas_grid);
            pManager.AddColourParameter("ShadeColor","S","Shade color of canvas",GH_ParamAccess.item, GH_Skin.canvas_shade);
            pManager.AddBooleanParameter("Reset", "R", "Set default color", GH_ParamAccess.item,false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Color c1 = Color.White;
            Color c2 = Color.White;
            Color c3 = Color.White;
            Color c4 = Color.White;
            Boolean b = false;

            if (!DA.GetData(0, ref c1)) return;
            if (!DA.GetData(1, ref c2)) return;
            if (!DA.GetData(2,ref c3)) return;
            if(!DA.GetData(3,ref c4)) return;
            if (!DA.GetData(4, ref b)) return;

            if (!b)
            {
                GH_Skin.canvas_back = c1;
                GH_Skin.canvas_edge = c2;
                GH_Skin.canvas_grid = c3;
                GH_Skin.canvas_shade = c4;
            }
            else 
            {
                GH_Skin.canvas_back = Color.FromArgb(212,208,200);
                GH_Skin.canvas_edge = Color.FromArgb(0,0,0);
                GH_Skin.canvas_grid = Color.FromArgb(30,0,0,0);
                GH_Skin.canvas_shade = Color.FromArgb(80,0,0,0);
            }

        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_canvasPainter;
        public override Guid ComponentGuid
        {
            get { return new Guid("6B2EDF0E-A595-43E6-AEED-176E08775084"); }
        }
    }
}