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
using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CheckInterest : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private double workAPR = 0D;
        private double workPayment = 0D;
        private DateTime workFromDate = DateTime.Now;
        private DateTime workToDate = DateTime.Now;
        private double workStartBalance = 0D;
        private bool gotToDate = false;
        /***********************************************************************************************/
        public CheckInterest(double apr, double payment, DateTime fromDate, DateTime toDate, double startBalance)
        {
            InitializeComponent();
            workAPR = apr;
            workFromDate = fromDate;
            workToDate = toDate;
            workStartBalance = startBalance;
            workPayment = payment;
            gotToDate = true;
        }
        /***********************************************************************************************/
        public CheckInterest(double apr, DateTime fromDate, DateTime toDate, double startBalance)
        {
            InitializeComponent();
            workAPR = apr;
            workFromDate = fromDate;
            workToDate = toDate;
            workStartBalance = startBalance;
            gotToDate = true;
        }
        /***********************************************************************************************/
        public CheckInterest( double apr, DateTime fromDate, double startBalance)
        {
            InitializeComponent();
            workAPR = apr;
            workFromDate = fromDate;
            workStartBalance = startBalance;
        }
        /***********************************************************************************************/
        private void CheckInterest_Load(object sender, EventArgs e)
        {
            txtAPR.Text = workAPR.ToString();
            txtStartBalance.Text = workStartBalance.ToString();
            txtPayment.Text = workPayment.ToString();
            txtFromDate.Text = workFromDate.ToString("MM/dd/yyyy");
            txtDays.Text = "1";
            DateTime startDate = txtFromDate.Text.ObjToDateTime();
            DateTime endDate = startDate.AddDays(1);
            if ( gotToDate )
            {
                endDate = workToDate;
                TimeSpan ts = endDate - startDate;
                txtDays.Text = ts.TotalDays.ToString();
            }
            txtToDate.Text = endDate.ToString("MM/dd/yyyy");
            Calculate();
        }
        /***********************************************************************************************/
        private void Calculate ()
        {
            double startBalance = txtStartBalance.Text.ObjToDouble();
            double apr = txtAPR.Text.ObjToDouble();
            if (apr <= 0D)
                apr = 5D;

            if (apr >= 1D)
                apr = apr / 100D;

            DateTime startDate = txtFromDate.Text.ObjToDateTime();
            DateTime endDate = txtToDate.Text.ObjToDateTime();

            TimeSpan ts = endDate - startDate;
            int days = ts.Days;

            int yearlyDays = 365;

            if (DateTime.IsLeapYear(endDate.Year))
                yearlyDays = 366;

            double dailyInterest = apr / (double)(yearlyDays) * (double)(days);
            lblDailyRate.Text = "Daily Rate : " + dailyInterest.ToString();
            double interest = dailyInterest * startBalance;
            interest = G1.RoundDown(interest);
            txtInterest.Text = interest.ToString();
            //double endBalance = startBalance - interest;
            double endBalance = startBalance - workPayment + interest;
            txtEndingBalance.Text = endBalance.ToString();
        }
        /***********************************************************************************************/
        private void txtAPR_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Calculate();
        }
        /***********************************************************************************************/
        private void txtStartBalance_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Calculate();
        }
        /***********************************************************************************************/
        private void txtDays_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            int days = txtDays.Text.ObjToInt32();
            if ( days <= 0 )
            {
                MessageBox.Show("***ERROR*** Days Entered must be greater than 0!");
                return;
            }
            DateTime startDate = txtFromDate.Text.ObjToDateTime();
            DateTime endDate = startDate.AddDays(days);
            txtToDate.Text = endDate.ToString("MM/dd/yyyy");
            Calculate();
        }
        /***********************************************************************************************/
        private void txtFromDate_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            string date = txtFromDate.Text;
            if (G1.validate_date(date))
            {
                DateTime startDate = date.ObjToDateTime();
                DateTime endDate = txtToDate.Text.ObjToDateTime();

                TimeSpan ts = endDate - startDate;
                int days = ts.Days;
                txtDays.Text = days.ToString();
                Calculate();
            }
            else
                MessageBox.Show("***ERROR*** Invalid Date");
        }
        /***********************************************************************************************/
        private void txtToDate_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            string date = txtToDate.Text;
            if (G1.validate_date(date))
            {
                DateTime startDate = date.ObjToDateTime();
                startDate = txtFromDate.Text.ObjToDateTime();
                DateTime endDate = txtToDate.Text.ObjToDateTime();

                TimeSpan ts = endDate - startDate;
                int days = ts.Days;
                txtDays.Text = days.ToString();
                Calculate();
            }
            else
                MessageBox.Show("***ERROR*** Invalid Date");
        }
        /***********************************************************************************************/
    }
}