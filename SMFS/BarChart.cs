using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class BarChart : DevExpress.XtraEditors.XtraForm
    {
        private string workTitle = "";
        private DataTable workDt = null;
        private ChartControl barChart = null;
        private PieSeriesView myView = null;
        private Series series1 = null;
        private DateTime workDate1 = DateTime.Now;
        private DateTime workDate2 = DateTime.Now;
        /****************************************************************************************/
        public BarChart( string title, DataTable dt, DateTime date1, DateTime date2 )
        {
            InitializeComponent();
            workTitle = title;
            workDt = dt;
            workDate1 = date1;
            workDate2 = date2;
        }
        /****************************************************************************************/
        private void BarChart_Load (object sender, EventArgs e)
        {
            string name = "";
            cmbWhat.Items.Clear();
            cmbWhat.Text = "";
            for ( int col=0; col<workDt.Columns.Count; col++)
            {
                name = workDt.Columns[col].ColumnName.Trim().ToUpper();
                if (name == "CONTRACTVALUE")
                    cmbWhat.Items.Add("Total New Contracts");
                else if (name == "TOTALPAYMENTS")
                    cmbWhat.Items.Add("Total Payments");
                else if (name == "COMMISSION")
                    cmbWhat.Items.Add("Commission");
                else if (name == "BASE COMMISSION")
                    cmbWhat.Items.Add("Base Commission");
                else if (name == "TOTAL COMMISSION")
                    cmbWhat.Items.Add("Total Commission");
                else if (name == "CONTRACT COMMISSION")
                    cmbWhat.Items.Add("Contract Commission");
            }
            if ( cmbWhat.Items.Count > 0 )
            {
                cmbWhat.Text = cmbWhat.Items[0].ObjToString();
            }
        }
        /****************************************************************************************/
        private void RunBarChart()
        {
            // Create an empty chart.
            if (barChart != null)
            {
                CleanupBarChart();
                barChart.Dispose();
            }
            barChart = null;
            barChart = new ChartControl();

            string what = this.cmbWhat.Text;
            string title = "Agents " + workTitle + " by " + what;
            title += " " + workDate1.ToString("MM/dd/yyyy") + " - " + workDate2.ToString("MM/dd/yyyy");

            barChart.Titles.Add(new ChartTitle() { Text = title });

            // Create a pie series.
            series1 = new Series(title, ViewType.Bar);

            // Bind the series to data.
            //series1.DataSource = DataPoint.GetDataPoints();
            //series1.ArgumentDataMember = "Argument";
            //series1.ValueDataMembers.AddRange(new string[] { "Value" });

            series1.DataSource = workDt;
            series1.ArgumentDataMember = "agentNumber";
            if ( cmbLabelBy.Text.ToUpper() == "AGENT NAME")
                series1.ArgumentDataMember = "agentName";

            if ( what.ToUpper() == "TOTAL NEW CONTRACTS")
                series1.ValueDataMembers.AddRange(new string[] { "contractValue" });
            else if (what.ToUpper() == "TOTAL PAYMENTS")
                series1.ValueDataMembers.AddRange(new string[] { "totalPayments" });
            else if(what.ToUpper() == "COMMISSION")
                series1.ValueDataMembers.AddRange(new string[] { "commission" });
            else if (what.ToUpper() == "CONTRACT COMMISSION")
                series1.ValueDataMembers.AddRange(new string[] { "Contract Commission" });
            else if (what.ToUpper() == "BASE COMMISSION")
                series1.ValueDataMembers.AddRange(new string[] { "Base Commission" });
            else if (what.ToUpper() == "TOTAL COMMISSION")
                series1.ValueDataMembers.AddRange(new string[] { "Total Commission" });

            // Add the series to the chart.
            barChart.Series.Add(series1);

            // Format the the series labels.
            series1.Label.TextPattern = "{VP:p0} (${V:.##})";

            // Format the series legend items.
            series1.LegendTextPattern = "{A}";

            //// Adjust the position of series labels. 
            //((PieSeriesLabel)series1.Label).Position = PieSeriesLabelPosition.TwoColumns;

            //// Detect overlapping of series labels.
            //((PieSeriesLabel)series1.Label).ResolveOverlappingMode = ResolveOverlappingMode.Default;

            double filter = 0D;
            string str = txtFilter.Text;
            if (G1.validate_numeric(str))
                filter = str.ObjToDouble();
            try
            {
                if (filter > 0D)
                {
                    series1.TopNOptions.Enabled = true;
                    if (cmbFilter.Text == "%")
                    {
                        series1.TopNOptions.Mode = TopNMode.ThresholdPercent;
                        series1.TopNOptions.ThresholdPercent = filter;
                    }
                    else
                    {
                        series1.TopNOptions.Mode = TopNMode.ThresholdValue;
                        series1.TopNOptions.ThresholdValue = filter;
                    }
                }
                else
                {
                    series1.TopNOptions.Enabled = false;
                }
            }
            catch (Exception ex)
            {
            }

            // Access the view-type-specific options of the series.
            //myView = (PieSeriesView)series1.View;

            //// Specify a data filter to explode points.
            //myView.ExplodedPointsFilters.Add(new SeriesPointFilter(SeriesPointKey.Value_1,
            //    DataFilterCondition.GreaterThanOrEqual, 9));
            //myView.ExplodedPointsFilters.Add(new SeriesPointFilter(SeriesPointKey.Argument,
            //    DataFilterCondition.NotEqual, "Others"));


            //myView.ExplodeMode = PieExplodeMode.UseFilters;
            //myView.ExplodedDistancePercentage = 30;
            //myView.RuntimeExploding = true;

            // Customize the legend.
            barChart.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;

            // Add the chart to the form.
            barChart.Dock = DockStyle.Fill;
            this.panelBottom.Controls.Add(barChart);
        }
        /****************************************************************************************/
        private void CleanupBarChart()
        {
            for ( int i=0; i<this.panelBottom.Controls.Count; i++)
            {
                Control control = this.panelBottom.Controls[i];
                this.panelBottom.Controls.RemoveAt(i);
            }
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            RunBarChart();
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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

            printableComponentLink1.Component = barChart;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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

            printableComponentLink1.Component = barChart;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);

            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "Pie Chart";
            if (!String.IsNullOrWhiteSpace(this.Text))
                title = this.Text;
            string str = txtFilter.Text;
            if ( G1.validate_numeric ( str))
            {
                double filter = str.ObjToDouble();
                if (filter > 0D)
                {
                    string what = cmbFilter.Text;
                    title += " Threshold " + filter.ToString() + " " + what;
                }
            }
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
            string search = "Agents : All";
            Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
    }
}