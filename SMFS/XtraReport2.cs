using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace SMFS
{
    public partial class XtraReport2 : DevExpress.XtraReports.UI.XtraReport
    {
//        private DetailBand Detail;
        private PageHeaderBand PageHeader;
        private PageFooterBand PageFooter;
        private XRLabel HelloWorldLabel;

        public XtraReport2()
        {
            InitializeComponent();
            this.Detail = new DetailBand();
            this.PageHeader = new PageHeaderBand();
            this.PageFooter = new PageFooterBand();
            this.PageFooter.Height = 30;
            this.PageHeader.Height = 30;

            this.Bands.AddRange(new Band[] { this.Detail, this.PageHeader, this.PageFooter });

            this.HelloWorldLabel = new XRLabel();
            this.HelloWorldLabel.Text = "Hello, World!";
            this.HelloWorldLabel.Font = new Font("Tahoma", 15, FontStyle.Bold);

            this.Detail.Controls.Add(this.HelloWorldLabel);
            this.ShowPreview();
        }

    }
}
