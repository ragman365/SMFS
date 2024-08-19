using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using GeneralLib;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.ReportGeneration;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using EMRControlLib;

namespace GeneralLib
{
    /****************************************************************************/
    public class Printer
    {
        /***********************************************************************************************/
        public static int xQuads = 6;
        public static int yQuads = 6;
        public static CreateAreaEventArgs localE;
        /***********************************************************************************************/
        public static int pageTopBorder = 9;
        public static int pageMarginLeft = 0;
        public static int pageMarginRight = 0;
        public static int pageMarginTop = 0;
        public static int pageMarginBottom = 0;
        /***********************************************************************************************/
        public static void setupPrinterMargins ( int left, int right, int top, int bottom)
        {
            pageMarginLeft = left;
            pageMarginRight = right;
            pageMarginTop = top;
            pageMarginBottom = bottom;
        }
        /***********************************************************************************************/
        public static void setupPrinterQuads(CreateAreaEventArgs lE, int qx, int qy )
        {
            localE = lE;
            xQuads = qx;
            yQuads = qy;
        }
        /***********************************************************************************************/
        public static void SetQuadSize(int x, int y)
        {
            xQuads = x;
            yQuads = y;
        }
        /***********************************************************************************************/
        private void DrawQuadGrid()
        {
            for (int x = 1; x <= xQuads; x++)
            {
                for (int y = 1; y <= yQuads; y++)
                {
                    string text = "Quad(" + x.ToString() + "," + y.ToString() + ")";
                    DrawQuad(x, y, 1, 1, text, Color.Red);
                }
            }
        }
        /***********************************************************************************************/
        public static void DrawQuadTicks()
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = pageMarginTop - pageTopBorder;

            float borderwidth = localE.Graph.BorderWidth;

            float quadHeight = (int)(totalHeight / yQuads);
            float quadWidth = (int)(totalWidth / xQuads);
            float startX = 0;
            float startY = 0;

            for (int y = 1; y <= yQuads; y++)
            {
                float fy = (y - 1) * quadHeight;
                PointF p1 = new PointF(startX, fy);
                PointF p2 = new PointF(startX+25, fy);
                localE.Graph.DrawLine(p1, p2, Color.Red, 1f);
            }
        }
        /***********************************************************************************************/
        public static void DrawQuadBorder(int x, int y, int width, int height, BorderSide border, int thickness, Color color)
        {
            if (border == BorderSide.None)
                return;
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = pageMarginTop - pageTopBorder;

            float borderwidth = localE.Graph.BorderWidth;

            float quadHeight = (int)(totalHeight / yQuads);
            float quadWidth = (int)(totalWidth / xQuads);
            float startX = (x - 1) * quadWidth;
            float startY = (y - 1) * quadHeight;

            float endX = startX + (quadWidth * width);
            if (x == xQuads || width == xQuads )
                endX = totalWidth - 1 - thickness;

            float actualWidth = (width * quadWidth) - thickness;
            if (x == xQuads || width == xQuads)
                actualWidth = totalWidth - 1 - thickness;

            float actualHeight = (height * quadHeight) - thickness;
            if (y == yQuads || height == yQuads)
            {
                //actualHeight = totalHeight - 15 - thickness;
                actualHeight = totalHeight - yQuads - thickness - 1;
            }


            if (border == BorderSide.Top || border == BorderSide.All)
            {
                PointF p1 = new PointF(startX, startY);
                PointF p2 = new PointF(startX + actualWidth, startY);
                localE.Graph.DrawLine(p1, p2, color, (float)thickness);
            }
            if (border == BorderSide.Bottom || border == BorderSide.All)
            {
                PointF p1 = new PointF(startX, startY + actualHeight-2);
                PointF p2 = new PointF(startX + actualWidth, startY + actualHeight-2);
                localE.Graph.DrawLine(p1, p2, color, (float)thickness);
            }
            if (border == BorderSide.Left || border == BorderSide.All)
            {
                PointF p1 = new PointF(startX, startY);
                PointF p2 = new PointF(startX, startY + actualHeight);
                localE.Graph.DrawLine(p1, p2, color, (float)thickness);
            }
            if (border == BorderSide.Right || border == BorderSide.All)
            {
                PointF p1 = new PointF((startX + actualWidth), startY);
                PointF p2 = new PointF((startX + actualWidth), startY + actualHeight);
                //PointF p1 = new PointF((endX), startY);
                ////PointF p2 = new PointF((endX), startY + actualHeight - pageTopBorder);
                //PointF p2 = new PointF((endX), startY + actualHeight);
                localE.Graph.DrawLine(p1, p2, color, (float)thickness);
            }
        }
        /***********************************************************************************************/
        public static void DrawQuadBorderRight(int x, int y, int width, int height, BorderSide border, int thinkness, Color color)
        {
            if (border == BorderSide.None)
                return;
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - 0);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            if (border == BorderSide.Top || border == BorderSide.All)
            {
                Point p1 = new Point(startX, startY);
                Point p2 = new Point(startX + actualWidth, startY);
                localE.Graph.DrawLine(p1, p2, color, (float)thinkness);
            }
            if (border == BorderSide.Bottom || border == BorderSide.All)
            {
                Point p1 = new Point(startX, startY + actualHeight);
                Point p2 = new Point(startX + actualWidth, startY + actualHeight);
                localE.Graph.DrawLine(p1, p2, color, (float)thinkness);
            }
            if (border == BorderSide.Left || border == BorderSide.All)
            {
                Point p1 = new Point(startX, startY);
                Point p2 = new Point(startX, startY + actualHeight);
                localE.Graph.DrawLine(p1, p2, color, (float)thinkness);
            }
            if (border == BorderSide.Right || border == BorderSide.All)
            {
                Point p1 = new Point((startX + actualWidth - 1), startY);
                Point p2 = new Point((startX + actualWidth - 1), startY + actualHeight);
                localE.Graph.DrawLine(p1, p2, color, (float)thinkness);
            }
        }
        /***********************************************************************************************/
        public static void DrawQuad(int x, int y, int width, int height, string text, Color color)
        {
            BorderSide border = BorderSide.None;
            Font font = new Font("Arial", 8);
            DrawQuad(x, y, width, height, text, color, border, font, HorizontalAlignment.Left);
        }
        /***********************************************************************************************/
        public static void DrawQuad(int x, int y, int width, int height, string text)
        {
            Color color = Color.Black;
            BorderSide border = BorderSide.None;
            Font font = new Font("Arial", 8);
            DrawQuad(x, y, width, height, text, color, border, font, HorizontalAlignment.Left);
        }
        /***********************************************************************************************/
        public static int RichDrawQuadLine(int x, int y, int width, int height, string text, Color color, BorderSide border, Font font, HorizontalAlignment halignment, VertAlignment valignment = VertAlignment.Default)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;
            startY = y;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            TextBrick textBrick = localE.Graph.DrawString("", color, new RectangleF(startX, startY, actualWidth, actualHeight), border);
            textBrick.BackColor = Color.White;
            textBrick.BorderWidth = 2;
            if (font != null)
                textBrick.Font = font;
            else
                textBrick.Font = new Font("Arial", 16);

            if (halignment == HorizontalAlignment.Center)
                textBrick.HorzAlignment = HorzAlignment.Center;
            else if (halignment == HorizontalAlignment.Left)
                textBrick.HorzAlignment = HorzAlignment.Near;
            else if (halignment == HorizontalAlignment.Right)
                textBrick.HorzAlignment = HorzAlignment.Far;

            if (valignment == VertAlignment.Center)
                textBrick.VertAlignment = VertAlignment.Center;
            else if (valignment == VertAlignment.Bottom)
                textBrick.VertAlignment = VertAlignment.Bottom;
            else if (valignment == VertAlignment.Top)
                textBrick.VertAlignment = VertAlignment.Top;
            else if (valignment == VertAlignment.Default)
                textBrick.VertAlignment = VertAlignment.Default;

            RichTextBrick richBrick = new RichTextBrick();
            richBrick.BackColor = Color.White;
            richBrick.RtfText = text;
            localE.Graph.DrawBrick(richBrick, new RectangleF(startX, startY, actualWidth, actualHeight-1));
            return startY + actualHeight;
            //            localE.Graph.DrawRect(new RectangleF(startX, startY, actualWidth, actualHeight - 1), BorderSide.All, Color.Transparent, Color.Black);
        }
        /***********************************************************************************************/
        public static void DrawQuad(int x, int y, int width, int height, string text, Color color, BorderSide border, Font font, HorizontalAlignment halignment, VertAlignment valignment = VertAlignment.Default)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;


            TextBrick textBrick = localE.Graph.DrawString(text, color, new RectangleF(startX, startY, actualWidth, actualHeight), border);

            textBrick.Value = text;
            textBrick.BorderWidth = 2;
            if (font != null)
                textBrick.Font = font;
            else
                textBrick.Font = new Font("Arial", 16);

            if (halignment == HorizontalAlignment.Center)
                textBrick.HorzAlignment = HorzAlignment.Center;
            else if (halignment == HorizontalAlignment.Left)
                textBrick.HorzAlignment = HorzAlignment.Near;
            else if (halignment == HorizontalAlignment.Right)
                textBrick.HorzAlignment = HorzAlignment.Far;

            if (valignment == VertAlignment.Center)
                textBrick.VertAlignment = VertAlignment.Center;
            else if (valignment == VertAlignment.Bottom)
                textBrick.VertAlignment = VertAlignment.Bottom;
            else if (valignment == VertAlignment.Top)
                textBrick.VertAlignment = VertAlignment.Top;
            else if (valignment == VertAlignment.Default)
                textBrick.VertAlignment = VertAlignment.Default;
        }
        /***********************************************************************************************/
        public static int DrawQuadY(int x, int y, int width, int height, string text, Color color, BorderSide border, Font font, HorizontalAlignment halignment, VertAlignment valignment = VertAlignment.Default)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            //int startY = (y - 1) * quadHeight;

            int startY = y;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            actualHeight = height;


            TextBrick textBrick = localE.Graph.DrawString(text, color, new RectangleF(startX, startY, actualWidth, actualHeight), border);

            textBrick.Value = text;
            textBrick.BorderWidth = 2;
            if (font != null)
                textBrick.Font = font;
            else
                textBrick.Font = new Font("Arial", 16);

            if (halignment == HorizontalAlignment.Center)
                textBrick.HorzAlignment = HorzAlignment.Center;
            else if (halignment == HorizontalAlignment.Left)
                textBrick.HorzAlignment = HorzAlignment.Near;
            else if (halignment == HorizontalAlignment.Right)
                textBrick.HorzAlignment = HorzAlignment.Far;

            if (valignment == VertAlignment.Center)
                textBrick.VertAlignment = VertAlignment.Center;
            else if (valignment == VertAlignment.Bottom)
                textBrick.VertAlignment = VertAlignment.Bottom;
            else if (valignment == VertAlignment.Top)
                textBrick.VertAlignment = VertAlignment.Top;
            else if (valignment == VertAlignment.Default)
                textBrick.VertAlignment = VertAlignment.Default;

            return y + height;
        }
        /***********************************************************************************************/
        public static int RichDrawQuad(int x, int y, int width, int height, string text, Color color, BorderSide border, Font font, HorizontalAlignment halignment, VertAlignment valignment = VertAlignment.Default)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;

            //startY = (int) totalHeight;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            TextBrick textBrick = localE.Graph.DrawString("", color, new RectangleF(startX, startY, actualWidth, actualHeight), border);
            textBrick.BackColor = Color.White;
            textBrick.BorderWidth = 2;
            if (font != null)
                textBrick.Font = font;
            else
                textBrick.Font = new Font("Arial", 16);

            if (halignment == HorizontalAlignment.Center)
                textBrick.HorzAlignment = HorzAlignment.Center;
            else if (halignment == HorizontalAlignment.Left)
                textBrick.HorzAlignment = HorzAlignment.Near;
            else if (halignment == HorizontalAlignment.Right)
                textBrick.HorzAlignment = HorzAlignment.Far;

            if (valignment == VertAlignment.Center)
                textBrick.VertAlignment = VertAlignment.Center;
            else if (valignment == VertAlignment.Bottom)
                textBrick.VertAlignment = VertAlignment.Bottom;
            else if (valignment == VertAlignment.Top)
                textBrick.VertAlignment = VertAlignment.Top;
            else if (valignment == VertAlignment.Default)
                textBrick.VertAlignment = VertAlignment.Default;

            RichTextBrick richBrick = new RichTextBrick();
            richBrick.BackColor = Color.White;
            richBrick.RtfText = text;
            localE.Graph.DrawBrick(richBrick, new RectangleF(startX, startY, actualWidth, actualHeight-1));
            return startY + actualHeight;
            //            localE.Graph.DrawRect(new RectangleF(startX, startY, actualWidth, actualHeight - 1), BorderSide.All, Color.Transparent, Color.Black);
        }
        /***********************************************************************************************/
        public static void MyRichDrawQuad(int startX, int startY, int actualWidth, int actualHeight, string text, Color color, BorderSide border, Font font, HorizontalAlignment halignment, VertAlignment valignment = VertAlignment.Default)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            //int quadHeight = (int)(totalHeight / yQuads);
            //int quadWidth = (int)(totalWidth / xQuads);
            //int startX = (x - 1) * quadWidth;
            //int startY = (y - 1) * quadHeight;

            //int actualWidth = width * quadWidth;
            //int actualHeight = height * quadHeight;

            TextBrick textBrick = localE.Graph.DrawString("", color, new RectangleF(startX, startY, actualWidth, actualHeight), border);
            textBrick.BackColor = Color.White;
            textBrick.BorderWidth = 2;
            if (font != null)
                textBrick.Font = font;
            else
                textBrick.Font = new Font("Arial", 16);

            if (halignment == HorizontalAlignment.Center)
                textBrick.HorzAlignment = HorzAlignment.Center;
            else if (halignment == HorizontalAlignment.Left)
                textBrick.HorzAlignment = HorzAlignment.Near;
            else if (halignment == HorizontalAlignment.Right)
                textBrick.HorzAlignment = HorzAlignment.Far;

            if (valignment == VertAlignment.Center)
                textBrick.VertAlignment = VertAlignment.Center;
            else if (valignment == VertAlignment.Bottom)
                textBrick.VertAlignment = VertAlignment.Bottom;
            else if (valignment == VertAlignment.Top)
                textBrick.VertAlignment = VertAlignment.Top;
            else if (valignment == VertAlignment.Default)
                textBrick.VertAlignment = VertAlignment.Default;

            RichTextBrick richBrick = new RichTextBrick();
            richBrick.BackColor = Color.White;
            richBrick.RtfText = text;
            localE.Graph.DrawBrick(richBrick, new RectangleF(startX, startY, actualWidth, actualHeight - 1));
            //            localE.Graph.DrawRect(new RectangleF(startX, startY, actualWidth, actualHeight - 1), BorderSide.All, Color.Transparent, Color.Black);
        }
        /***********************************************************************************************/
        public static CreateAreaEventArgs GetLocalE()
        {
            return localE;
        }
        /***********************************************************************************************/
        public static void RichDrawQuadPicture(int x, int y, int width, int height, RichTextBoxEx rtb, Color color, BorderSide border, Font font, HorizontalAlignment halignment, VertAlignment valignment = VertAlignment.Default)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            TextBrick textBrick = localE.Graph.DrawString("", color, new RectangleF(startX, startY, actualWidth, actualHeight), border);
            textBrick.BackColor = Color.White;
            textBrick.BorderWidth = 2;
            if (font != null)
                textBrick.Font = font;
            else
                textBrick.Font = new Font("Arial", 16);

            if (halignment == HorizontalAlignment.Center)
                textBrick.HorzAlignment = HorzAlignment.Center;
            else if (halignment == HorizontalAlignment.Left)
                textBrick.HorzAlignment = HorzAlignment.Near;
            else if (halignment == HorizontalAlignment.Right)
                textBrick.HorzAlignment = HorzAlignment.Far;

            if (valignment == VertAlignment.Center)
                textBrick.VertAlignment = VertAlignment.Center;
            else if (valignment == VertAlignment.Bottom)
                textBrick.VertAlignment = VertAlignment.Bottom;
            else if (valignment == VertAlignment.Top)
                textBrick.VertAlignment = VertAlignment.Top;
            else if (valignment == VertAlignment.Default)
                textBrick.VertAlignment = VertAlignment.Default;

            //RichTextBrick richBrick = new RichTextBrick();
            //richBrick.BackColor = Color.White;
            PictureBox picture = null;
            for ( int i=0; i<rtb.Controls.Count; i++)
            {
                Control control = rtb.Controls[i];
                picture = (PictureBox)control;
                //                picture.SizeMode = PictureBoxSizeMode.CenterImage;
                actualWidth = picture.Width+2;
                actualHeight = picture.Height+2;
                break;
            }
            if (picture != null)
            {
                if (picture.Image != null)
                {
                    localE.Graph.DrawImage(picture.Image, new RectangleF(startX, startY, actualWidth, actualHeight - 1), BorderSide.None, Color.White);
                }
            }
//            localE.Graph.DrawRect(new RectangleF(startX, startY, actualWidth, actualHeight - 1), BorderSide.All, Color.Transparent, Color.Black);
        }
        /***********************************************************************************************/
        public static void DrawPagePicture(Image image, int x, int y, int width, int height, Color color, BorderSide border, Font font, HorizontalAlignment halignment, VertAlignment valignment = VertAlignment.Default)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            TextBrick textBrick = localE.Graph.DrawString("", color, new RectangleF(startX, startY, actualWidth, actualHeight), border);
            textBrick.BackColor = Color.White;
            textBrick.BorderWidth = 2;
            if (font != null)
                textBrick.Font = font;
            else
                textBrick.Font = new Font("Arial", 16);

            if (halignment == HorizontalAlignment.Center)
                textBrick.HorzAlignment = HorzAlignment.Center;
            else if (halignment == HorizontalAlignment.Left)
                textBrick.HorzAlignment = HorzAlignment.Near;
            else if (halignment == HorizontalAlignment.Right)
                textBrick.HorzAlignment = HorzAlignment.Far;

            if (valignment == VertAlignment.Center)
                textBrick.VertAlignment = VertAlignment.Center;
            else if (valignment == VertAlignment.Bottom)
                textBrick.VertAlignment = VertAlignment.Bottom;
            else if (valignment == VertAlignment.Top)
                textBrick.VertAlignment = VertAlignment.Top;
            else if (valignment == VertAlignment.Default)
                textBrick.VertAlignment = VertAlignment.Default;

            if (image != null)
            {
                actualWidth = image.Width;
                localE.Graph.DrawImage(image, new RectangleF(startX, startY, actualWidth, actualHeight - 1), BorderSide.None, Color.White);
            }
        }
        /***********************************************************************************************/
        public static void DrawGridDate(int x, int y, int width, int height, Color color, BorderSide border, Font font)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            PageInfoBrick printDate = localE.Graph.DrawPageInfo(PageInfo.DateTime, "{0:MM/dd/yyyy HH:mm}", color, new RectangleF(startX, startY, actualWidth, actualHeight), border);
        }
        /***********************************************************************************************/
        public static void DrawGridPage(int x, int y, int width, int height, Color color, BorderSide border, Font font)
        {
            float totalWidth = localE.Graph.ClientPageSize.Width;
            float totalHeight = (pageMarginTop - pageTopBorder);

            int quadHeight = (int)(totalHeight / yQuads);
            int quadWidth = (int)(totalWidth / xQuads);
            int startX = (x - 1) * quadWidth;
            int startY = (y - 1) * quadHeight;

            int actualWidth = width * quadWidth;
            int actualHeight = height * quadHeight;

            TextBrick pageNumberLabel = new TextBrick(border, 1, color, Color.Transparent, Color.Black);
            pageNumberLabel.Text = "PAGE NO.";
            pageNumberLabel.Rect = new RectangleF(0, 0, 144, 18);
            PageInfoBrick pageNumberInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            pageNumberInfo.PageInfo = PageInfo.Number;
            pageNumberInfo.Rect = new RectangleF(60, 0, 84, 18);
            pageNumberInfo.HorzAlignment = HorzAlignment.Far;

            //TextBrick pageNumberLabel = new TextBrick(border, 1, color, Color.Transparent, Color.Black);
            //pageNumberLabel.Text = "PAGE NO.";
            //pageNumberLabel.Rect = new RectangleF(0, 0, 144, 91);
            //PageInfoBrick pageNumberInfo = new PageInfoBrick(BorderSide.None, 0, Color.Black, Color.Transparent, Color.Black);
            //pageNumberInfo.PageInfo = PageInfo.Number;
            //pageNumberInfo.Rect = new RectangleF(80, 0, 144, 91);
            //pageNumberInfo.HorzAlignment = HorzAlignment.Far;

            // Create RightTopPanel and Paint
            PanelBrick rightTopPanel = new PanelBrick();
            rightTopPanel.BorderWidth = 0;
            rightTopPanel.Bricks.Add(pageNumberLabel);
            rightTopPanel.Bricks.Add(pageNumberInfo);
            localE.Graph.DrawBrick(rightTopPanel, new RectangleF(startX, startY, actualWidth, actualHeight));
        }
        /****************************************************************************/
    }
}
