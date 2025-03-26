using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Base.Handler;
using DevExpress.XtraGrid.Views.Base.ViewInfo;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid.Handler;
using System;
using DevExpress.XtraGrid.Drawing;
using DevExpress.XtraGrid.Views.Printing;
using DevExpress.Utils.Drawing;

namespace MinRowHeightXtraGrid
{

    public class MinRowHeightGridControl : GridControl
    {
        protected override void RegisterAvailableViewsCore(InfoCollection collection)
        {
            base.RegisterAvailableViewsCore(collection);
            collection.Add(new MyGridViewInfoRegistrator());
            //            collection.Add(new MyGridViewPrintInfoRegistrator());
        }
    }
    public class MyGridViewInfoRegistrator : GridInfoRegistrator
    {
        public override string ViewName { get { return MinRowHeightGridView.MinRowHeightName; } }
        public override BaseView CreateView(GridControl grid)
        {
            return new MinRowHeightGridView(grid as GridControl);
        }
        public override BaseViewInfo CreateViewInfo(BaseView view)
        {
            return new MinRowHeightGridViewInfo(view as MinRowHeightGridView);
        }
        public override BaseViewHandler CreateHandler(BaseView view)
        {
            return new GridHandler(view as GridView);
        }
    }
    public class MinRowHeightGridView : GridView
    {
        public static GridControl printGridControl = null;

        public const string MinRowHeightName = "MinRowHeightGridView";
        public MinRowHeightGridView(GridControl grid) : base(grid) { }
        public override bool Editable
        {
            get
            {
                return false;
            }
        }
        protected override string ViewName { get { return MinRowHeightName; } }

        protected override BaseViewPrintInfo CreatePrintInfoInstance(PrintInfoArgs args)
        {
            return new CustomGridViewPrintInfo(args);
        }
    }
    public class MinRowHeightGridViewInfo : GridViewInfo
    {
        public MinRowHeightGridViewInfo(MinRowHeightGridView view) : base(view) { }
        protected override int CalcMinRowHeight()
        {
            int size = Convert.ToInt32(PaintAppearance.Row.CalcTextSize(GInfo.Cache, "Gq", int.MaxValue).Width) + 1;
            CustomGridViewPrintInfo.printSize = size;
            return size;
        }

        //protected override GridCellInfo CalcRowCellDrawInfoCore(GridDataRowInfo ri, GridColumnInfoArgs ci, GridCellInfo cell, GridColumnInfoArgs nextColumn, bool calcEditInfo, GridRow nextRow, bool allowCache)
        //{
        //    if (ci.Column != null)
        //    {
        //        cell.CellValueRect.Inflate(0, CellValueVIndent);
        //        if (calcEditInfo) CreateCellEditViewInfo(cell, true);
        //    }
        //    return base.CalcRowCellDrawInfoCore(ri, ci, cell, nextColumn, calcEditInfo, nextRow, allowCache);
        //}
    }
    /***********************************************************************************************/
    public class CustomGridViewPrintInfo : GridViewPrintInfo
    {
        public static int printSize = 10;
        public CustomGridViewPrintInfo(PrintInfoArgs args) : base(args)
        {
            //DevExpress.XtraPrinting.BrickGraphics graph = (DevExpress.XtraPrinting.BrickGraphics)args.Graph;
            //float size = graph.Font.Size;
            //float points = graph.Font.SizeInPoints;
            //printSize = (int)size;
            int size = printSize;
        }
        protected override int CalcRowHeight(GraphicsCache cache, int rowHandle, bool isGroup)
        {
            //            int size = Convert.ToInt32(PaintAppearance.Row.CalcTextSize(GInfo.Cache, "Gq", int.MaxValue).Width) + 1;
            int size = printSize;
            if (size < 7)
                size = 7;
            return size;
        }
        /***********************************************************************************************/
    }
}