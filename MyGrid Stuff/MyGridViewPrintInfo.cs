﻿// Developer Express Code Central Example:
// How to customize the GridControl's print output.
// 
// This example demonstrates how to override the default exporting process to take
// into account a custom drawn content provided via the
// GridView.CustomDrawFooterCell Event
// (ms-help://DevExpress.NETv10.1/DevExpress.WindowsForms/DevExpressXtraGridViewsGridGridView_CustomDrawFooterCelltopic.htm)
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E2667

using System;
using DevExpress.XtraGrid.Views.Printing;
using DevExpress.XtraPrinting;
using System.Drawing;
using DevExpress.Data;
using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace MyXtraGrid
{
    public class MyGridViewPrintInfo : GridViewPrintInfo
    {
        public int FooterPanelHeight
        {
            get
            {
                return CalcStyleHeight(AppearancePrint.FooterPanel) + 4;
            }
        }
        public MyGridViewPrintInfo(DevExpress.XtraGrid.Views.Printing.PrintInfoArgs args) : base(args) { }

        protected override void CreatePrintColumnCollection()
        {
            int width = 0;
            this.fMaxRowWidth = 0;
            foreach (GridColumn col in View.VisibleColumns)
            {
                width = this.View.IndicatorWidth / View.VisibleColumns.Count;
                PrintColumnInfo colInfo = new PrintColumnInfo();
                colInfo.Bounds = new Rectangle(this.fMaxRowWidth + this.View.IndicatorWidth, 0, col.VisibleWidth - width, headerRowHeight);
                colInfo.Column = col;
                Columns.Add(colInfo);
                this.fMaxRowWidth += colInfo.Bounds.Width;

            }
            this.fMaxRowWidth += this.View.IndicatorWidth;
        }

        //protected override void PrintRow(Graphics g, IBrickGraphics graph, int rowHandle, int level)
        //{
        //    base.PrintRow(g, graph, rowHandle, level);
        //    PrintRowIndicator(g, graph, rowHandle);
        //}

        private void PrintRowIndicator(Graphics g, IBrickGraphics graph, int rowHandle) {

            string displayText;
            ImageBrick ib;
            Rectangle rect = new Rectangle(new Point(Indent, Y - this.CurrentRowHeight), new Size(this.View.IndicatorWidth, this.CurrentRowHeight));
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            GraphicsCache cache = new GraphicsCache(Graphics.FromImage(bmp));
            RowIndicatorCustomDrawEventArgs args = (View as MyGridView).GetCustomDrawRowIndicatorArgs(cache, rect);

            displayText = args.Info.DisplayText;

            BorderSide border = args.Appearance.Options.UseBorderColor ? BorderSide.All : BorderSide.None;
            ib = new ImageBrick(border, 1, args.Appearance.BorderColor, args.Appearance.BackColor);
            ib.Rect = rect;
            ib.Image = bmp;
            if (ib == null) {
            }
            graph.DrawBrick(ib, rect);


        }


        public override void PrintFooterPanel(IBrickGraphics graph)
        {
            base.PrintFooterPanel(graph);



         

             


             ImageBrick ib;


             GridViewInfo info = this.View.GetViewInfo() as GridViewInfo;

             Rectangle rect = new Rectangle(new Point(Indent, Y - this.FooterPanelHeight), new Size(this.View.ViewRect.Width, this.CurrentRowHeight));
             Bitmap bmp = new Bitmap(rect.Width, rect.Height);
             GraphicsCache cache = new GraphicsCache(Graphics.FromImage(bmp));
             RowObjectCustomDrawEventArgs args = (View as MyGridView).GetCustomDrawRowArgs(cache, rect);

             BorderSide border = args.Appearance.Options.UseBorderColor ? BorderSide.All : BorderSide.None;
             ib = new ImageBrick(border, 1, args.Appearance.BorderColor, args.Appearance.BackColor);
             ib.Rect = rect;
             ib.Image = bmp;
             if (ib == null) {
             }
             graph.DrawBrick(ib, rect);

             CustomDrawFooterCells(graph);

        }
      // override Print
        private void CustomDrawFooterCells(IBrickGraphics graph)
        {
            if (!View.OptionsPrint.PrintFooter) return;
            foreach (PrintColumnInfo colInfo in Columns)
            {
                if (colInfo.Column.SummaryItem.SummaryType == SummaryItemType.None) continue;
                Rectangle r = Rectangle.Empty;
                r.X = colInfo.Bounds.X + Indent;
                r.Y = colInfo.RowIndex * FooterPanelHeight + 2 + Y;
                r.Width = colInfo.Bounds.Width;
                r.Height = FooterPanelHeight * colInfo.RowCount;
                r.X -= Indent;
                r.Y -= r.Height;
                string text = string.Empty;
                ImageBrick ib = GetImageBrick(colInfo, r, out text);
                if (ib != null)
                    graph.DrawBrick(ib, ib.Rect);
            }
        }

        private ImageBrick GetImageBrick(PrintColumnInfo colInfo, Rectangle rect, out string displayText)
        {
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            GraphicsCache cache = new GraphicsCache(Graphics.FromImage(bmp));
            FooterCellCustomDrawEventArgs args = (View as MyGridView).GetCustomDrawCellArgs(cache, rect, colInfo.Column);
            displayText = args.Info.DisplayText;
            if (!args.Handled)
                return null;
            BorderSide border = args.Appearance.Options.UseBorderColor? BorderSide.All: BorderSide.None;
            ImageBrick ib = new ImageBrick(border, 1, args.Appearance.BorderColor, args.Appearance.BackColor);
            ib.Rect = rect;
            ib.Image = bmp;
            return ib;
        }
    }
}
