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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ExplainTrust : DevExpress.XtraEditors.XtraForm
    {
        private string workContract = "";
        private int workMethod = 0;
        private double workContractValue = 0D;
        private double workDownPayment = 0D;
        private DateTime workIssueDate = DateTime.Now;
        private double workPayment = 0D;
        private double workInterest = 0D;
        private double workPrincipal = 0D;
        private double workAPR = 0D;
        private double workTrust100 = 0D;
        private double workTrust85 = 0D;
        private double workMonths = 0D;
        private DataTable workDt = null;
        /***********************************************************************************************/
        public ExplainTrust( string contract, int method, double contractValue, double downPayment, DateTime issueDate, double payment, double interest, double principal, double apr, double trust100, double trust85, double months, DataTable dt)
        {
            InitializeComponent();
            workContract = contract;
            workMethod = method;
            workContractValue = contractValue;
            workDt = dt;
            workDownPayment = downPayment;
            workIssueDate = issueDate;
            workPayment = payment;
            workInterest = interest;
            workPrincipal = principal;
            workAPR = apr;
            workTrust100 = trust100;
            workTrust85 = trust85;
            workMonths = months;
        }
        /***********************************************************************************************/
        private void ExplainTrust_Load(object sender, EventArgs e)
        {
            string text = "\n\nContract " + workContract + " Issue Date is " + workIssueDate.ToString("MM/dd/yyyy") + ".\n\n";
            if ( workMethod == 1 )
            {
                text += "Method 1 will be used to calculated Trust100 and Trust85 ";
                text += "because the Interest Rate is Zero.\n\n";
                text += "So, the Principal (" + workPrincipal.ToString() + ") is used as the Trust100,\n";
                text += "and 85% of Principal (" + workTrust85.ToString() + ") calculated at Trust85.";
            }
            else if ( workMethod == 2 )
            {
                text += "Method 2 will be used to calculated Trust100 and Trust85 ";
                text += "because the Issue Date is greater than or equal to 12/1/2017.\n\n";
                text += "So, the Principal (" + workPrincipal.ToString() + ") is used as the Trust100,\n";
                text += "and 85% of Principal (" + workTrust85.ToString() + ") calculated at Trust85.";
            }
            else if ( workMethod == 3 )
            {
                text += "Method 3 will be used to calculated Trust100 and Trust85 ";
                text += "because the Issue Date is less than 12/1/2017.\n\n";
                if (workPayment == 0D && workPrincipal == 0D)
                    text += "However, the payment is equal to 0.00, so no Trust100 or Trust85 will be calculated!\n";
                else
                {
                    text += "Contract Value is (" + workContractValue.ToString() + ") with a Down Payment of (" + workDownPayment.ToString() + ").\n";
                    if (workMonths <= 0D)
                    {
                        text += "However, Financed months is equal 0. So, Trust100 is calculated as the Contract Value of (" + workContractValue.ToString() + "),\n";
                        text += "and Trust85 (" + workTrust85.ToString() + ") is 85% or Trust100.\n";
                    }
                    else
                    {
                        text += "So, Trust100 (" + workTrust100.ToString() + ") will be calcuated as ContractValue ( (" + workContractValue.ToString() + ") - Down Payment (" + workDownPayment.ToString() + " ) ) / Finance Months (" + workMonths.ToString() + ").\n";
                        if (workTrust100 > 0D)
                        {
                            int count = (int)(workPrincipal / workTrust100);
                            if ( count > 1 )
                            {
                                text += "Also, the Principal ( " + workPrincipal.ToString() + ") is a multiple of (" + count.ToString() + "), so multiple Trust100 will be determined.\n";
                            }
                            text += "Therefore, 85% of Trust100 (" + workTrust100.ToString() + ") will be use to calculate Trust85 (" + workTrust85.ToString() + ").";
                        }
                    }
                }
            }
            else
                text += "Method has not been determined!\n";
            rtb.Text = text;
            this.Refresh();

            //trust100P = contractValue;
            //if (financeMonths > 0D)
            //{
            //    trust100P = (contractValue - downpayment) / financeMonths;
            //    //                    trust100P = (contractValue) / financeMonths;
            //    trust100P = G1.RoundDown(trust100P);
            //    int count = (int)(principal / trust100P); // For amounts greater than 1 equal payment
            //    if (count > 1)
            //        trust100P = trust100P * count; // Customer made more than one payment
            //}
            //trust85P = trust100P * .85D; // trust85 is 85% of trust100
            //if (payment == 0D && principal == 0D) // This is when manual payments are made with only debit or credit
            //{
            //    trust100P = 0D;
            //    trust85P = 0D;
            //}
            //if (principal < 0D && payment < 0D)
            //{ // Must be debit
            //    trust100P = trust100P * (-1D);
            //    trust85P = 0D * (-1D);
            //}

        }
        /***********************************************************************************************/
    }
}