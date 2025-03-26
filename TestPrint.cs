using System;
using DevExpress.XtraPrinting;
using System.Collections.Generic;
using System.Drawing;
/***********************************************************************************************/

namespace SMFS
{
    /***********************************************************************************************/
    public partial class TestPrint : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public TestPrint()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/

        private void button1_Click(object sender, EventArgs e)
        {
            // Create a new Printing System.
            PrintingSystem printingSystem = new PrintingSystem();

            // Create a link and add it to the printing system's collection of links.
            Link link = new Link();
            printingSystem.Links.Add(link);

            // Subscribe to the events to customize the detail and marginal page header sections of a document.
            link.CreateDetailArea += Link_CreateDetailArea;
            link.CreateMarginalHeaderArea += Link_CreateMarginalHeaderArea;

            // Create a document and show it in the document preview.
            try
            {
                link.ShowPreview();
            }
            catch ( Exception ex)
            {
            }
        }
        private void Link_CreateDetailArea(object sender, CreateAreaEventArgs e)
        {
            try
            {
                // Specify required settings for the brick graphics.
                BrickGraphics brickGraphics = e.Graph;
                BrickStringFormat format = new BrickStringFormat(StringAlignment.Near, StringAlignment.Center);
                brickGraphics.StringFormat = format;
                brickGraphics.BorderColor = SystemColors.ControlDark;

                // Declare bricks.
                ImageBrick imageBrick;
                TextBrick textBrick;
                CheckBoxBrick checkBrick;
                Brick brick;

                // Declare text strings.
                string[] rows = { "Species No:", "Length (cm):", "Category:", "Common Name:", "Species Name:" },
                    desc = { "90070", "30", "Angelfish", "Blue Angelfish", "Pomacanthus nauarchus" };

                string note = "Habitat is around boulders, caves, coral ledges and crevices in shallow waters. " +
                    "Swims alone or in groups. Its color changes dramatically from juvenile to adult. The mature" +
                    " adult fish can startle divers by producing a powerful drumming or thumping sound intended " +
                    "to warn off predators. Edibility is good. Range is the entire Indo-Pacific region.";

                // Define the image to display.
                Image img = Image.FromFile(@"c:\rag\angelfish.png");

                // Start creation of a non-separable group of bricks.
                brickGraphics.BeginUnionRect();

                // Display the image.
                imageBrick = brickGraphics.DrawImage(img, new RectangleF(0, 0, 250, 150), BorderSide.All, Color.Transparent);
                imageBrick.Hint = "Blue Angelfish";


                textBrick = brickGraphics.DrawString("1", Color.Blue, new RectangleF(5, 5, 30, 15), BorderSide.All);
                textBrick.StringFormat = textBrick.StringFormat.ChangeAlignment(StringAlignment.Center);

                // Display a checkbox.
                checkBrick = brickGraphics.DrawCheckBox(new RectangleF(5, 145, 10, 10), BorderSide.All, Color.White, true);

                // Create a set of bricks, representing a column with species names.
                brickGraphics.BackColor = Color.FromArgb(153, 204, 255);
                brickGraphics.Font = new Font("Arial", 10, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline);
                for (int i = 0; i < 5; i++)
                {

                    // Draw a VisualBrick representing borders for the following TextBrick.
                    brick = brickGraphics.DrawRect(new RectangleF(256, 32 * i, 120, 32), BorderSide.All,
                        Color.Transparent, Color.Empty);

                    // Draw the TextBrick with species names.
                    textBrick = brickGraphics.DrawString(rows[i], Color.Black, new RectangleF(258, 32 * i + 2, 116, 28),
                        BorderSide.All);
                }

                // Create a set of bricks representing a column with the species characteristics.
                brickGraphics.Font = new Font("Arial", 11, FontStyle.Bold);
                brickGraphics.BackColor = Color.White;
                for (int i = 0; i < 1; i++)
                {
                    brick = brickGraphics.DrawRect(new RectangleF(376, 32 * i, brickGraphics.ClientPageSize.Width - 376, 32),
                        BorderSide.All,
                    Color.Transparent, brickGraphics.BorderColor);

                    // Draw a TextBrick with species characteristics.
                    textBrick.Value = "HeaderArea";

                    textBrick = brickGraphics.DrawString(desc[i], Color.Indigo, new RectangleF(378, 32 * i + 2,
                        brickGraphics.ClientPageSize.Width - 380, 28),
                    BorderSide.All);

                    // For text bricks containing numeric data, set text alignment to Far.
                    if (i < 2) textBrick.StringFormat =
                        textBrick.StringFormat.ChangeAlignment(StringAlignment.Far);
                }

                // Drawing the TextBrick with notes.
                brickGraphics.Font = new Font("Arial", 8);
                brickGraphics.BackColor = Color.Cornsilk;
                textBrick = brickGraphics.DrawString(note, Color.Black, new RectangleF(new PointF(0, 160), new
                    SizeF(brickGraphics.ClientPageSize.Width, 40)), BorderSide.All);
                textBrick.StringFormat = textBrick.StringFormat.ChangeLineAlignment(StringAlignment.Near);
                textBrick.Hint = note;

                // Finish the creation of a non-separable group of bricks.
                brickGraphics.EndUnionRect();
            }
            catch ( Exception ex)
            {
            }
        }

        private void Link_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            try
            {
                // Specify required settings for the brick graphics.
                BrickGraphics brickGraphics = e.Graph;
                brickGraphics.BackColor = Color.White;
                brickGraphics.Font = new Font("Arial", 8);

                // Declare bricks.
                PageInfoBrick pageInfoBrick;
                PageImageBrick pageImageBrick;

                // Declare text strings.
                string devexpress = "XtraPrintingSystem by Developer Express Inc.";

                // Define the image to display.
                Image pageImage = Image.FromFile(@"c:\rag\logo.png");

                SizeF size = brickGraphics.MeasureString(devexpress);

                // Display the PageImageBrick containing the DevExpress logo.
                pageImageBrick = brickGraphics.DrawPageImage(pageImage, new RectangleF(343, 0,
                    pageImage.Width, pageImage.Height), BorderSide.None, Color.Transparent);
                pageImageBrick.Alignment = BrickAlignment.Center;

                // Set the rectangle for a page info brick. 
                RectangleF r = RectangleF.Empty;
                r.Height = 20;

                // Display the PageInfoBrick containing date-time information. Date-time information is displayed
                // in the left part of the MarginalHeader section using the FullDateTimePattern.
                pageInfoBrick = brickGraphics.DrawPageInfo(PageInfo.DateTime, "{0:F}", Color.Black, r, BorderSide.None);
                pageInfoBrick.Alignment = BrickAlignment.Near;

                // Display the PageInfoBrick containing the page number among total pages. The page number
                // is displayed in the right part of the MarginalHeader section.
                pageInfoBrick = brickGraphics.DrawPageInfo(PageInfo.NumberOfTotal, "Page {0} of {1}", Color.Black, r,
                     BorderSide.None);
                pageInfoBrick.Alignment = BrickAlignment.Far;

                // Display the DevExpress text string.
                pageInfoBrick = brickGraphics.DrawPageInfo(PageInfo.None, devexpress, Color.Black, new RectangleF(new
                    PointF(343 - (size.Width - pageImage.Width) / 2, pageImage.Height + 3), size), BorderSide.None);
                pageInfoBrick.Alignment = BrickAlignment.Center;

                pageInfoBrick.Value = "MarginalArea";
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            // Create a new Printing System.
            PrintingSystem printingSystem = new PrintingSystem();

            // Create a link and add it to the printing system's collection of links.
            Link link = new Link();
            printingSystem.Links.Add(link);

            // Subscribe to the events to customize the detail and marginal page header sections of a document.
            link.CreateDetailArea += Link_CreateDetailArea;
            link.CreateMarginalHeaderArea += Link_CreateMarginalHeaderArea;

            link.CreateDocument();
            ModifyDocument(link.PrintingSystem.Pages, link.PrintingSystem);

            // Create a document and show it in the document preview.
            link.ShowPreview();
        }
        private void ModifyDocument(PageList pages, PrintingSystem printingSystem)
        {
            foreach (DevExpress.XtraPrinting.Page p in pages)
            {
                PageInfoBrick marginalAreaBrick = new PageInfoBrick();
                TextBrick headerAreaBrick = new TextBrick();

                DevExpress.XtraPrinting.Native.NestedBrickIterator iterator = new DevExpress.XtraPrinting.Native.NestedBrickIterator(p.InnerBricks);
                while (iterator.MoveNext())
                {
                    VisualBrick visualBrick = iterator.CurrentBrick as VisualBrick;
                    if (visualBrick != null)
                        if (visualBrick.Value != null)
                        {
                            if (visualBrick.Value.ToString() == "MarginalArea")
                            {
                                marginalAreaBrick = (PageInfoBrick)visualBrick;
                                marginalAreaBrick.Text = "RAMMAZAMMA";
                            }
                            if (visualBrick.Value.ToString() == "HeaderArea")
                            {
                                headerAreaBrick = (TextBrick)visualBrick;
                                headerAreaBrick.Text = "Black Angel Shark";
                            }
                        }
                }


                marginalAreaBrick.Format = headerAreaBrick.Text;

                //// Define the image to display.
                //Image pageImage = Image.FromFile(@"..\..\logo.png");
                //BrickGraphics brickGraphics = new BrickGraphics(printingSystem);

                //// Display the headerAreaBrick text string.
                //SizeF size = brickGraphics.MeasureString(headerAreaBrick.Text);
                //marginalAreaBrick = brickGraphics.DrawPageInfo(PageInfo.None, headerAreaBrick.Text, Color.Black, new RectangleF(new
                //    PointF(343 - (size.Width - pageImage.Width) / 2, pageImage.Height + 3), size), BorderSide.None);
                //marginalAreaBrick.Alignment = BrickAlignment.Center;
                ////marginalAreaBrick.Text = headerAreaBrick.Text;
            }
        }
        /***********************************************************************************************/
    }
}