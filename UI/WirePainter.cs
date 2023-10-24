using System;
using System.Collections.Generic;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace MyGrasshopperAssembly1.UI
{
    public class WirePainter : GH_Component
    {
        public WirePainter()
          : base("WirePainter", "Wire",
              "Change the color of your wires.",
              "RigonTools", "UI")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("DefaultColor","D","Default wire color.",GH_ParamAccess.item,GH_Skin.wire_default);
            pManager.AddColourParameter("EmptyColor", "E", "Empty wire color.", GH_ParamAccess.item, GH_Skin.wire_empty);
            pManager.AddColourParameter("SelectedAColor", "SA", "The selected end wire color", GH_ParamAccess.item, GH_Skin.wire_selected_a);
            pManager.AddColourParameter("SelectedBColor", "SB", "The unselected end wire color", GH_ParamAccess.item, GH_Skin.wire_selected_b);
            pManager.AddBooleanParameter("Reset","R","Set default color",GH_ParamAccess.item,false);
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
            if (!DA.GetData(2, ref c3)) return;
            if (!DA.GetData(3, ref c4)) return;
            if (!DA.GetData(4, ref b)) return;

            if (!b)
            {
                GH_Skin.wire_default = c1;
                GH_Skin.wire_empty = c2;
                GH_Skin.wire_selected_a = c3;
                GH_Skin.wire_selected_b = c4;
            }
            else
            {
                GH_Skin.wire_default = Color.FromArgb(150,0,0,0);
                GH_Skin.wire_empty = Color.FromArgb(180,255,60,0);
                GH_Skin.wire_selected_a = Color.FromArgb(125,210,40);
                GH_Skin.wire_selected_b = Color.FromArgb(50,0,0,0);
            }
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.icon_wirePainter;

        public override Guid ComponentGuid
        {
            get { return new Guid("0A041671-0FCD-4870-9718-5998E26A711C"); }
        }
    }
}