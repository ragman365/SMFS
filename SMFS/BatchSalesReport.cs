using System;
using System.Data;
using System.Windows.Forms;
using GeneralLib;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Pdf;
using System.IO;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class BatchSalesReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public BatchSalesReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void BatchSalesReport_Load(object sender, EventArgs e)
        {
            MySQL.SetMaxAllowedPackets();
            picLoader.Hide();
            labelMaximum.Hide();
            barImport.Hide();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 8, 4, 4, "Sales Report Files", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.ToString("MM/dd/yyyy");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            Printer.DrawQuad(20, 8, 5, 4, "Report Date:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string filename = dr["filename"].ObjToString();
            //string contractNumber = dr["contractNumber"].ObjToString();
            //if (File.Exists(filename))
            //{
            //    ViewPDF viewForm = new ViewPDF("Contract (" + contractNumber + ") Contract", "", filename);
            //    viewForm.Show();
            //}
        }
        /***********************************************************************************************/
        private void btnProcess_Click(object sender, EventArgs e)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection.";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                string folderPath = Path.GetDirectoryName(folderBrowser.FileName);
                string[] files = Directory.GetFiles(folderPath);
                if (files.Length <= 0)
                    MessageBox.Show("No Files found!!!!", "Files Found Dialog");
                else
                    LoadFileList(folderPath, files);
            }
            //using (var fbd = new FolderBrowserDialog())
            //{
            //    DialogResult result = fbd.ShowDialog();

            //    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //    {
            //        string[] files = Directory.GetFiles(fbd.SelectedPath);
            //        if (files.Length <= 0)
            //            MessageBox.Show("No Files found!!!!", "Files Found Dialog");
            //        else
            //            LoadFileList(fbd.SelectedPath, files);
            //    }
            //}
        }
        /***********************************************************************************************/
        private bool decodeFileContract(string filename, ref string contractNumber, ref string customerRecord, ref string imageRecord, ref string contractRecord)
        {
            bool rv = false;
            contractNumber = "";
            customerRecord = "";
            imageRecord = "";
            contractRecord = "";
            if (String.IsNullOrWhiteSpace(filename))
                return rv;
            contractNumber = G1.DecodeFilename(filename, true);
            string file = G1.DecodeFilename(filename);
            string cmd = "Select `record`,`contractNumber` from `pdfimages` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                rv = true;
                imageRecord = dx.Rows[0]["record"].ObjToString();
                cmd = "Select `record`,`contractNumber` from `pdfimages` where `record` = '" + imageRecord + "' AND `image` IS NULL;";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    rv = false;
                    imageRecord = "";
                }
            }
            return rv;
        }
        /***********************************************************************************************/
        private void LoadFileList(string filePath, string[] files)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("record");
            dt.Columns.Add("filename");
            dt.Columns.Add("alreadyimported");
            dt.Columns.Add("status");

            DataRow dRow = null;
            string filename = "";
            int lastrow = files.Length;

            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            barImport.Hide();
            labelMaximum.Hide();

            bool rv = false;

            for (int i = 0; i < files.Length; i++)
            {
                picLoader.Refresh();
                //barImport.Value = i;
                //barImport.Refresh();
                //labelMaximum.Text = i.ToString() + " of " + lastrow.ToString();
                //labelMaximum.Refresh();

                filename = files[i].Trim();
                dRow = dt.NewRow();
                dRow["filename"] = filename;
                dt.Rows.Add(dRow);
            }

            CheckForBackups(dt);

            picLoader.Hide();
            //barImport.Value = lastrow;
            //labelMaximum.Text = lastrow.ToString() + " of " + lastrow.ToString();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void CheckForBackups(DataTable dt)
        {
            string file = "";
            string filename = "";
            string newDirectory = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                filename = dt.Rows[i]["filename"].ObjToString();
                file = G1.DecodeFilename(filename);
                newDirectory = filename.Replace(file, "");
                newDirectory += "/backups";
                if (!Directory.Exists(newDirectory))
                    Directory.CreateDirectory(newDirectory);
                newDirectory += "/" + file;
                if (File.Exists(newDirectory))
                    dt.Rows[i]["alreadyimported"] = "YES";
            }
        }
        /***********************************************************************************************/
        private DataTable surchargeDt = null;
        private void btnImport_Click(object sender, EventArgs e)
        {
            barImport.Visible = true;
            barImport.Show();
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "REMOVE") < 0)
                dt.Columns.Add("REMOVE");

            surchargeDt = ImportInventoryList.LoadSurchargeTable();

            bool doMarked = false;
            string filename = "";
            string status = "";
            string contractNumber = "";
            string record = "";
            string record1 = "";
            string newDirectory = "";
            string file = "";
            string alreadyImported = "";

            int[] rows = gridMain.GetSelectedRows();
            if (rows.Length > 0)
            {
                DialogResult result = MessageBox.Show("Import Selected Marked Rows ONLY?", "Selected Rows Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Yes)
                    doMarked = true;
            }
            int lastRow = dt.Rows.Count;
            if (doMarked)
                lastRow = rows.Length;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            labelMaximum.Show();
            barImport.Refresh();
            labelMaximum.Refresh();

            int notFound = 0;
            bool success = false;

            string cmd = "";
            DataTable ddx = null;
            for (int i = 0; i < lastRow; i++)
            {
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString() + " of " + lastRow.ToString();
                labelMaximum.Refresh();
                try
                {
                    int row = i;
                    if (doMarked)
                    {
                        row = rows[i];
                        row = gridMain.GetDataSourceRowIndex(row);
                    }
                    status = dt.Rows[row]["status"].ObjToString();
                    alreadyImported = dt.Rows[row]["alreadyimported"].ObjToString();
                    filename = dt.Rows[row]["filename"].ObjToString();
                    if (String.IsNullOrWhiteSpace(filename))
                        continue;
                    if (alreadyImported.ToUpper() == "YES" && chkSkip.Checked)
                        continue;
                    filename = filename.Replace('\\', '/');
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        success = ProcessSalesFile(filename, ref notFound);
                        if (success)
                        {
                            file = G1.DecodeFilename(filename);
                            newDirectory = filename.Replace(file, "");
                            newDirectory += "/backups";
                            if (!Directory.Exists(newDirectory))
                                Directory.CreateDirectory(newDirectory);
                            newDirectory += "/" + file;
                            File.Copy(filename, newDirectory, true);
                            File.Delete(filename);
                            dt.Rows[row]["REMOVE"] = "Y";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** on Contract " + contractNumber + " Row " + i.ToString() + " Exception: " + ex.Message.ToString(), "Batch Import Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                if (dt.Rows[i]["REMOVE"].ObjToString() == "Y")
                    dt.Rows.RemoveAt(i);
            }
            this.Cursor = Cursors.Default;


            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString() + " of " + lastRow.ToString();

            MessageBox.Show("***Info*** Batch Import Finished with (" + notFound + ") Serial Numbers Not Found!", "Batch Import Info Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            this.Refresh();
        }
        /***********************************************************************************************/
        private bool ProcessSalesFile(string filename, ref int notFound)
        {
            DataTable dt = Import.ImportCSVfile(filename);

            if (dt.Rows.Count <= 0)
                return false;

            string record = "";
            double grossAmt = 0D;
            double discount = 0D;
            double netAmount = 0D;
            string cmd = "";
            string serialNumber = "";
            string accountcode = "";
            int badCount = 0;
            DataTable dx = null;

            DateTime dateReceived = DateTime.Now;
            double surcharge = 0D;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        serialNumber = dt.Rows[i]["Serial #"].ObjToString();
                        if (String.IsNullOrWhiteSpace(serialNumber))
                            continue;
                        accountcode = dt.Rows[i]["Ship to Address Book Number"].ObjToString();
                        if (String.IsNullOrWhiteSpace(accountcode))
                            continue;

                        cmd = "Select * from `inventory` WHERE `serialNumber` = '" + serialNumber + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            record = dx.Rows[0]["record"].ObjToString();
                            dateReceived = dx.Rows[0]["DateReceived"].ObjToDateTime();

                            surcharge = ImportInventoryList.GetSurcharge(surchargeDt, dateReceived);

                            grossAmt = dt.Rows[i]["Amt Grs"].ObjToDouble();
                            discount = dt.Rows[i]["Amt Disc Avl"].ObjToDouble();
                            netAmount = grossAmt - discount - surcharge;
                            if (netAmount < 0D)
                                netAmount = 0D;
                            G1.update_db_table("inventory", "record", record, new string[] { "gross", grossAmt.ToString(), "discount", discount.ToString(), "net", netAmount.ToString(), "surcharge", surcharge.ToString() });
                        }
                        else
                        {
                            badCount++;
                            notFound++;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            if ( badCount > 0)
                return false;
            return true;
        }
        /***********************************************************************************************/
    }
}