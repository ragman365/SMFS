using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using MySql.Data.MySqlClient;
using GeneralLib;
namespace SMFS
{
    public class MyReport : DevExpress.XtraReports.UI.XtraReport
    {
        private DevExpress.XtraReports.UI.TopMarginBand topMarginBand1;
        private DevExpress.XtraReports.UI.PageHeaderBand pageHeaderBand1;
        private DevExpress.XtraReports.UI.XRLabel label1;
        private DevExpress.XtraReports.UI.XRLabel label2;
        private DevExpress.XtraReports.UI.XRLabel label3;
        private DevExpress.XtraReports.UI.XRLabel label4;
        private DevExpress.XtraReports.UI.XRLabel label5;
        private DevExpress.XtraReports.UI.XRLabel label6;
        private DevExpress.XtraReports.UI.XRLabel label7;
        private DevExpress.XtraReports.UI.XRLabel label8;
        private DevExpress.XtraReports.UI.XRLabel label9;
        private DevExpress.XtraReports.UI.XRLabel label10;
        private DevExpress.XtraReports.UI.XRLabel label11;
        private DevExpress.XtraReports.UI.XRLabel label12;
        private DevExpress.XtraReports.UI.DetailBand detailBand1;
        private DevExpress.XtraReports.UI.XRLabel label13;
        private DevExpress.XtraReports.UI.XRLabel label14;
        private DevExpress.XtraReports.UI.XRLabel label15;
        private DevExpress.XtraReports.UI.XRLabel label16;
        private DevExpress.XtraReports.UI.XRLabel label17;
        private DevExpress.XtraReports.UI.XRLabel label18;
        private DevExpress.XtraReports.UI.XRLabel label19;
        private DevExpress.XtraReports.UI.XRLabel label20;
        private DevExpress.XtraReports.UI.XRLabel label21;
        private DevExpress.XtraReports.UI.XRLabel label22;
        private DevExpress.XtraReports.UI.XRLabel label23;
        private DevExpress.XtraReports.UI.ReportFooterBand reportFooterBand1;
        private DevExpress.XtraReports.UI.XRLabel label24;
        private DevExpress.XtraReports.UI.XRLabel label25;
        private DevExpress.XtraReports.UI.XRLabel label26;
        private DevExpress.XtraReports.UI.XRLabel label27;
        private DevExpress.XtraReports.UI.XRLabel label28;
        private DevExpress.XtraReports.UI.XRLabel label29;
        private DevExpress.XtraReports.UI.XRLabel label30;
        private DevExpress.XtraReports.UI.XRLabel label31;
        private DevExpress.XtraReports.UI.XRLabel label32;
        private DevExpress.XtraReports.UI.XRLabel label33;
        private DevExpress.XtraReports.UI.XRLabel label34;
        private DevExpress.XtraReports.UI.BottomMarginBand bottomMarginBand1;
        private DevExpress.XtraReports.UI.XRControlStyle ReportHeaderBandStyle;
        private DevExpress.XtraReports.UI.XRControlStyle ReportGroupHeaderBandStyle;
        private DevExpress.XtraReports.UI.XRControlStyle ReportDetailBandStyle;
        private DevExpress.XtraReports.UI.XRControlStyle ReportGroupFooterBandStyle;
        private DevExpress.XtraReports.UI.XRControlStyle ReportFooterBandStyle;
        private DevExpress.XtraReports.UI.XRControlStyle ReportOddStyle;
        private DevExpress.XtraReports.UI.XRControlStyle ReportEvenStyle;

        public MyReport( DataTable dt, DevExpress.XtraGrid.GridControl dgv )
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.topMarginBand1 = new DevExpress.XtraReports.UI.TopMarginBand();
            this.pageHeaderBand1 = new DevExpress.XtraReports.UI.PageHeaderBand();
            this.label1 = new DevExpress.XtraReports.UI.XRLabel();
            this.label2 = new DevExpress.XtraReports.UI.XRLabel();
            this.label3 = new DevExpress.XtraReports.UI.XRLabel();
            this.label4 = new DevExpress.XtraReports.UI.XRLabel();
            this.label5 = new DevExpress.XtraReports.UI.XRLabel();
            this.label6 = new DevExpress.XtraReports.UI.XRLabel();
            this.label7 = new DevExpress.XtraReports.UI.XRLabel();
            this.label8 = new DevExpress.XtraReports.UI.XRLabel();
            this.label9 = new DevExpress.XtraReports.UI.XRLabel();
            this.label10 = new DevExpress.XtraReports.UI.XRLabel();
            this.label11 = new DevExpress.XtraReports.UI.XRLabel();
            this.label12 = new DevExpress.XtraReports.UI.XRLabel();
            this.detailBand1 = new DevExpress.XtraReports.UI.DetailBand();
            this.label13 = new DevExpress.XtraReports.UI.XRLabel();
            this.label14 = new DevExpress.XtraReports.UI.XRLabel();
            this.label15 = new DevExpress.XtraReports.UI.XRLabel();
            this.label16 = new DevExpress.XtraReports.UI.XRLabel();
            this.label17 = new DevExpress.XtraReports.UI.XRLabel();
            this.label18 = new DevExpress.XtraReports.UI.XRLabel();
            this.label19 = new DevExpress.XtraReports.UI.XRLabel();
            this.label20 = new DevExpress.XtraReports.UI.XRLabel();
            this.label21 = new DevExpress.XtraReports.UI.XRLabel();
            this.label22 = new DevExpress.XtraReports.UI.XRLabel();
            this.label23 = new DevExpress.XtraReports.UI.XRLabel();
            this.reportFooterBand1 = new DevExpress.XtraReports.UI.ReportFooterBand();
            this.label24 = new DevExpress.XtraReports.UI.XRLabel();
            this.label25 = new DevExpress.XtraReports.UI.XRLabel();
            this.label26 = new DevExpress.XtraReports.UI.XRLabel();
            this.label27 = new DevExpress.XtraReports.UI.XRLabel();
            this.label28 = new DevExpress.XtraReports.UI.XRLabel();
            this.label29 = new DevExpress.XtraReports.UI.XRLabel();
            this.label30 = new DevExpress.XtraReports.UI.XRLabel();
            this.label31 = new DevExpress.XtraReports.UI.XRLabel();
            this.label32 = new DevExpress.XtraReports.UI.XRLabel();
            this.label33 = new DevExpress.XtraReports.UI.XRLabel();
            this.label34 = new DevExpress.XtraReports.UI.XRLabel();
            this.bottomMarginBand1 = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.ReportHeaderBandStyle = new DevExpress.XtraReports.UI.XRControlStyle();
            this.ReportGroupHeaderBandStyle = new DevExpress.XtraReports.UI.XRControlStyle();
            this.ReportDetailBandStyle = new DevExpress.XtraReports.UI.XRControlStyle();
            this.ReportGroupFooterBandStyle = new DevExpress.XtraReports.UI.XRControlStyle();
            this.ReportFooterBandStyle = new DevExpress.XtraReports.UI.XRControlStyle();
            this.ReportOddStyle = new DevExpress.XtraReports.UI.XRControlStyle();
            this.ReportEvenStyle = new DevExpress.XtraReports.UI.XRControlStyle();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // topMarginBand1
            // 
            this.topMarginBand1.Dpi = 254F;
            this.topMarginBand1.HeightF = 100F;
            this.topMarginBand1.Name = "topMarginBand1";
            // 
            // pageHeaderBand1
            // 
            this.pageHeaderBand1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.label1,
            this.label2,
            this.label3,
            this.label4,
            this.label5,
            this.label6,
            this.label7,
            this.label8,
            this.label9,
            this.label10,
            this.label11,
            this.label12});
            this.pageHeaderBand1.Dpi = 254F;
            this.pageHeaderBand1.HeightF = 126F;
            this.pageHeaderBand1.Name = "pageHeaderBand1";
            this.pageHeaderBand1.StyleName = "ReportHeaderBandStyle";
            // 
            // label1
            // 
            this.label1.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label1.Dpi = 254F;
            this.label1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.label1.Name = "label1";
            this.label1.SizeF = new System.Drawing.SizeF(1592F, 63F);
            this.label1.Text = "Funeral Home";
            this.label1.WordWrap = false;
            // 
            // label2
            // 
            this.label2.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label2.Dpi = 254F;
            this.label2.LocationFloat = new DevExpress.Utils.PointFloat(0F, 63F);
            this.label2.Name = "label2";
            this.label2.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label2.Text = "Num";
            this.label2.WordWrap = false;
            // 
            // label3
            // 
            this.label3.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label3.Dpi = 254F;
            this.label3.LocationFloat = new DevExpress.Utils.PointFloat(144.7273F, 63F);
            this.label3.Name = "label3";
            this.label3.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label3.Text = "Agent";
            this.label3.WordWrap = false;
            // 
            // label4
            // 
            this.label4.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label4.Dpi = 254F;
            this.label4.LocationFloat = new DevExpress.Utils.PointFloat(289.4546F, 63F);
            this.label4.Name = "label4";
            this.label4.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label4.Text = "Contract";
            this.label4.WordWrap = false;
            // 
            // label5
            // 
            this.label5.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label5.Dpi = 254F;
            this.label5.LocationFloat = new DevExpress.Utils.PointFloat(434.1818F, 63F);
            this.label5.Name = "label5";
            this.label5.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label5.Text = "Customer";
            this.label5.WordWrap = false;
            // 
            // label6
            // 
            this.label6.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label6.Dpi = 254F;
            this.label6.LocationFloat = new DevExpress.Utils.PointFloat(578.9091F, 63F);
            this.label6.Name = "label6";
            this.label6.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label6.Text = "Phone";
            this.label6.WordWrap = false;
            // 
            // label7
            // 
            this.label7.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label7.Dpi = 254F;
            this.label7.LocationFloat = new DevExpress.Utils.PointFloat(723.6364F, 63F);
            this.label7.Name = "label7";
            this.label7.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label7.Text = "Total Contract";
            this.label7.WordWrap = false;
            // 
            // label8
            // 
            this.label8.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label8.Dpi = 254F;
            this.label8.LocationFloat = new DevExpress.Utils.PointFloat(868.3637F, 63F);
            this.label8.Name = "label8";
            this.label8.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label8.Text = "Monthly Payment";
            this.label8.WordWrap = false;
            // 
            // label9
            // 
            this.label9.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label9.Dpi = 254F;
            this.label9.LocationFloat = new DevExpress.Utils.PointFloat(1013.091F, 63F);
            this.label9.Name = "label9";
            this.label9.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label9.Text = "Balance Due";
            this.label9.WordWrap = false;
            // 
            // label10
            // 
            this.label10.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label10.Dpi = 254F;
            this.label10.LocationFloat = new DevExpress.Utils.PointFloat(1157.818F, 63F);
            this.label10.Name = "label10";
            this.label10.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label10.Text = "Due Date";
            this.label10.WordWrap = false;
            // 
            // label11
            // 
            this.label11.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label11.Dpi = 254F;
            this.label11.LocationFloat = new DevExpress.Utils.PointFloat(1302.546F, 63F);
            this.label11.Name = "label11";
            this.label11.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label11.Text = "Last Date Paid";
            this.label11.WordWrap = false;
            // 
            // label12
            // 
            this.label12.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label12.Dpi = 254F;
            this.label12.LocationFloat = new DevExpress.Utils.PointFloat(1447.273F, 63F);
            this.label12.Name = "label12";
            this.label12.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label12.Text = "Days Late";
            this.label12.WordWrap = false;
            // 
            // detailBand1
            // 
            this.detailBand1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.label13,
            this.label14,
            this.label15,
            this.label16,
            this.label17,
            this.label18,
            this.label19,
            this.label20,
            this.label21,
            this.label22,
            this.label23});
            this.detailBand1.Dpi = 254F;
            this.detailBand1.EvenStyleName = "ReportEvenStyle";
            this.detailBand1.HeightF = 63F;
            this.detailBand1.Name = "detailBand1";
            this.detailBand1.OddStyleName = "ReportOddStyle";
            this.detailBand1.StyleName = "ReportDetailBandStyle";
            // 
            // label13
            // 
            this.label13.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label13.Dpi = 254F;
            this.label13.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[num]")});
            this.label13.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.label13.Name = "label13";
            this.label13.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label13.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label13.WordWrap = false;
            // 
            // label14
            // 
            this.label14.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label14.Dpi = 254F;
            this.label14.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[agentNumber]")});
            this.label14.LocationFloat = new DevExpress.Utils.PointFloat(144.7273F, 0F);
            this.label14.Name = "label14";
            this.label14.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label14.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label14.WordWrap = false;
            // 
            // label15
            // 
            this.label15.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label15.Dpi = 254F;
            this.label15.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[contractNumber]")});
            this.label15.LocationFloat = new DevExpress.Utils.PointFloat(289.4546F, 0F);
            this.label15.Name = "label15";
            this.label15.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label15.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label15.WordWrap = false;
            // 
            // label16
            // 
            this.label16.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label16.Dpi = 254F;
            this.label16.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[customer]")});
            this.label16.LocationFloat = new DevExpress.Utils.PointFloat(434.1818F, 0F);
            this.label16.Name = "label16";
            this.label16.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label16.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label16.WordWrap = false;
            // 
            // label17
            // 
            this.label17.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label17.Dpi = 254F;
            this.label17.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[phone]")});
            this.label17.LocationFloat = new DevExpress.Utils.PointFloat(578.9091F, 0F);
            this.label17.Name = "label17";
            this.label17.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label17.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label17.WordWrap = false;
            // 
            // label18
            // 
            this.label18.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label18.Dpi = 254F;
            this.label18.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[totalContract]")});
            this.label18.LocationFloat = new DevExpress.Utils.PointFloat(723.6364F, 0F);
            this.label18.Name = "label18";
            this.label18.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label18.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label18.TextFormatString = "{0:N2}";
            this.label18.WordWrap = false;
            // 
            // label19
            // 
            this.label19.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label19.Dpi = 254F;
            this.label19.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[amtOfMonthlyPayt]")});
            this.label19.LocationFloat = new DevExpress.Utils.PointFloat(868.3637F, 0F);
            this.label19.Name = "label19";
            this.label19.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label19.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label19.TextFormatString = "{0:N2}";
            this.label19.WordWrap = false;
            // 
            // label20
            // 
            this.label20.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label20.Dpi = 254F;
            this.label20.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[balanceDue]")});
            this.label20.LocationFloat = new DevExpress.Utils.PointFloat(1013.091F, 0F);
            this.label20.Name = "label20";
            this.label20.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label20.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label20.TextFormatString = "{0:N2}";
            this.label20.WordWrap = false;
            // 
            // label21
            // 
            this.label21.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label21.Dpi = 254F;
            this.label21.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[dueDate8]")});
            this.label21.LocationFloat = new DevExpress.Utils.PointFloat(1157.818F, 0F);
            this.label21.Name = "label21";
            this.label21.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label21.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label21.TextFormatString = "{0:mm/dd/yyyy}";
            this.label21.WordWrap = false;
            // 
            // label22
            // 
            this.label22.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label22.Dpi = 254F;
            this.label22.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[lastDatePaid8]")});
            this.label22.LocationFloat = new DevExpress.Utils.PointFloat(1302.546F, 0F);
            this.label22.Name = "label22";
            this.label22.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label22.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label22.TextFormatString = "{0:N2}";
            this.label22.WordWrap = false;
            // 
            // label23
            // 
            this.label23.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label23.Dpi = 254F;
            this.label23.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[daysLate]")});
            this.label23.LocationFloat = new DevExpress.Utils.PointFloat(1447.273F, 0F);
            this.label23.Name = "label23";
            this.label23.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label23.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.label23.TextFormatString = "{0:N0}";
            this.label23.WordWrap = false;
            // 
            // reportFooterBand1
            // 
            this.reportFooterBand1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.label24,
            this.label25,
            this.label26,
            this.label27,
            this.label28,
            this.label29,
            this.label30,
            this.label31,
            this.label32,
            this.label33,
            this.label34});
            this.reportFooterBand1.Dpi = 254F;
            this.reportFooterBand1.HeightF = 63F;
            this.reportFooterBand1.Name = "reportFooterBand1";
            this.reportFooterBand1.StyleName = "ReportHeaderBandStyle";
            // 
            // label24
            // 
            this.label24.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label24.Dpi = 254F;
            this.label24.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.label24.Name = "label24";
            this.label24.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label24.WordWrap = false;
            // 
            // label25
            // 
            this.label25.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label25.Dpi = 254F;
            this.label25.LocationFloat = new DevExpress.Utils.PointFloat(144.7273F, 0F);
            this.label25.Name = "label25";
            this.label25.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label25.WordWrap = false;
            // 
            // label26
            // 
            this.label26.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label26.Dpi = 254F;
            this.label26.LocationFloat = new DevExpress.Utils.PointFloat(289.4546F, 0F);
            this.label26.Name = "label26";
            this.label26.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label26.WordWrap = false;
            // 
            // label27
            // 
            this.label27.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label27.Dpi = 254F;
            this.label27.LocationFloat = new DevExpress.Utils.PointFloat(434.1818F, 0F);
            this.label27.Name = "label27";
            this.label27.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label27.WordWrap = false;
            // 
            // label28
            // 
            this.label28.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label28.Dpi = 254F;
            this.label28.LocationFloat = new DevExpress.Utils.PointFloat(578.9091F, 0F);
            this.label28.Name = "label28";
            this.label28.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label28.WordWrap = false;
            // 
            // label29
            // 
            this.label29.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label29.Dpi = 254F;
            this.label29.LocationFloat = new DevExpress.Utils.PointFloat(723.6364F, 0F);
            this.label29.Name = "label29";
            this.label29.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label29.WordWrap = false;
            // 
            // label30
            // 
            this.label30.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label30.Dpi = 254F;
            this.label30.LocationFloat = new DevExpress.Utils.PointFloat(868.3637F, 0F);
            this.label30.Name = "label30";
            this.label30.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label30.WordWrap = false;
            // 
            // label31
            // 
            this.label31.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label31.Dpi = 254F;
            this.label31.LocationFloat = new DevExpress.Utils.PointFloat(1013.091F, 0F);
            this.label31.Name = "label31";
            this.label31.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label31.WordWrap = false;
            // 
            // label32
            // 
            this.label32.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label32.Dpi = 254F;
            this.label32.LocationFloat = new DevExpress.Utils.PointFloat(1157.818F, 0F);
            this.label32.Name = "label32";
            this.label32.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label32.WordWrap = false;
            // 
            // label33
            // 
            this.label33.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label33.Dpi = 254F;
            this.label33.LocationFloat = new DevExpress.Utils.PointFloat(1302.546F, 0F);
            this.label33.Name = "label33";
            this.label33.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label33.WordWrap = false;
            // 
            // label34
            // 
            this.label34.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.label34.Dpi = 254F;
            this.label34.LocationFloat = new DevExpress.Utils.PointFloat(1447.273F, 0F);
            this.label34.Name = "label34";
            this.label34.SizeF = new System.Drawing.SizeF(144.7273F, 63F);
            this.label34.WordWrap = false;
            // 
            // bottomMarginBand1
            // 
            this.bottomMarginBand1.Dpi = 254F;
            this.bottomMarginBand1.HeightF = 100F;
            this.bottomMarginBand1.Name = "bottomMarginBand1";
            // 
            // ReportHeaderBandStyle
            // 
            this.ReportHeaderBandStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.ReportHeaderBandStyle.Name = "ReportHeaderBandStyle";
            this.ReportHeaderBandStyle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 0, 254F);
            this.ReportHeaderBandStyle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ReportGroupHeaderBandStyle
            // 
            this.ReportGroupHeaderBandStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.ReportGroupHeaderBandStyle.Name = "ReportGroupHeaderBandStyle";
            this.ReportGroupHeaderBandStyle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 0, 0, 0, 254F);
            this.ReportGroupHeaderBandStyle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ReportDetailBandStyle
            // 
            this.ReportDetailBandStyle.BackColor = System.Drawing.Color.Transparent;
            this.ReportDetailBandStyle.Name = "ReportDetailBandStyle";
            this.ReportDetailBandStyle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 0, 254F);
            this.ReportDetailBandStyle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ReportGroupFooterBandStyle
            // 
            this.ReportGroupFooterBandStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.ReportGroupFooterBandStyle.Name = "ReportGroupFooterBandStyle";
            this.ReportGroupFooterBandStyle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 0, 254F);
            this.ReportGroupFooterBandStyle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ReportFooterBandStyle
            // 
            this.ReportFooterBandStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.ReportFooterBandStyle.Name = "ReportFooterBandStyle";
            this.ReportFooterBandStyle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 0, 254F);
            this.ReportFooterBandStyle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ReportOddStyle
            // 
            this.ReportOddStyle.BackColor = System.Drawing.Color.Transparent;
            this.ReportOddStyle.Name = "ReportOddStyle";
            this.ReportOddStyle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 0, 254F);
            this.ReportOddStyle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // ReportEvenStyle
            // 
            this.ReportEvenStyle.BackColor = System.Drawing.Color.Transparent;
            this.ReportEvenStyle.Name = "ReportEvenStyle";
            this.ReportEvenStyle.Padding = new DevExpress.XtraPrinting.PaddingInfo(10, 10, 0, 0, 254F);
            this.ReportEvenStyle.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // MyReport
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.topMarginBand1,
            this.pageHeaderBand1,
            this.detailBand1,
            this.reportFooterBand1,
            this.bottomMarginBand1});
            this.Dpi = 254F;
            this.Margins = new System.Drawing.Printing.Margins(254, 254, 100, 100);
            this.PageHeight = 2970;
            this.PageWidth = 2100;
            this.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.ReportUnit = DevExpress.XtraReports.UI.ReportUnit.TenthsOfAMillimeter;
            this.SnapGridSize = 25F;
            this.StyleSheet.AddRange(new DevExpress.XtraReports.UI.XRControlStyle[] {
            this.ReportHeaderBandStyle,
            this.ReportGroupHeaderBandStyle,
            this.ReportDetailBandStyle,
            this.ReportGroupFooterBandStyle,
            this.ReportFooterBandStyle,
            this.ReportOddStyle,
            this.ReportEvenStyle});
            this.Version = "17.2";
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
    }
}