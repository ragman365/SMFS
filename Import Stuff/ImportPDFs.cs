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
    public partial class ImportPDFs : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public ImportPDFs()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void ImportPDFs_Load(object sender, EventArgs e)
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
            Printer.DrawQuad(6, 8, 4, 4, "PDF Contract Files", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
            DataRow dr = gridMain.GetFocusedDataRow();
            string filename = dr["filename"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (File.Exists(filename))
            {
                ViewPDF viewForm = new ViewPDF("Contract (" + contractNumber + ") Contract", "", filename);
                viewForm.Show();
            }
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
        private bool decodeFileContract(string filename, ref string contractNumber, ref string customerRecord, ref string imageRecord, ref string contractRecord )
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
            string[] Lines = file.Split(' ');
            if ( Lines.Length > 0 )
            {
                contractNumber = Lines[0].Trim();
                contractNumber = contractNumber.Replace(".PDF", "");
                contractNumber = contractNumber.Replace(".pdf", "");
            }
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
        private void LoadFileList ( string filePath, string [] files )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("record");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("filename");
            dt.Columns.Add("alreadyimported");
            dt.Columns.Add("imageRecord");
            dt.Columns.Add("status");
            DataRow dRow = null;
            string contractNumber = "";
            string customerRecord = "";
            string contractRecord = "";
            string imageRecord = "";
            string filename = "";
            int lastrow = files.Length;

            barImport.Minimum = 0;
            barImport.Maximum = lastrow;
            labelMaximum.Show();

            bool rv = false;

            for ( int i=0; i<files.Length; i++)
            {
                picLoader.Refresh();
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString() + " of " + lastrow.ToString();
                labelMaximum.Refresh();

                filename = files[i].Trim();
                rv = decodeFileContract(filename, ref contractNumber, ref customerRecord, ref imageRecord, ref contractRecord);
                if (rv)
                {
                    dRow = dt.NewRow();
                    contractNumber = contractNumber.Replace(".PDF", "");
                    contractNumber = contractNumber.Replace(".pdf", "");
                    dRow["contractNumber"] = contractNumber;
                    dRow["filename"] = filename;
                    dRow["record"] = customerRecord;
                    dRow["imageRecord"] = imageRecord;
                    if (!String.IsNullOrWhiteSpace(imageRecord))
                        dRow["alreadyimported"] = "YES";
                    dt.Rows.Add(dRow);
                }
                else
                {
                    contractNumber = contractNumber.Replace(".PDF", "");
                    contractNumber = contractNumber.Replace(".pdf", "");
                    dRow = dt.NewRow();
                    dRow["status"] = "*INFO* No Current Record Yet";
                    if (!String.IsNullOrWhiteSpace(contractRecord))
                        dRow["status"] = "Found Contract / No Customer";
                    dRow["filename"] = filename;
                    dRow["record"] = "";
                    dRow["imageRecord"] = "";
                    dRow["contractNumber"] = contractNumber;
                    dt.Rows.Add(dRow);
                }
            }
            picLoader.Hide();
            barImport.Value = lastrow;
            labelMaximum.Text = lastrow.ToString() + " of " + lastrow.ToString();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void btnImport_Click(object sender, EventArgs e)
        {
            barImport.Visible = true;
            barImport.Show();
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "REMOVE") < 0)
                dt.Columns.Add("REMOVE");

            bool doMarked = false;
            string filename = "";
            string status = "";
            string contractNumber = "";
            string record = "";
            string record1 = "";
            string newDirectory = "";
            string file = "";

            int[] rows = gridMain.GetSelectedRows();
            if (rows.Length > 0)
            {
                DialogResult result = MessageBox.Show("Import Selected Marked Rows ONLY?", "Selected Rows Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                    doMarked = true;
            }
            int lastRow = dt.Rows.Count;
            if (doMarked)
                lastRow = rows.Length;

            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            labelMaximum.Show();
            string cmd = "";
            DataTable ddx = null;
            for (int i = 0; i < lastRow; i++)
            {
                barImport.Value = i;
                barImport.Refresh();
                labelMaximum.Text = i.ToString() + " of " + lastRow.ToString();
                try
                {
                    int row = i;
                    if (doMarked)
                    {
                        row = rows[i];
                        row = gridMain.GetDataSourceRowIndex(row);
                    }
                    status = dt.Rows[row]["status"].ObjToString();
                    //if (!String.IsNullOrWhiteSpace(status))
                    //    continue;
                    filename = dt.Rows[row]["filename"].ObjToString();
                    contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(contractNumber))
                        continue;
                    if (String.IsNullOrWhiteSpace(filename))
                        continue;
                    record = dt.Rows[row]["record"].ObjToString();
                    record1 = dt.Rows[row]["imageRecord"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record1) && chkSkip.Checked)
                        continue;
                    filename = filename.Replace('\\', '/');
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        cmd = "Select `record`, `contractNumber` from `pdfimages` where `contractNumber` = '" + contractNumber + "';";
                        ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                            record1 = ddx.Rows[0]["record"].ObjToString();
                        if ( String.IsNullOrWhiteSpace ( record1))
                            record1 = G1.create_record("pdfimages", "filename", "-1");
                        G1.update_db_table("pdfimages", "record", record1, new string[] { "filename", filename, "contractNumber", contractNumber });
                        G1.ReadAndStorePDF("pdfimages", record1, filename);
                        dt.Rows[row]["alreadyimported"] = "YES";

                        file = G1.DecodeFilename(filename);
                        newDirectory = filename.Replace(file, "");
                        newDirectory += "/backups";
                        if (!Directory.Exists(newDirectory))
                            Directory.CreateDirectory(newDirectory);
                        newDirectory += "/" + file;
                        File.Copy(filename, newDirectory, true );
                        File.Delete(filename);
                        dt.Rows[row]["REMOVE"] = "Y";
                    }
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** on Contract " + contractNumber + " Row " + i.ToString() + " Exception: " + ex.Message.ToString());
                }
            }
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                if (dt.Rows[i]["REMOVE"].ObjToString() == "Y")
                    dt.Rows.RemoveAt(i);
            }
            this.Cursor = Cursors.Default;
            barImport.Value = lastRow;
            barImport.Refresh();
            labelMaximum.Text = lastRow.ToString() + " of " + lastRow.ToString();
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void goToCustomerContractsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
    }
}