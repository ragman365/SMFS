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
using System.IO;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using System.Diagnostics.Contracts;
using DevExpress.XtraGrid.Columns;
using System.Configuration;
using DevExpress.XtraPrinting;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FunPayments : DevExpress.XtraEditors.XtraForm
    {
        public string myWorkContract = "";
        public string myWorkPayer = "";
        private string workContract = "";
        private string workPayer = "";

        private string workSSN = "";
        private DateTime workBirthday = DateTime.Now;

        private string workServiceId = "";
        private DateTime workDeceasedDate = DateTime.Now;
        private bool funModified = false;
        private bool workNoEdit = false;
        private bool loading = true;
        private Color serviceColor = Color.Transparent;
        private bool showServices = true;
        private bool showMerchandise = false;
        private bool showCashAdvanced = false;
        private DataTable workDt = null;
        private DevExpress.XtraEditors.XtraForm workControl = null;
        private bool justLoading = false;
        private string custExtendedRecord = "";
        private bool totalModified = false;
        private double totalFiled = 0D;
        private double totalReceived = 0D;
        private double totalDiscount = 0D;
        private double totalAmountDiscount = 0D;
        private double totalAmountGrowth = 0D;
        private double totalMerchandise = 0D;
        private double totalServices = 0D;
        private double totalGoodsAndServices = 0D;
        private string cashLocal = "";
        private string checkLocal = "";
        private string workDatabase = "SMFS";
        /****************************************************************************************/
        public FunPayments(DevExpress.XtraEditors.XtraForm mainControl, string contract, string payer = "", bool loading = false, bool noEdit = false )
        {
            InitializeComponent();
            workContract = contract;
            myWorkContract = contract;

            this.Tag = workContract;

            workPayer = payer;
            myWorkPayer = payer;

            workNoEdit = noEdit;

            workControl = mainControl;
            SetupTotalsSummary();
            totalFiled = 0D;
            totalReceived = 0D;
            totalDiscount = 0D;
            totalAmountDiscount = 0D;
            totalAmountGrowth = 0D;
            totalModified = false;
            justLoading = loading;
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox12, "Add New Payment");
            tt.SetToolTip(this.pictureBox11, "Cancel Payment");
            tt.SetToolTip(this.pictureBox1, "Select Insurance Policy Payment");
        }
        /****************************************************************************************/
        private void LoadupTitle()
        {
            this.Text = "Payments for Contract " + workContract;
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string name = dt.Rows[0]["lastName"].ObjToString() + ", " + dt.Rows[0]["firstName"].ObjToString();
            workDeceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
            string title = "Payments for (" + workContract + ") " + name;
            if (!String.IsNullOrWhiteSpace(workServiceId))
                title += " Service Id: " + workServiceId;
            workSSN = dt.Rows[0]["ssn"].ObjToString();
            workBirthday = dt.Rows[0]["birthDate"].ObjToDateTime();
            this.Text = title;
        }
        /****************************************************************************************/
        private void FunPayments_Load(object sender, EventArgs e)
        {
            workDatabase = G1.conn1.Database.ObjToString();

            oldWhat = "";
            LoadupTitle();

            if (G1.isAdminOrSuper() || G1.isHomeOffice())
                gridMain.Columns["dateEntered"].OptionsColumn.AllowEdit = true;

            LoadFuneralTypes();

            //pictureBox1.Hide(); // Just do this for now. May come in handy later
            custExtendedRecord = "";
            if (justLoading)
                this.Hide();
            //label3.Hide();
            SetupToolTips();
            gridMain.OptionsView.AllowHtmlDrawGroups = true;
            btnSavePayments.Hide();
            funModified = false;
            loading = true;
            DateTime now = DateTime.Now;
            txtAsOf.Text = now.ToString("MM/dd/yyyy");

            LoadData();

            loading = false;
            if ( workNoEdit )
            {
                this.pictureBox11.Hide();
                this.pictureBox12.Hide();
                this.btnSavePayments.Hide();
            }
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("payment", null);
            AddSummaryColumn("grossAmountReceived", null);
            AddSummaryColumn("amountDiscount", null);
            AddSummaryColumn("amountGrowth", null);
            //AddSummaryColumn("amountReceived", null);
            //AddSummaryColumn("amountFiled", null);
            //AddSummaryColumn("custPrice", null);
            //AddSummaryColumn("custMerchandise", null);
            //AddSummaryColumn("custServices", null);
            //AddSummaryColumn("totalDiscount", null);
            //AddSummaryColumn("merchandiseDiscount", null);
            //AddSummaryColumn("servicesDiscount", null);
            //AddSummaryColumn("currentPrice", null);
            //AddSummaryColumn("currentMerchandise", null);
            //AddSummaryColumn("currentServices", null);
            //AddSummaryColumn("balanceDue", null);
            //AddSummaryColumn("additionalDiscount", null);

            //AddSummaryColumn("currentprice", null);
            //AddSummaryColumn("difference", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void LoadFuneralTypes ()
        {
            string cmd = "Select * from `ref_funeral_type`;";
            DataTable dt = G1.get_db_data(cmd);
            cmbFunType.DataSource = dt;
            cmbFunType.Text = "";
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `cust_payments` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);

            AddMod(dt, gridMain);
            dt.Columns.Add("initialized");

            string pendingComment = "";
            custExtendedRecord = "";
            cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                cmd = "Select * from `cust_extended` where `contractNumber` = '" + workContract + "';";
                dx = G1.get_db_data(cmd);
            }
            if (dx.Rows.Count > 0)
            {
                custExtendedRecord = dx.Rows[0]["record"].ObjToString();
                pendingComment = dx.Rows[0]["pendingComment"].ObjToString();
                workServiceId = dx.Rows[0]["serviceId"].ObjToString();
                GetDepositBankAccounts();

                cmbFunType.Text = dx.Rows[0]["contractType"].ObjToString();
            }

            rtb.Text = pendingComment;

            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("discount", Type.GetType("System.Double"));
            dt.Columns.Add("growth", Type.GetType("System.Double"));

            double payment = 0D;
            double amountReceived = 0D;
            double discount = 0D;
            double growth = 0D;
            string type = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type != "TRUST" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" )
                    continue;
                discount = 0D;
                growth = 0D;
                payment = dt.Rows[i]["payment"].ObjToDouble();
                amountReceived = dt.Rows[i]["amountReceived"].ObjToDouble();
                if (amountReceived > 0D)
                {
                    if (amountReceived > payment)
                        growth = amountReceived - payment;
                    else if (amountReceived < payment)
                        discount = payment - amountReceived;
                }
                dt.Rows[i]["discount"] = discount;
                dt.Rows[i]["growth"] = growth;
            }
            dgv.DataSource = dt;

            DataTable fsTable = null;

            //if (editFunServices == null)
            //    InitializeServicePanel();
            //if (editFunPayments == null)
            //    InitializePaymentsPanel();

            //DataTable servicesDt = editFunServices.FireEventFunServicesReturn();


            //using (FunServices fs = new FunServices(workContract))
            //{
            //    fs.ShowDialog();
            //    fsTable = fs.Answer;
            //}

            DataTable funDt = null;
            FunServices form = (FunServices)G1.IsFormOpen("FunServices", workContract);
            if (form != null)
                funDt = form.FireEventFunServicesReturn();

            if (funDt == null)
            {
                FunServices serviceForm = new FunServices(workContract);
                funDt = serviceForm.funServicesDT;
            }

            double price = 0D;
            double total = 0D;
            string data = "";
            double totalPrice = 0D;
            double customerPrice = 0D;

            funDt = FunServices.RemoveEmptyDiscretionary(funDt);

            for ( int i=0; i<funDt.Rows.Count; i++)
            {
                price = funDt.Rows[i]["price"].ObjToDouble();
                if (price > 0D || workDatabase.ToUpper() != "SMFS" )
                {
                    customerPrice += price;
                    price = funDt.Rows[i]["currentPrice"].ObjToDouble();
                    total += price;
                    totalPrice += price;
                }
            }
            price = 0D;

            //cmd = "Select * from `cust_services` where `contractNumber` = '" + workContract + "';";

            //totalMerchandise = 0D;
            //totalServices = 0D;
            //type = "";

            //dx = G1.get_db_data(cmd);
            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    price = 0D;
            //    data = dx.Rows[i]["price"].ObjToString();
            //    data = data.Replace(",", "");
            //    data = data.Replace("$", "");
            //    if (G1.validate_numeric(data))
            //    {
            //        price = data.ObjToDouble();
            //        total += price;
            //    }
            //    type = dx.Rows[i]["type"].ObjToString().ToUpper();
            //    if (type == "MERCHANDISE")
            //        totalMerchandise += price;
            //    else if (type == "SERVICE")
            //        totalServices += price;
            //}

            //price = ResolveImportedData();
            //total += price;
            //totalMerchandise += price;

            data = G1.ReformatMoney(total);
            txtGoods.Text = "$" + data;
            totalGoodsAndServices = total;

            ReCalcTotal(funDt);

            CalculateAmountDue( true );

            LoadupTitle();

            CleanupFieldColumns();

            funModified = false;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ReCalcTotal(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            string select = "";
            string ignore = "";
            string who = "";
            double price = 0D;
            double customerDiscount = 0D;
            string type = "";
            string deleted = "";
            double servicesTotal = 0D;
            double merchandiseTotal = 0D;
            double cashAdvanceTotal = 0D;

            double ignoreServices = 0D;
            double ignoreMerchandise = 0D;
            double ignoreCashAdvance = 0D;

            double totalListedPrice = 0D;
            double packagePrice = 0D;
            double packageDiscount = 0D;
            double totalUnselected = 0D;
            int packageDiscountRow = -1;

            double salesTax = 0D;
            double tax = 0D;

            double grandTotal = 0D;
            double actualDiscount = 0D;
            string isPackage = "";

            if (G1.get_column_number(dt, "DELETED") < 0)
                dt.Columns.Add("DELETED");

            string currentPriceColumn = "currentprice";
            if (G1.get_column_number(dt, "currentprice") < 0)
            {
                if (G1.get_column_number(dt, "price1") >= 0)
                {
                    dt.Columns["price1"].ColumnName = "currentprice";
                }
                else
                    currentPriceColumn = "price";
            }
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));

            bool gotPackage = FunServices.DoWeHavePackage(dt);
            string service = "";

            FunServices.PreProcessUrns(dt);

            double upgrade = 0D;

            FunServices.AddUpgrade(dt);

            double totalServices = 0D;
            double totalMerchandise = 0D;
            double totalCashAdvance = 0D;
            double difference = 0D;

            bool myPackage = FunServices.GetPackageDetails(dt, ref totalListedPrice, ref packageDiscount, ref packagePrice, ref totalServices, ref totalMerchandise, ref totalCashAdvance, ref actualDiscount, ref grandTotal);
            if (myPackage)
                currentPriceColumn = "price";

            string pSelect = "";
            double urnCredit = 0D;
            double alterCredit = 0D;

            bool allIsPackage = true;
            double added = 0D;
            double totalUpgrades = 0D;

            string zeroData = "";

            string database = G1.conn1.Database.ObjToString();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                    if (deleted == "DELETED" || deleted == "D")
                        continue;

                    select = dt.Rows[i]["select"].ObjToString();
                    service = dt.Rows[i]["service"].ObjToString();
                    if (service == "Register Book And Pouch")
                    {
                    }
                    if (service == "Alternative Container")
                    {
                    }
                    type = dt.Rows[i]["type"].ObjToString().ToUpper();
                    ignore = dt.Rows[i]["ignore"].ObjToString();
                    who = dt.Rows[i]["who"].ObjToString();
                    price = dt.Rows[i]["price"].ObjToDouble();
                    upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                    if (upgrade > 0D)
                    {
                        totalUpgrades += upgrade;
                    }

                    zeroData = dt.Rows[i]["data"].ObjToString().ToUpper();

                    if (myPackage)
                    {
                        isPackage = dt.Rows[i]["isPackage"].ObjToString().ToUpper();
                        if (isPackage == "P")
                        {
                            if (service.ToUpper().IndexOf("URN CREDIT") >= 0)
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                urnCredit = dt.Rows[i]["price"].ObjToDouble();
                                //dt.Rows[i]["price"] = 0D;
                            }
                            else if (service.ToUpper().IndexOf("ALTERNATIVE CONTAINER CREDIT") >= 0)
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                alterCredit = dt.Rows[i]["price"].ObjToDouble();
                                //dt.Rows[i]["price"] = 0D;
                            }
                            else if (service.ToUpper().IndexOf("CREMATION CASKET CREDIT") >= 0)
                            {
                                pSelect = dt.Rows[i]["pSelect"].ObjToString();
                                if (pSelect == "1")
                                    continue;
                                alterCredit = dt.Rows[i]["price"].ObjToDouble();
                                //dt.Rows[i]["price"] = 0D;
                            }
                            else
                            {
                                if (select == "1")
                                {
                                    price = dt.Rows[i]["price"].ObjToDouble();
                                    if (price <= 0D && upgrade <= 0D)
                                    {
                                        if ( zeroData != "ZERO")
                                            continue;
                                    }
                                    if (upgrade > 0D)
                                    {
                                        if (type.ToUpper() == "MERCHANDISE")
                                        {
                                            merchandiseTotal += upgrade;
                                            if (ignore == "Y")
                                                ignoreMerchandise += price;
                                        }
                                    }
                                    if (ignore == "Y")
                                    {
                                        if (type == "SERVICE")
                                            ignoreServices += price;
                                        else if (type == "MERCHANDISE" && upgrade <= 0D)
                                            ignoreMerchandise += price;
                                        else if (type == "CASH ADVANCE")
                                            ignoreCashAdvance += price;
                                    }
                                }
                                continue;
                            }
                        }
                        else
                        {
                            allIsPackage = false;
                            if (price > 0D)
                                added += price;
                        }
                    }
                    if (service.ToUpper() == "TOTAL LISTED PRICE")
                    {
                        if (select == "0")
                            continue;
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        totalListedPrice = packagePrice;
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service.ToUpper() == "PACKAGE PRICE")
                    {
                        if (select == "0")
                            continue;
                        packagePrice = dt.Rows[i]["price"].ObjToDouble();
                        if (packagePrice > 0)
                            gotPackage = true;
                        continue;
                    }
                    else if (service.ToUpper() == "PACKAGE DISCOUNT")
                    {
                        if (select == "0")
                        {
                            //mainPackageDiscount = 0D;
                            continue;
                        }
                        packageDiscount = dt.Rows[i]["price"].ObjToDouble();
                        packageDiscountRow = i;
                        customerDiscount = packageDiscount;
                        continue;
                    }

                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "0")
                    {
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (database.Trim().ToUpper() != "SMFS")
                        {
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            continue;
                        }
                        if (price < 0D)
                        {
                            price = Math.Abs(price);
                            difference = dt.Rows[i]["difference"].ObjToDouble();
                            customerDiscount += difference;
                            //if (type.ToUpper() == "SERVICE")
                            //    servicesTotal += price;
                            //else if (type.ToUpper() == "MERCHANDISE")
                            //    merchandiseTotal += price;
                            //else if (type.ToUpper() == "CASH ADVANCE")
                            //    cashAdvanceTotal += price;
                        }
                        else
                        {
                            price = dt.Rows[i]["difference"].ObjToDouble();
                            if (myPackage && price == 0D)
                                price = dt.Rows[i]["price"].ObjToDouble();
                            customerDiscount -= price;
                        }
                        continue;
                    }
                    if (select == "1")
                    {

                        tax = dt.Rows[i]["taxAmount"].ObjToDouble();
                        if (tax > 0D)
                        {
                            tax = G1.RoundValue(tax);
                            salesTax += tax;
                        }

                        type = dt.Rows[i]["type"].ObjToString();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        if (service.IndexOf("(Refund)") > 0)
                        {
                            if (type.ToUpper() == "SERVICE")
                                servicesTotal += price;
                            else if (type.ToUpper() == "MERCHANDISE")
                                merchandiseTotal += price;
                            continue;
                        }
                        if (price <= 0D && upgrade <= 0D)
                        {
                            if ( zeroData != "ZERO" )
                                continue;
                        }
                        price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                        if (gotPackage)
                        {
                            price = dt.Rows[i]["price"].ObjToDouble();
                            price = Math.Abs(price);
                        }
                        customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                        if (type.ToUpper() == "SERVICE")
                        {
                            servicesTotal += price;
                            if (ignore == "Y")
                                ignoreServices += price;
                        }
                        else if (type.ToUpper() == "MERCHANDISE")
                        {
                            merchandiseTotal += price;
                            if (ignore == "Y")
                                ignoreMerchandise += price;
                        }
                        else if (type.ToUpper() == "CASH ADVANCE")
                        {
                            cashAdvanceTotal += price;
                            if (ignore == "Y")
                                ignoreCashAdvance += price;
                        }
                    }
                    else
                    {
                        type = dt.Rows[i]["type"].ObjToString().ToUpper();
                        if (gotPackage && type != "CASH ADVANCE")
                        {
                            upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                            price = dt.Rows[i]["price"].ObjToDouble();
                            if (price <= 0D && upgrade <= 0D)
                            {
                                if ( zeroData != "ZERO")
                                    continue;
                            }
                            price = dt.Rows[i][currentPriceColumn].ObjToDouble();
                            customerDiscount += dt.Rows[i]["difference"].ObjToDouble();
                            //if (type.ToUpper() == "SERVICE")
                            //    servicesTotal += price;
                            //else if (type.ToUpper() == "MERCHANDISE")
                            //    merchandiseTotal += price;
                            //else if (type.ToUpper() == "CASH ADVANCE")
                            //    cashAdvanceTotal += price;
                            totalUnselected += price;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            try
            {
                if (!myPackage)
                {
                    CalculateContractDifference(dt);
                    customerDiscount = 0D;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
                        if (deleted == "DELETED" || deleted == "D")
                            continue;

                        select = dt.Rows[i]["select"].ObjToString();
                        price = dt.Rows[i]["price"].ObjToDouble();
                        upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
                        if (upgrade > 0D)
                        {

                        }
                        zeroData = dt.Rows[i]["data"].ObjToString().ToUpper();
                        if (price <= 0D && upgrade > 0D)
                            price = upgrade;
                        if (price == 0D)
                        {
                            if ( zeroData != "ZERO" )
                            continue;
                        }
                        price = dt.Rows[i]["difference"].ObjToDouble();
                        if (select == "1")
                            customerDiscount = customerDiscount + price;
                    }

                }
                double totalIgnore = ignoreServices + ignoreMerchandise + ignoreCashAdvance;

                string money = G1.ReformatMoney(servicesTotal + totalServices - ignoreServices);
                //txtServices.Text = money;
                //txtServices.Refresh();

                money = G1.ReformatMoney(merchandiseTotal + totalMerchandise - ignoreMerchandise);
                //txtMerchandise.Text = money;
                //txtMerchandise.Refresh();

                money = G1.ReformatMoney(cashAdvanceTotal + totalCashAdvance - ignoreCashAdvance);
                //txtCashAdvance.Text = money;
                //txtCashAdvance.Refresh();

                double actualCashAdvance = cashAdvanceTotal + totalCashAdvance - ignoreCashAdvance;

                double subtotal = servicesTotal + merchandiseTotal + cashAdvanceTotal + totalCashAdvance + totalServices + totalMerchandise - totalIgnore;
                //subtotal += salesTax;
                money = G1.ReformatMoney(subtotal);
                //txtSubtotal.Text = money;
                //txtSubtotal.Refresh();
                txtGoods.Text = money;
                txtGoods.Refresh();

                double newDiscount = 0D;

                double total = subtotal;
                if (gotPackage)
                {
                    //money = G1.ReformatMoney(actualDiscount + totalIgnore + urnCredit);
                    //txtDiscount.Text = money;
                    //txtDiscount.Refresh();
                    //total = packagePrice + cashAdvanceTotal + servicesTotal + merchandiseTotal - urnCredit;
                    //total = total + (actualDiscount + totalIgnore);

                    //total = subtotal + (actualDiscount + totalIgnore);

                    //total = packagePrice + added - urnCredit;
                    //money = G1.ReformatMoney(subtotal - total + urnCredit);
                    //txtDiscount.Text = money;
                    //txtDiscount.Refresh();
                    ////total = total + totalUpgrades - urnCredit;
                    //total = total - urnCredit;

                    money = G1.ReformatMoney(actualDiscount + totalIgnore - totalUpgrades) ;
                    txtPreDiscount.Text = money;
                    txtPreDiscount.Refresh();
                    //total = packagePrice + cashAdvanceTotal + servicesTotal + merchandiseTotal - urnCredit;
                    //total = total + (actualDiscount + totalIgnore);

                    total = subtotal + (actualDiscount + totalIgnore);

                    total = packagePrice + added - urnCredit - alterCredit;
                    total = packagePrice + added;
                    newDiscount = subtotal - total + urnCredit + alterCredit - totalUpgrades;
                    if (newDiscount > Math.Abs(actualDiscount))
                        newDiscount = actualDiscount;
                    money = G1.ReformatMoney( newDiscount );
                    txtPreDiscount.Text = money;
                    txtPreDiscount.Refresh();
                    //total = total + totalUpgrades - urnCredit;
                    total = total - urnCredit - alterCredit;

                }
                else
                {
                    if ( customerDiscount > 0D)
                    {
                        newDiscount = G1.RoundValue(customerDiscount - totalIgnore - totalUpgrades);
                        customerDiscount = newDiscount;
                    }
                    double discount = customerDiscount * -1D;
                    money = G1.ReformatMoney(discount);
                    txtPreDiscount.Text = money;
                    txtPreDiscount.Refresh();
                    total = total + discount;
                }

                subtotal += salesTax;
                money = G1.ReformatMoney(subtotal);
                txtGoods.Text = money;
                txtGoods.Refresh();

                //total -= salesTax;
                money = G1.ReformatMoney(total);
                txtTotalDue.Text = money;
                txtTotalDue.Refresh();

                CalculateAmountDue ( false );

                //money = G1.ReformatMoney(salesTax);
                //txtSalesTax.Text = money;
                //txtSalesTax.Refresh();
            }
            catch (Exception ex)
            {
            }

            //ProcessPackage(dt);
        }
        /***********************************************************************************************/
        private void CalculateContractDifference(DataTable dt)
        {
            double price = 0D;
            double currentprice = 0D;
            double difference = 0D;
            if (G1.get_column_number(dt, "difference") < 0)
                dt.Columns.Add("difference", Type.GetType("System.Double"));
            string select = "";
            string zero = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                select = dt.Rows[i]["select"].ObjToString();
                if (select != "1")
                    continue;
                price = dt.Rows[i]["price"].ObjToDouble();
                zero = dt.Rows[i]["data"].ObjToString().ToUpper();

                if (price > 0D || zero == "ZERO" )
                {
                    currentprice = dt.Rows[i]["currentprice"].ObjToDouble();
                    difference = currentprice - price;
                    dt.Rows[i]["difference"] = difference;
                }
                else
                    dt.Rows[i]["difference"] = 0D;
            }
        }
        /***********************************************************************************************/
        //private void ReCalcTotal(DataTable dt)
        //{
        //    string select = "";
        //    double price = 0D;
        //    double customerDiscount = 0D;
        //    string type = "";
        //    string deleted = "";
        //    double servicesTotal = 0D;
        //    double merchandiseTotal = 0D;
        //    double cashAdvanceTotal = 0D;
        //    bool gotPackage = false;

        //    double packagePrice = 0D;
        //    double packageDiscount = 0D;
        //    double totalUnselected = 0D;
        //    int packageDiscountRow = -1;

        //    if (G1.get_column_number(dt, "DELETED") < 0)
        //        dt.Columns.Add("DELETED");

        //    string currentPriceColumn = "currentPrice";
        //    if (G1.get_column_number(dt, "currentPrice") < 0)
        //        currentPriceColumn = "price";

        //    bool gotDifference = false;
        //    if (G1.get_column_number(dt, "difference") >= 0)
        //        gotDifference = true;

        //    gotPackage = FunServices.DoWeHavePackage(dt);
        //    string service = "";
        //    FunServices.PreProcessUrns(dt);
        //    double upgrade = 0D;
        //    double difference = 0D;
        //    string isPackage = "";

        //    FunServices.AddUpgrade(dt);

        //    double totalListedPrice = 0D;
        //    double totalCashAdvance = 0D;
        //    double actualDiscount = 0D;
        //    double grandTotal = 0D;

        //    double added = 0D;

        //    bool myPackage = FunServices.GetPackageDetails(dt, ref totalListedPrice, ref packageDiscount, ref packagePrice, ref totalServices, ref totalMerchandise, ref totalCashAdvance, ref actualDiscount, ref grandTotal);


        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            deleted = dt.Rows[i]["DELETED"].ObjToString().ToUpper();
        //            if (deleted == "DELETED" || deleted == "D")
        //                continue;

        //            service = dt.Rows[i]["service"].ObjToString();

        //            if (service.ToUpper() == "TOTAL LISTED PRICE")
        //            {
        //                packagePrice = dt.Rows[i]["price"].ObjToDouble();
        //                if (packagePrice > 0)
        //                    gotPackage = true;
        //                continue;
        //            }
        //            else if (service.ToUpper() == "PACKAGE PRICE")
        //            {
        //                packagePrice = dt.Rows[i]["price"].ObjToDouble();
        //                if (packagePrice > 0)
        //                    gotPackage = true;
        //                continue;
        //            }
        //            else if (service.ToUpper() == "PACKAGE DISCOUNT")
        //            {
        //                packageDiscount = dt.Rows[i]["price"].ObjToDouble();
        //                packageDiscountRow = i;
        //                customerDiscount = packageDiscount;
        //                continue;
        //            }

        //            select = dt.Rows[i]["select"].ObjToString();
        //            if (select == "1")
        //            {
        //                type = dt.Rows[i]["type"].ObjToString();
        //                isPackage = dt.Rows[i]["isPackage"].ObjToString().ToUpper();
        //                upgrade = dt.Rows[i]["upgrade"].ObjToDouble();
        //                price = dt.Rows[i]["price"].ObjToDouble();
        //                if (price <= 0D && upgrade <= 0D)
        //                {
        //                    if ( workDatabase.ToUpper() == "SMFS" )
        //                        continue;
        //                }
        //                price = dt.Rows[i][currentPriceColumn].ObjToDouble();
        //                if (gotDifference)
        //                {
        //                    difference = dt.Rows[i]["difference"].ObjToDouble();
        //                    if (upgrade > 0D)
        //                        difference = upgrade;
        //                    if ( myPackage )
        //                        difference = 0D;
        //                    customerDiscount += difference;
        //                }
        //                if (type.ToUpper() == "SERVICE")
        //                    servicesTotal += price;
        //                else if (type.ToUpper() == "MERCHANDISE")
        //                    merchandiseTotal += price;
        //                else if (type.ToUpper() == "CASH ADVANCE")
        //                    cashAdvanceTotal += price;
        //                if (myPackage)
        //                {
        //                    if (dt.Rows[i]["pSelect"].ObjToString().ToUpper() != "P")
        //                    {
        //                        //price = dt.Rows[i]["price"].ObjToDouble();
        //                        if (price > 0D)
        //                            added += price;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                type = dt.Rows[i]["type"].ObjToString().ToUpper();
        //                if (gotPackage && type != "CASH ADVANCE")
        //                    totalUnselected += dt.Rows[i]["price"].ObjToDouble();
        //                //if ( myPackage )
        //                //{
        //                //    if (dt.Rows[i]["pSelect"].ObjToString().ToUpper() != "P")
        //                //    {
        //                //        price = dt.Rows[i]["price"].ObjToDouble();
        //                //        if ( price > 0D )
        //                //            added += price;
        //                //    }
        //                //}
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //    string money = G1.ReformatMoney(servicesTotal);
        //    //txtServices.Text = money;
        //    //txtServices.Refresh();
        //    //money = G1.ReformatMoney(merchandiseTotal);
        //    //txtMerchandise.Text = money;
        //    //txtMerchandise.Refresh();
        //    //money = G1.ReformatMoney(cashAdvanceTotal);
        //    //txtCashAdvance.Text = money;
        //    //txtCashAdvance.Refresh();
        //    double subtotal = servicesTotal + merchandiseTotal;
        //    money = G1.ReformatMoney(subtotal);
        //    //txtSubtotal.Text = money;
        //    //txtSubtotal.Refresh();
        //    double total = subtotal + cashAdvanceTotal;
        //    if (gotPackage)
        //        total = packagePrice + cashAdvanceTotal;
        //    money = G1.ReformatMoney(total);
        //    //txtTotal.Text = money;
        //    //txtTotal.Refresh();
        //    //if (gotPackage)
        //    //{
        //    //    money = G1.ReformatMoney(mainPackageDiscount + totalUnselected);
        //    //    txtDiscount.Text = money;
        //    //    if (packageDiscountRow >= 0)
        //    //        dt.Rows[packageDiscountRow]["price"] = mainPackageDiscount + totalUnselected;
        //    //}
        //    //else
        //    //{
        //    //    money = G1.ReformatMoney(customerDiscount);
        //    //    txtDiscount.Text = money;
        //    //}
        //    //txtDiscount.Refresh();

        //    if (gotPackage)
        //    {
        //        double urnCredit = 0D;
        //        double totalIgnore = 0D;
        //        customerDiscount = Math.Abs(customerDiscount);
        //        total = subtotal - customerDiscount + cashAdvanceTotal;
        //        double totalPrice = subtotal + cashAdvanceTotal;
        //        totalGoodsAndServices = totalPrice;

        //        money = G1.ReformatMoney(totalPrice);

        //        txtGoods.Text = money;

        //        total = packagePrice + cashAdvanceTotal + servicesTotal + merchandiseTotal;
        //        total = total + (actualDiscount + totalIgnore);

        //        total = subtotal + (actualDiscount + totalIgnore);

        //        total = packagePrice + added;
        //        money = G1.ReformatMoney(totalPrice - total);
        //        txtPreDiscount.Text = money;

        //        //total = packagePrice + added;
        //        //money = G1.ReformatMoney(totalPrice - total);
        //        //txtPreDiscount.Text = money;

        //    }
        //    else
        //    {
        //        customerDiscount = Math.Abs(customerDiscount);
        //        total = subtotal - customerDiscount + cashAdvanceTotal;
        //        double totalPrice = subtotal + cashAdvanceTotal;
        //        totalGoodsAndServices = totalPrice;

        //        money = G1.ReformatMoney(totalPrice);

        //        txtGoods.Text = money;

        //        money = G1.ReformatMoney(customerDiscount);
        //        txtPreDiscount.Text = money;
        //    }
        //    //txtTotal.Text = money;
        //    //txtTotal.Refresh();
        //}
        /***********************************************************************************************/
        private void CleanupFieldColumns()
        {
            if (LoginForm.classification.ToUpper() != "FIELD")
                return;
            gridMain.Columns["amountGrowth"].Visible = false;
            gridMain.Columns["amountDiscount"].Visible = false;
            gridMain.Columns["grossAmountReceived"].Visible = false;
        }
        /***********************************************************************************************/
        private void GetDepositBankAccounts()
        {
            if (String.IsNullOrWhiteSpace(workServiceId))
                return;
            string trust = "";
            string loc = "";
            string contract = Trust85.decodeContractNumber(workServiceId, ref trust, ref loc);
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + loc + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                cashLocal = dt.Rows[0]["cashLocal"].ObjToString();
                checkLocal = dt.Rows[0]["checkLocal"].ObjToString();
            }
        }
        /***********************************************************************************************/
        private void CalculateAmountDue( bool discountDone = false )
        {
            try
            {
                string data = txtGoods.Text.Trim();
                data = data.Replace("$", "");
                data = data.Replace(",", "");
                double total = data.ObjToDouble();

                data = txtPreDiscount.Text.Trim();
                data = data.Replace("$", "");
                data = data.Replace(",", "");
                double totalDiscount = data.ObjToDouble();

                DateTime payoffDate = txtAsOf.Text.ObjToDateTime();
                if (payoffDate.Year < 100)
                    payoffDate = DateTime.Now;
                double payoff = DailyHistory.CalculatePayoff(workContract, payoffDate);
                payoff = 0D;

                double totalPayments = total - payoff;

                double discount = 0D;

                totalPayments = calculateTotalPayments( ref discount );
                //if ( discount > 0D && !discountDone )
                //{
                //    totalDiscount = totalDiscount - Math.Abs(discount);
                //    data = "$" + G1.ReformatMoney(totalDiscount);
                //    txtPreDiscount.Text = data;

                //}
                data = G1.ReformatMoney(totalPayments);
                txtPayments.Text = "$" + data;
                txtPayments.Refresh();

                double totalDue = total - totalPayments - Math.Abs ( totalDiscount);
                data = G1.ReformatMoney(totalDue);
                txtTotalDue.Text = "$" + data;
                txtTotalDue.Refresh();
            }
            catch ( Exception ex)
            {
            }
            panelClaimTop.Refresh();
        }
        /***********************************************************************************************/
        private double ResolveImportedData()
        {
            double goods = 0D;
            string cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return goods;

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            double casketPrice = 0D;
            double vaultPrice = 0D;

            string casketCode = dx.Rows[0]["extraItemAmtMI1"].ObjToString();
            if (casketCode.ToUpper().IndexOf("-BAD") < 0)
                casketPrice = dx.Rows[0]["extraItemAmtMR1"].ObjToDouble();
            string vaultCode = dx.Rows[0]["extraItemAmtMI2"].ObjToString();
            if (vaultCode.ToUpper().IndexOf("-BAD") < 0)
                vaultPrice = dx.Rows[0]["extraItemAmtMR2"].ObjToDouble();
            goods += casketPrice + vaultPrice;
            return goods;
        }
        ///****************************************************************************************/
        //private DataTable saveDt = null;
        //private void pictureBox3_Click(object sender, EventArgs e)
        //{
        //}
        /***************************************************************************************/
        public void FireEventFunReloadPayments()
        {
            double price = 0D;
            double total = 0D;
            string data = "";

            FunServices serviceForm = new FunServices(workContract);
            DataTable funDt = serviceForm.funServicesDT;



            for (int i = 0; i < funDt.Rows.Count; i++)
            {
                price = funDt.Rows[i]["price"].ObjToDouble();
                price += funDt.Rows[i]["taxAmount"].ObjToDouble();
                total += price;
            }

            //string cmd = "Select * from `cust_services` where `contractNumber` = '" + workContract + "';";

            //DataTable dx = G1.get_db_data(cmd);
            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    data = dx.Rows[i]["price"].ObjToString();
            //    data = data.Replace(",", "");
            //    data = data.Replace("$", "");
            //    if (String.IsNullOrWhiteSpace(data))
            //    {
            //        data = dx.Rows[i]["price"].ObjToString();
            //        data = data.Replace(",", "");
            //        data = data.Replace("$", "");
            //    }
            //    if (G1.validate_numeric(data))
            //    {
            //        price = data.ObjToDouble();
            //        total += price;
            //    }
            //}

            //total += ResolveImportedData();

            data = G1.ReformatMoney(total);
            txtGoods.Text = "$" + data;

            ReCalcTotal(funDt);

            CalculateAmountDue( true );
        }
        /***************************************************************************************/
        public DevExpress.XtraGrid.GridControl FireEventPrintPreview()
        {
            return dgv;
        }
        /***************************************************************************************/
        public void FireEventUpdateServiceId ( string newServiceId )
        {
            workServiceId = newServiceId;
        }
        /***************************************************************************************/
        public DataTable FireEventFunPaymentsReturn()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            return dt;
        }
        /***************************************************************************************/
        public bool FireEventFunServicesOkayToClose()
        {
            if (funModified)
            {
                bool okay = validateClosing();
                return okay;
            }
            return true;
        }
        /***************************************************************************************/
        public bool FireEventFunServicesModified()
        {
            if (funModified)
                return true;
            return false;
        }
        /***************************************************************************************/
        public void FireEventSaveFunServices(bool save = false)
        {
            if (save && funModified)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                SaveCustomerPayments(dt);
            }
            this.Close();
        }
        /****************************************************************************************/
        private void panelClaimTop_Paint(object sender, PaintEventArgs e)
        {
            //Rectangle rect = panelClaimTop.Bounds;
            //Graphics g = panelClaimTop.CreateGraphics();
            //Pen pen = new Pen(Brushes.Black);
            //int left = rect.Left;
            //int top = rect.Top;
            //int width = rect.Width - 1;
            //int high = rect.Height - 1;
            //g.DrawRectangle(pen, left, top, width, high);
        }
        /****************************************************************************************/
        private void panelBottom_Paint(object sender, PaintEventArgs e)
        {
            //Rectangle rect = panelBottom.Bounds;
            //Graphics g = panelBottom.CreateGraphics();
            //Pen pen = new Pen(Brushes.Black);
            //int left = rect.Left;
            //int top = rect.Top;
            //int width = rect.Width - 1;
            //int high = rect.Height - 1;
            //g.DrawRectangle(pen, left, top, width, high);
        }
        /***********************************************************************************************/
        private void CheckForSaving()
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            if (!funModified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nPayments have been modified!\nWould you like to SAVE your Payments?", "Payments Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            //string type = dt.Rows[row]["type"].ObjToString().ToUpper();
        }
        /****************************************************************************************/
        private double GetTotalDue ()
        {
            string str = this.txtTotalDue.Text;
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            double totalDue = str.ObjToDouble();
            return totalDue;
        }
        /****************************************************************************************/
        private void btnSavePayments_Click(object sender, EventArgs e)
        {
            bool okay = validateClosing();
            if (!okay)
                return;
            //double totalDue = GetTotalDue();
            //string pendingComment = rtb.Text.Trim();
            //if ( totalDue > 0D && String.IsNullOrWhiteSpace ( pendingComment))
            //{
            //    DialogResult result = MessageBox.Show("***Warning***\nPayments have been modified but not completed!\nYou must enter reasons for pending payments!", "Pending Payments Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //    return;
            //}
            if ( workDatabase.ToUpper() != "SMFS" )
            {
                funModified = false;
                DataTable dx = (DataTable)dgv.DataSource;
                SaveCustomerPayments(dx);
                btnSavePayments.Hide();
                btnSavePayments.Refresh();
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            SaveCustomerPayments(dt);
            btnSavePayments.Hide();
            btnSavePayments.Refresh();
        }
        /***********************************************************************************************/
        private void SaveCustomerPayments(DataTable dt)
        {
            string desc = "";
            string price = "";
            string type = "";
            string status = "";
            string record = "";
            string assignmentFrom = "";
            string trustOrPolicy = "";
            string pendingComment = rtb.Text.Trim();
            string amountFiled = "";
            string amountReceived = "";
            string amountDiscount = "";
            string amountGrowth = "";
            string grossAmountReceived = "";
            string depositNumber = "";
            string approvedBy = "";
            string bankAccount = "";
            string localDescription = "";
            string added = "Changed";
            string changeRecord = "";
            string what = "";
            string description = "";
            string[] Lines = null;
            string policyHolder = "";
            string policyNumber = "";
            string policyRecord = "";
            string payer = "";
            string names = "";
            string referenceNumber = "";
            string fName = "";
            string lName = "";
            string cmd = "";
            string message = "";
            DataTable ddx = null;
            totalModified = false;
            totalFiled = 0D;
            totalReceived = 0D;
            totalDiscount = 0D;
            totalAmountDiscount = 0D;
            totalAmountGrowth = 0D;
            double dValue = 0D;
            double totalGross = 0D;
            DateTime dateEntered = DateTime.Now;
            DateTime dateModified = DateTime.Now;
            //string cmd = "Delete from `cust_payments` where `contractNumber` = '" + workContract + "';";
            //G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    added = "Changed";
                    what = "";
                    record = dt.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("cust_payments", "description", "-1");
                        dt.Rows[i]["mod"] = "Y";
                        added = "Added";
                    }
                    if (G1.BadRecord("cust_payments", record))
                        break;
                    dt.Rows[i]["record"] = record;
                    if (dt.Rows[i]["mod"].ObjToString().ToUpper() != "Y")
                        continue;

                    desc = dt.Rows[i]["description"].ObjToString();
                    status = dt.Rows[i]["status"].ObjToString();
                    price = dt.Rows[i]["payment"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString();
                    assignmentFrom = dt.Rows[i]["assignment_from"].ObjToString();
                    names = dt.Rows[i]["names"].ObjToString();
                    referenceNumber = dt.Rows[i]["referenceNumber"].ObjToString();

                    trustOrPolicy = dt.Rows[i]["trust_policy"].ObjToString();
                    if (type.ToUpper() == "REFUND")
                    {
                        dValue = price.ObjToDouble();
                        dValue = Math.Abs(dValue);
                        dValue = dValue * -1D;
                        price = dValue.ToString();
                    }
                    if ((type.ToUpper().IndexOf("CLASS A") >= 0 || type.ToUpper().IndexOf("INS") >= 0) && added.ToUpper() == "ADDED")
                    {
                        Lines = trustOrPolicy.Split('/');
                        if (Lines.Length >= 2)
                        {
                            payer = Lines[0];
                            policyNumber = Lines[1];
                            if (Lines.Length > 2)
                            {
                                policyNumber += "/" + Lines[2];
                            }
                            Lines = names.Split(',');
                            if (Lines.Length >= 2)
                            {
                                lName = Lines[0].Trim();
                                fName = Lines[1].Trim();
                                if (!String.IsNullOrWhiteSpace(policyNumber) && !String.IsNullOrWhiteSpace(payer) && !String.IsNullOrWhiteSpace(lName) && !String.IsNullOrWhiteSpace(fName))
                                {
                                    UpdatePayerPolicies(payer, policyNumber, fName, lName, workDeceasedDate.ToString("yyyyMMdd"), workServiceId, false);

                                    message = "Accept Insurance " + this.Text + "\n";
                                    message += "Assignment for Payer " + payer + " for a Liability of " + price + "\n";
                                    message += "Policy # " + policyNumber + " Policy Name " + names;

                                    Messages.SendTheMessage(LoginForm.username, "cjenkins", "Insurance Assignment Payer (" + payer + ") Policy (" + policyNumber + ")", message);
                                }
                            }
                        }
                    }
                    dateEntered = dt.Rows[i]["dateEntered"].ObjToDateTime();
                    dateModified = dt.Rows[i]["dateModified"].ObjToDateTime();
                    amountFiled = dt.Rows[i]["amountFiled"].ObjToString();
                    amountReceived = dt.Rows[i]["amountReceived"].ObjToString();
                    amountDiscount = dt.Rows[i]["amountDiscount"].ObjToString();
                    amountGrowth = dt.Rows[i]["amountGrowth"].ObjToString();
                    grossAmountReceived = dt.Rows[i]["grossAmountReceived"].ObjToString();
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    approvedBy = dt.Rows[i]["approvedBy"].ObjToString();
                    bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                    localDescription = dt.Rows[i]["localDescription"].ObjToString();
                    totalFiled += amountFiled.ObjToDouble();
                    totalReceived += amountReceived.ObjToDouble();
                    totalAmountDiscount += amountDiscount.ObjToDouble();
                    totalAmountGrowth += amountGrowth.ObjToDouble();
                    totalGross += grossAmountReceived.ObjToDouble();
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        what = depositNumber;
                        if (!String.IsNullOrWhiteSpace(price))
                            what += " for " + price;
                    }
                    else if (!String.IsNullOrWhiteSpace(price))
                        what = "Payment " + price;

                    if (!String.IsNullOrWhiteSpace(type))
                    {
                        if (!String.IsNullOrWhiteSpace(what))
                            what += " ";
                        what += type;
                    }
                    G1.update_db_table("cust_payments", "record", record, new string[] { "description", desc, "type", type, "contractNumber", workContract, "payment", price, "status", status, "assignment_from", assignmentFrom, "trust_policy", trustOrPolicy, "dateEntered", dateEntered.ToString("yyyy-MM-dd"), "dateModified", dateModified.ToString("yyyy-MM-dd"), "amountFiled", amountFiled, "amountReceived", amountReceived, "depositNumber", depositNumber, "approvedBy", approvedBy, "grossAmountReceived", grossAmountReceived, "bankAccount", bankAccount, "localDescription", localDescription, "amountDiscount", amountDiscount, "amountGrowth", amountGrowth, "names", names, "referencenumber", referenceNumber });

                    //if (savedContracts)
                    //{
                    if (workDatabase.ToUpper() == "SMFS")
                    {
                        changeRecord = G1.create_record("fcust_changes", "what", "-1");
                        if (G1.BadRecord("fcust_changes", changeRecord))
                            continue;
                        G1.update_db_table("fcust_changes", "record", changeRecord, new string[] { "contractNumber", workContract, "action", added, "type", "Payment", "what", what, "user", LoginForm.username, "date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                    }
                    //}

                }
                catch ( Exception ex)
                {
                }
            }
            if (!String.IsNullOrWhiteSpace(custExtendedRecord))
            {
                totalModified = true;
                AddCustomerPayments();
            }
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            funModified = false;
            NotifyContract(dt);
        }
        /****************************************************************************************/
        private double ProcessDBR()
        {
            string status = "";
            double dbr = 0D;
            double totalDBR = 0D;
            DateTime monthDate = new DateTime(workDeceasedDate.Year, workDeceasedDate.Month, 1);
            string cmd = "Select * from `payments` where `contractNumber` = '" + workContract + "' AND `paydate8` >= '" + monthDate.ToString("yyyy-MM-dd") + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return totalDBR;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["fill"].ObjToString();
                if (status.ToUpper() == "D")
                    continue;
                dbr = dt.Rows[i]["trust100P"].ObjToDouble();
                totalDBR += dbr;
            }
            return totalDBR;
        }
        /***********************************************************************************************/
        private void NotifyContract(DataTable dt)
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                var form = Application.OpenForms[i];
                if (form.Visible)
                {
                    string text = form.Name.ObjToString();
                    if (text.ToUpper().IndexOf("CONTRACT1") >= 0)
                    {
                        text = form.Text;
                        Contract1 editForm = (Contract1)form;
                        string contract = editForm.myWorkContract;
                        if (contract == workContract)
                        {
                            editForm.FireEventFunPaymentsChanged(contract, dt);
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        public void FireEventServiceIdChanged ( string newServiceId )
        {
            workServiceId = newServiceId;
        }
        /****************************************************************************************/
        public void FireEventFunServicesChanged(string contract, DataTable dt)
        {
            if (contract == workContract)
            {
                LoadData();
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
                return;
            }
            int rowHandle = e.RowHandle;
            if (rowHandle < 0)
                return;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;

            bool protectField = false;
            //string type = dt.Rows[row]["type"].ObjToString().ToUpper();
        }
        /****************************************************************************************/
        private void btnShowPDF_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private bool justSaved = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            if ( justSaved )
            {
                justSaved = false;
                return;
            }
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            bool doBanks = false;
            dr["mod"] = "Y";
            dr["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
            if (e.Column.FieldName.Trim().ToUpper() == "STATUS")
            {
                string status = dr["status"].ObjToString().ToUpper();
                if (status.ToUpper() == "DEPOSITED")
                {
                    dr["grossAmountReceived"] = dr["payment"];
                    doBanks = true;
                }
                else
                {
                    dr["grossAmountReceived"] = 0D;
                }
                //CalculateAmountDue();
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "DEPOSITNUMBER")
            {
                dr["grossAmountReceived"] = dr["payment"];
                //CalculateAmountDue();
                doBanks = true;
            }
            else if ( e.Column.FieldName.Trim().ToUpper() == "TRUST_POLICY")
            {
                string newPolicy = dr["trust_policy"].ObjToString();
                //MessageBox.Show(" Old=" + oldPolicy + " New=" + newPolicy);
                if (!String.IsNullOrWhiteSpace(oldPolicy) && oldPolicy != newPolicy )
                {
                    string cmd = "Select * from `cust_payment_ins_checklist` where `contractNumber` = '" + workContract + "' and `serviceId` = '" + workServiceId + "' AND `policyNumber` = '" + oldPolicy + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        string record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("cust_payment_ins_checklist", "record", record, new string[] { "policyNumber", newPolicy });
                        record = dr["record"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(record) && record != "0" && record != "-1")
                            G1.update_db_table("cust_payments", "record", record, new string[] { "trust_policy", newPolicy });
                    }
                }
            }

            CalculateAmountDue();

            if (e.Column.FieldName.Trim().ToUpper() == "TYPE")
                doBanks = true;

            if ( doBanks )
            {
                string paymentType = dr["type"].ObjToString();
                string bankAccount = GetDepositBankAccount(paymentType);
                if (!String.IsNullOrWhiteSpace(bankAccount))
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    dr["bankAccount"] = bankAccount;
                    dt.Rows[row]["bankAccount"] = bankAccount;
                    //gridMain.RefreshRowCell(rowHandle, gridMain.Columns["bankAccount"]);
                    //gridMain.RefreshEditor(true);
                    //gridMain.RefreshRow(rowHandle);
                    //dgv.RefreshDataSource();
                    //dgv.Refresh();
                    //gridMain.SelectRow(rowHandle);
                    //gridMain.SelectCell(rowHandle, gridMain.Columns["bankAccount"]);
                    //gridMain.RefreshEditor(true);
                    //dgv.RefreshDataSource();
                }
            }

            SetupSave();
            gridMain.RefreshData();
        }
        /***********************************************************************************************/
        public static bool Mod10Check(string creditCardNumber)
        { //// check whether input string is null or empty 
            if (string.IsNullOrEmpty(creditCardNumber)) { return false; }
            //// 1. Starting with the check digit double the value of every other digit 
            //// 2. If doubling of a number results in a two digits number, add up 
            /// the digits to get a single digit number. This will results in eight single digit numbers 
            /// //// 3. Get the sum of the digits 
            int sumOfDigits = creditCardNumber.Where((e) => e >= '0' && e <= '9').Reverse().Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2)).Sum((e) => e / 10 + e % 10);
            //// If the final sum is divisible by 10, then the credit card number 
            // is valid. If it is not divisible by 10, the number is invalid. 
            return sumOfDigits % 10 == 0;
        }
        /****************************************************************************************/
        public enum CardType
        {
            Unknown = 0,
            MasterCard = 1,
            VISA = 2,
            Amex = 3,
            Discover = 4,
            DinersClub = 5,
            JCB = 6,
            enRoute = 7
        }
        /****************************************************************************************/
        // Class to hold credit card type information
        private class CardTypeInfo
        {
            public CardTypeInfo(string regEx, int length, CardType type)
            {
                RegEx = regEx;
                Length = length;
                Type = type;
            }

            public string RegEx { get; set; }
            public int Length { get; set; }
            public CardType Type { get; set; }
        }
        /****************************************************************************************/
        // Array of CardTypeInfo objects.
        // Used by GetCardType() to identify credit card types.
        private static CardTypeInfo[] _cardTypeInfo =
        {
            new CardTypeInfo("^(51|52|53|54|55)", 16, CardType.MasterCard),
            new CardTypeInfo("^(4)", 16, CardType.VISA),
            new CardTypeInfo("^(4)", 13, CardType.VISA),
            new CardTypeInfo("^(34|37)", 15, CardType.Amex),
            new CardTypeInfo("^(6011)", 16, CardType.Discover),
            new CardTypeInfo("^(300|301|302|303|304|305|36|38)",14, CardType.DinersClub),
            new CardTypeInfo("^(3)", 16, CardType.JCB),
            new CardTypeInfo("^(2131|1800)", 15, CardType.JCB),
            new CardTypeInfo("^(2014|2149)", 15, CardType.enRoute),
        };
        /****************************************************************************************/
        public static CardType GetCardType(string cardNumber)
        {
            foreach (CardTypeInfo info in _cardTypeInfo)
            {
                if (cardNumber.Length == info.Length &&
                    Regex.IsMatch(cardNumber, info.RegEx))
                    return info.Type;
            }
            return CardType.Unknown;
        }
        /****************************************************************************************/
        private void btnValidateCard_Click(object sender, EventArgs e)
        {
            //string creditCard = txtCC.Text.Trim();
            //if (String.IsNullOrWhiteSpace(creditCard))
            //{
            //    MessageBox.Show("***ERROR*** Credit Card Field is Empty!");
            //    return;
            //}
            //bool valid = Mod10Check(creditCard);
            //if (valid)
            //{
            //    CardType cardType = GetCardType(creditCard);
            //    txtCCType.Text = cardType.ObjToString();
            //}
            //else
            //    txtCCType.Text = "INVALID CARD";
        }
        /****************************************************************************************/
        private void CleanupPossibleInsurance ( DataRow dr )
        {
            try
            {
                string type = dr["type"].ObjToString().ToUpper();
                if (type.IndexOf("INSURANCE") >= 0 || type == "3RD PARTY" || type == "CLASS A")
                {
                    string names = dr["names"].ObjToString();
                    string trustOrPolicy = dr["trust_policy"].ObjToString();
                    string [] Lines = trustOrPolicy.Split('/');
                    if (Lines.Length >= 2)
                    {
                        string payer = Lines[0];
                        string policyNumber = Lines[1];
                        Lines = names.Split(',');
                        if (Lines.Length >= 2)
                        {
                            string lName = Lines[0].Trim();
                            string fName = Lines[1].Trim();
                            if (!String.IsNullOrWhiteSpace(policyNumber) && !String.IsNullOrWhiteSpace(payer) && !String.IsNullOrWhiteSpace(lName) && !String.IsNullOrWhiteSpace(fName))
                                UpdatePayerPolicies(payer, policyNumber, fName, lName, "0000-00-00", "", true );
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Remove Existing Payment
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            string depositNumber = dr["depositNumber"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( depositNumber))
            {
                if (!G1.isAdminOrSuper() && !G1.isHomeOffice())
                {
                    MessageBox.Show("Payment has Deposit Numbers!\nYou cannot delete this payment!", "Delete Payment Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string contract = dr["contractNumber"].ObjToString();
            DialogResult result = MessageBox.Show("Permanently Delete Payment?", "Delete Payment Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Cancel)
                return;
            if (result == DialogResult.Yes)
            {
                CleanupPossibleInsurance(dr);

                string record = dr["record"].ObjToString();
                G1.delete_db_table("cust_payments", "record", record);
                string cmd = "DELETE FROM `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
                G1.get_db_data(cmd);
                try
                {
                    LoadData();
                }
                catch (Exception ex)
                {
                }
                dt = (DataTable)dgv.DataSource;
                dgv.RefreshDataSource();
                dgv.Refresh();
                this.Refresh();
                //ReCalcTotal(dt);
                CalculateAmountDue();
                this.Cursor = Cursors.Arrow;
                return;
            }
            dr["status"] = "Cancelled";
            dt.Rows[row]["status"] = "Cancelled";
            funModified = true;
            dgv.RefreshDataSource();
            dgv.Refresh();
            this.Refresh();
            //ReCalcTotal(dt);
            CalculateAmountDue();
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Payment
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            DateTime now = DateTime.Now;
            dRow["mod"] = "Y";
            dRow["dateEntered"] = G1.DTtoMySQLDT(now);
            dRow["dateModified"] = G1.DTtoMySQLDT(now);
            dt.Rows.Add(dRow);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName != "payment")
                return;
            int dx = e.Bounds.Height;
            Brush brush = e.Cache.GetGradientBrush(e.Bounds, Color.Wheat, Color.FloralWhite, LinearGradientMode.Vertical);
            Rectangle r = e.Bounds;
            //Draw a 3D border 
            BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
            AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
            borderAppearance.BorderColor = Color.DarkGray;
            painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
            //Fill the inner region of the cell 
            r.Inflate(-1, -1);
            e.Cache.FillRectangle(brush, r);
            //Draw a summary value 
            r.Inflate(-2, 0);
            double discount = 0D;
            double total = calculateTotalPayments( ref discount );
            string text = G1.ReformatMoney(total);
            e.Appearance.DrawString(e.Cache, text, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /****************************************************************************************/
        private double calculateTotalPayments( ref double discount )
        {
            DataTable dt = (DataTable)dgv.DataSource;
            double price = 0D;
            double total = 0D;
            discount = 0D;
            string status = "";
            string type = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    status = dt.Rows[i]["status"].ObjToString().Trim().ToUpper();
                    if (status == "CANCELLED" || status == "REJECTED" )
                        continue;
                    type = dt.Rows[i]["type"].ObjToString().Trim().ToUpper();
                    //if (String.IsNullOrWhiteSpace(status) || status.ToUpper() == "ACCEPT")
                    //{
                    //    price = dt.Rows[i]["payment"].ObjToDouble();
                    //    total += price;
                    //}
                    //else if (String.IsNullOrWhiteSpace(status) || status.ToUpper() == "PENDING")
                    //{
                    //    price = dt.Rows[i]["payment"].ObjToDouble();
                    //    total += price;
                    //}
                    if (type.ToUpper() == "REFUND")
                    {
                        price = dt.Rows[i]["payment"].ObjToDouble();
                        price = Math.Abs(price);
                        price = price * -1D;
                        //dt.Rows[i]["payment"] = price;
                        total += price;
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(status) || status.ToUpper() == "DEPOSITED")
                    {
                        price = dt.Rows[i]["payment"].ObjToDouble();
                        total += price;
                    }
                    else if (type == "DISCOUNT")
                    {
                        price = dt.Rows[i]["payment"].ObjToDouble();
                        discount += price;
                        total += Math.Abs (price);
                    }
                    else if (status == "ACCEPT")
                    {
                        if (type == "CASH" || type == "CHECK" || type == "CREDIT CARD" || type == "CLASS A" || type == "3RD PARTY" || type == "OTHER" )
                        {
                            price = dt.Rows[i]["payment"].ObjToDouble();
                            total += price;
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            status = G1.ReformatMoney(total);
            return total;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Find Trust or Insurance as pending payment
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = "";
            if (dr != null)
            {
                string type = dr["type"].ObjToString();
                contractNumber = dr["trust_policy"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    contractNumber = workContract;
            }
            FunLookup fastForm = new FunLookup(contractNumber, workPayer );
            fastForm.SelectDone += FastForm_SelectDone;
            //fastForm.ListDone += FastForm_ListDone;
            fastForm.Show();
        }
        /****************************************************************************************/
        public static void DecodeFastLookup(string s, ref string contractNumber, ref string payer, ref string name, ref string source, ref string amount, ref string policyRecord )
        {
            contractNumber = "";
            payer = "";
            name = "";
            source = "";
            amount = "";
            policyRecord = "";
            string str = "";
            string[] Lines = s.Split(':');
            string[] nLines = null;
            for ( int i=0; i<Lines.Length; i++)
            {
                str = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.ToUpper().IndexOf ("INSURANCE") >= 0 )
                    source = str;
                else if (str.ToUpper() == "TRUST")
                    source = str;
                else if (str.ToUpper() == "POLICY")
                    source = str;
                else if (str.ToUpper() == "LIABILITY")
                    amount = Lines[i + 1];
                else if (str.ToUpper() == "CONTRACTVALUE")
                    amount = Lines[i + 1];
                else if (str.ToUpper() == "PAYER")
                    payer = Lines[i + 1];
                else if (str.Trim().IndexOf(",") >= 0)
                {
                    name = str;
                }
                else if (str.Trim().ToUpper().IndexOf("CONTRACT=") >= 0)
                {
                    nLines = str.Split('=');
                    if (nLines.Length > 1)
                        contractNumber = nLines[1];
                }
                else if (str.Trim().ToUpper().IndexOf("POLICYRECORD=") >= 0)
                {
                    nLines = str.Split('=');
                    if (nLines.Length > 1)
                        policyRecord = nLines[1];
                }
            }
        }
        /****************************************************************************************/
        public static bool CheckPayerPrepaid ( string payer, double oldPremium, double newPremium, ref DateTime lastDatePaid, ref DateTime dueDate, ref double paymentAmount, ref double numMonthPaid )
        {
            string record = "";
            string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return false;
            lastDatePaid = ddx.Rows[0]["lastDatePaid8"].ObjToDateTime();
            dueDate = ddx.Rows[0]["dueDate8"].ObjToDateTime();

            cmd = "Select * from `ipayments` where `payer` = '" + payer + "' ORDER by `payDate8` DESC LIMIT 1;";
            DataTable payDt = G1.get_db_data(cmd);
            if (payDt.Rows.Count <= 0)
                return false;
            paymentAmount = payDt.Rows[0]["paymentAmount"].ObjToDouble();
            numMonthPaid = payDt.Rows[0]["numMonthPaid"].ObjToDouble();
            return true;
        }
        /****************************************************************************************/
        public static void UpdatePayerPolicies ( string payer, string policyNumber, string policyFirstName, string policyLastName, string deceasedDate, string serviceId, bool reverse = false )
        {
            string record = "";
            string cmd = "Select * from `policies` where `payer` = '" + payer+ "' AND `policyNumber` = '" + policyNumber + "' AND `policyLastName` = '" + policyLastName + "' AND `policyFirstName` = '" + policyFirstName + "' ORDER BY `contractNumber` DESC;";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count > 0)
            {
                record = ddx.Rows[0]["record"].ObjToString();
                if (reverse)
                    G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", "0000-00-00", "ServiceId", "" });
                else
                {
                    double oldPremium = 0D;
                    double newPremium = 0D;
                    double monthlyPremium = 0D;
                    double historicPremium = 0D;
                    double monthlySecNat = 0D;
                    double monthly3rdParty = 0D;

                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
                    oldPremium = monthlyPremium - monthlySecNat;

                    G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate, "ServiceId", serviceId });

                    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
                    newPremium = monthlyPremium - monthlySecNat;

                    DateTime lastDatePaid = DateTime.Now;
                    DateTime dueDate = DateTime.Now;
                    double paymentAmount = 0D;
                    double numMonthPaid = 0D;

                    bool prepaid = CheckPayerPrepaid(payer, oldPremium, newPremium, ref lastDatePaid, ref dueDate, ref paymentAmount, ref numMonthPaid );
                }

                FunPayments.DeterminePayerDead(payer);

            //    double oldPremium = 0D;
            //    double newPremium = 0D;
            //    double monthlyPremium = 0D;
            //    double historicPremium = 0D;
            //    double monthlySecNat = 0D;
            //    double monthly3rdParty = 0D;

            //    //CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);

            //    oldPremium = monthlyPremium - monthlySecNat;

            //    CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
            //    oldPremium = monthlyPremium - monthlySecNat;
            //    oldPremium = G1.RoundValue(oldPremium);
            //    newPremium = monthlyPremium - monthlySecNat;
            //    newPremium = G1.RoundValue(newPremium);

            //    cmd = "Select * from `payers` where `payer` = '" + payer + "';";
            //    ddx = G1.get_db_data(cmd);
            //    if ( ddx.Rows.Count > 0 )
            //    {
            //        string contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
            //        string rule = "";
            //        double percent = (newPremium * 12D) * .95D;
            //        double eleven = newPremium * 11D;
            //        double twelve = newPremium * 12D;

            //        double annual = ddx.Rows[0]["annualPremium"].ObjToDouble();
            //        if (percent >= (annual - 0.02D) && percent <= (annual + 0.02D))
            //            rule = "95%";
            //        if (annual == eleven)
            //            rule = "11";
            //        if (String.IsNullOrWhiteSpace(rule))
            //            rule = "12";

            //        double annualPremium = annual;
            //        if (rule == "12")
            //            annualPremium = newPremium * 12D;
            //        else if (rule == "11")
            //            annualPremium = newPremium * 11D;
            //        else if (rule == "95%")
            //            annualPremium = newPremium * 0.95D * 12D;
            //        annualPremium = G1.RoundValue(annualPremium);

            //        record = ddx.Rows[0]["record"].ObjToString();
            //        G1.update_db_table("payers", "record", record, new string[] { "annualPremium", annualPremium.ToString(), "amtOfMonthlyPayt", newPremium.ToString() });

            //        if (newPremium == 0D)
            //        {
            //            if (reverse)
            //            {
            //                G1.update_db_table("payers", "record", record, new string[] { "deceasedDate", "0000-00-00" });

            //                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
            //                ddx = G1.get_db_data(cmd);
            //                if (ddx.Rows.Count > 0)
            //                {
            //                    record = ddx.Rows[0]["record"].ObjToString();
            //                    G1.update_db_table("icustomers", "record", record, new string[] { "deceasedDate", "0000-00-00", "serviceId", "" });
            //                }

            //                cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
            //                ddx = G1.get_db_data(cmd);
            //                if (ddx.Rows.Count > 0)
            //                {
            //                    record = ddx.Rows[0]["record"].ObjToString();
            //                    G1.update_db_table("icontracts", "record", record, new string[] { "deceasedDate", "0000-00-00", "serviceId", "" });
            //                }
            //            }
            //            else
            //            {
            //                G1.update_db_table("payers", "record", record, new string[] { "deceasedDate", deceasedDate });

            //                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
            //                ddx = G1.get_db_data(cmd);
            //                if (ddx.Rows.Count > 0)
            //                {
            //                    record = ddx.Rows[0]["record"].ObjToString();
            //                    G1.update_db_table("icustomers", "record", record, new string[] { "deceasedDate", deceasedDate, "serviceId", serviceId });
            //                }

            //                cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
            //                ddx = G1.get_db_data(cmd);
            //                if (ddx.Rows.Count > 0)
            //                {
            //                    record = ddx.Rows[0]["record"].ObjToString();
            //                    G1.update_db_table("icontracts", "record", record, new string[] { "deceasedDate", deceasedDate, "serviceId", serviceId });
            //                }
            //            }
            //        }

            //    }
            }
        }
        /****************************************************************************************/
        public static void DeterminePayerDead(string payer)
        {
            string record = "";
            string deceasedDate = "";
            string serviceId = "";

            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return;

            bool allDead = true;
            DateTime ddate = DateTime.MinValue;
            for (int i = 0; i < ddx.Rows.Count; i++)
            {
                if (ddx.Rows[i]["deceasedDate"].ObjToDateTime().Year > 100)
                {
                    if (ddx.Rows[i]["deceasedDate"].ObjToDateTime() > ddate)
                    {
                        ddate = ddx.Rows[i]["deceasedDate"].ObjToDateTime();
                        serviceId = ddx.Rows[i]["ServiceId"].ObjToString();
                    }
                }
                else
                    allDead = false;
            }

            bool reverse = true;
            if (allDead)
            {
                reverse = false;
                deceasedDate = ddate.ToString("yyyy-MM-dd");
            }

            double oldPremium = 0D;
            double newPremium = 0D;
            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            double monthly3rdParty = 0D;

            CustomerDetails.CalcMonthlyPremium(payer, ref monthlyPremium, ref historicPremium, ref monthlySecNat, ref monthly3rdParty);
            oldPremium = monthlyPremium - monthlySecNat;
            oldPremium = G1.RoundValue(oldPremium);
            newPremium = monthlyPremium - monthlySecNat;
            newPremium = G1.RoundValue(newPremium);

            DateTime payerDeceasedDate = DateTime.Now;

            cmd = "Select * from `payers` where `payer` = '" + payer + "';";
            ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count > 0)
            {
                string contractNumber = ddx.Rows[0]["contractNumber"].ObjToString();
                payerDeceasedDate = ddx.Rows[0]["deceasedDate"].ObjToDateTime();
                string rule = "";
                double percent = (newPremium * 12D) * .95D;
                double eleven = newPremium * 11D;
                double twelve = newPremium * 12D;

                double annual = ddx.Rows[0]["annualPremium"].ObjToDouble();
                if (percent >= (annual - 0.02D) && percent <= (annual + 0.02D))
                    rule = "95%";
                if (annual == eleven)
                    rule = "11";
                if (String.IsNullOrWhiteSpace(rule))
                    rule = "12";

                double annualPremium = annual;
                if (rule == "12")
                    annualPremium = newPremium * 12D;
                else if (rule == "11")
                    annualPremium = newPremium * 11D;
                else if (rule == "95%")
                    annualPremium = newPremium * 0.95D * 12D;
                annualPremium = G1.RoundValue(annualPremium);

                record = ddx.Rows[0]["record"].ObjToString();
                G1.update_db_table("payers", "record", record, new string[] { "annualPremium", annualPremium.ToString(), "amtOfMonthlyPayt", newPremium.ToString() });

                if (reverse)
                {
                    if ( payerDeceasedDate.Year > 100 )
                    {
                        string message = "Payer " + payer + " Deceased but all Policies are not!";
                        Messages.SendTheMessage(LoginForm.username, "cjenkins", "Payer Deceased Information", message);
                    }
                    //G1.update_db_table("payers", "record", record, new string[] { "deceasedDate", "0000-00-00" });

                    //cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                    //ddx = G1.get_db_data(cmd);
                    //if (ddx.Rows.Count > 0)
                    //{
                    //    record = ddx.Rows[0]["record"].ObjToString();
                    //    G1.update_db_table("icustomers", "record", record, new string[] { "deceasedDate", "0000-00-00", "serviceId", "" });
                    //}

                    //cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
                    //ddx = G1.get_db_data(cmd);
                    //if (ddx.Rows.Count > 0)
                    //{
                    //    record = ddx.Rows[0]["record"].ObjToString();
                    //    G1.update_db_table("icontracts", "record", record, new string[] { "deceasedDate", "0000-00-00", "serviceId", "" });
                    //}
                }
                else
                {
                    G1.update_db_table("payers", "record", record, new string[] { "deceasedDate", deceasedDate });

                    cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        record = ddx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("icustomers", "record", record, new string[] { "deceasedDate", deceasedDate, "serviceId", serviceId });
                    }

                    cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count > 0)
                    {
                        record = ddx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("icontracts", "record", record, new string[] { "deceasedDate", deceasedDate, "serviceId", serviceId });
                    }
                }
            }
        }
    /****************************************************************************************/
    private void FastForm_SelectDone(DataTable s)
        {
            if (s == null)
                return;
            DataRow[] dRows = s.Select("select='1'");
            if (dRows.Length <= 0)
                return;

            DataTable insDt = dRows.CopyToDataTable();

            DataTable dt = (DataTable)dgv.DataSource;

            AddMod(dt, gridMain);
            string contractNumber = "";
            string payer = "";
            string policyNumber = "";
            double liability = 0D;
            for (int i = 0; i < dRows.Length; i++)
            {
                contractNumber = dRows[i]["contractNumber"].ObjToString();
                if ( DailyHistory.isInsurance ( contractNumber))
                {
                    DataRow dRow = dt.NewRow();
                    dRow["type"] = "Insurance Direct";

                    policyNumber = dRows[i]["policyNumber"].ObjToString();
                    VerifyPolicyContract(policyNumber);
                    liability = dRows[i]["liability"].ObjToDouble();
                    payer = dRows[i]["payer"].ObjToString();

                    string policyRecord = dRows[i]["record1"].ObjToString();

                    string report = dRows[i]["report"].ObjToString();
                    dRow["payment"] = liability;
                    dRow["contractValue"] = 0D;
                    dRow["trust_policy"] = payer + "/" + policyNumber;
                    if (report.ToUpper() == "NOT THIRD PARTY")
                    {
                        if (liability == 150D || liability == 300D || liability == 450D)
                            dRow["type"] = "Class A";
                    }
                    dRow["status"] = "Accept";
                    //dRow["description"] = dRows[i]["policyLastName"].ObjToString() + ", " + dRows[i]["policyFirstName"].ObjToString() + "\n~" + policyNumber + "~" + payer + "~" + dRows[i]["policyLastName"].ObjToString() + "~" + dRows[i]["policyFirstName"].ObjToString();
                    dRow["names"] = dRows[i]["policyLastName"].ObjToString() + ", " + dRows[i]["policyFirstName"].ObjToString();
                    dRow["referenceNumber"] = policyNumber;
                    dRow["dateEntered"] = G1.DTtoMySQLDT(DateTime.Now);
                    dRow["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);

                    dt.Rows.Add(dRow);
                }
                else
                {
                    VerifyTrustContract(contractNumber);
                    double paid = 0D;
                    double amount = 0D;
                    DataRow dRow = dt.NewRow();
                    dRow["type"] = "Trust";
                    dRow["payment"] = paid;
                    dRow["contractValue"] = amount.ObjToDouble();
                    dRow["payment"] = 0D;
                    dRow["trust_policy"] = contractNumber;
                    dRow["status"] = "Pending";
                    dRow["names"] = dRows[i]["lastName"].ObjToString() + ", " + dRows[i]["firstName"].ObjToString() + " " + dRows[i]["middleName"].ObjToString();
                    dRow["referenceNumber"] = "";
//                    dRow["description"] = dRows[i]["lastName"].ObjToString() + ", " + dRows[i]["firstName"].ObjToString();
                    dRow["dateEntered"] = G1.DTtoMySQLDT(DateTime.Now);
                    dRow["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);

                    dt.Rows.Add(dRow);
                }
            }

            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
            int row = dt.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
            SetupSave();
            CalculateAmountDue();
            this.Cursor = Cursors.Arrow;
        }
        /****************************************************************************************/
        private void VerifyTrustContract ( string contractNumber)
        {
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("***WARNING*** Looking up customer (" + contractNumber + ")", "Lookup Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DateTime bDay = dt.Rows[0]["birthDate"].ObjToDateTime();
            if ( bDay != workBirthday )
            {
                MessageBox.Show("***WARNING*** Looking up customer (" + contractNumber + ") has a different Birthday!", "Lookup Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string ssn = dt.Rows[0]["ssn"].ObjToString();
            if (ssn != workSSN)
            {
                MessageBox.Show("***WARNING*** Looking up customer (" + contractNumber + ") has a different SSN!", "Lookup Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
        }
        /****************************************************************************************/
        private void VerifyPolicyContract(string policyNumber)
        {
            string cmd = "Select * from `policies` where `policyNumber` = '" + policyNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                MessageBox.Show("***WARNING*** Looking up customer (" + policyNumber + ")", "Lookup Policy Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DateTime bDay = dt.Rows[0]["birthDate"].ObjToDateTime();
            if (bDay != workBirthday)
            {
                MessageBox.Show("***WARNING*** Looking up customer (" + policyNumber + ") has a different Birthday!", "Lookup Policy Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string ssn = dt.Rows[0]["ssn"].ObjToString();
            if (ssn != workSSN)
            {
                MessageBox.Show("***WARNING*** Looking up customer (" + policyNumber + ") has a different SSN!", "Lookup Policy Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
        }
        /****************************************************************************************/
        private void FastForm_ListDone(string s)
        { // Trust or Policy Selected
            if (String.IsNullOrWhiteSpace(s))
                return;
            string source = "";
            string amount = "";
            string account = "";
            string name = "";
            string contractNumber = "";
            string payer = "";
            string policyRecord = "";

            DecodeFastLookup(s, ref contractNumber, ref payer, ref name, ref source, ref amount, ref policyRecord );

            double totalInterest = 0D;
            double BalanceDue = 0D;
            double paid = 0D;
            double contractValue = 0D;
            if (source.ToUpper() == "TRUST")
            {
                double maxTrust85 = 0D;
                double totalTrust85 = 0D;
                double trust85P = 0D;
                string cmd = "Select * from `fcontracts` where `contractNumber` = '" + contractNumber + "';";
                DataTable funDt = G1.get_db_data(cmd);
                if ( funDt.Rows.Count > 0 )
                {
                    double amtOfMonthlyPayment = funDt.Rows[0]["amtOfMonthlyPayt"].ObjToDouble();
                    int numPayments = funDt.Rows[0]["numberOfPayments"].ObjToInt32();
                    bool trustThreshold = false;
                    bool balanceThreshold = false;
                    bool isPaid = Customers.CheckForcedPayoff(contractNumber, amtOfMonthlyPayment, numPayments, ref maxTrust85, ref totalTrust85, ref contractValue, ref trustThreshold, ref balanceThreshold, trust85P);
                    double difference = maxTrust85 - totalTrust85;
                    difference = G1.RoundValue(difference);
                    paid = totalTrust85 / 0.85D;
                    paid = G1.RoundValue(paid);
                }

                BalanceDue = DailyHistory.ReCalculateDetails(contractNumber, ref totalInterest);
                contractValue = DailyHistory.GetContractValue(contractNumber);
                //paid = contractValue - BalanceDue;
            }

            DataTable dt = (DataTable)dgv.DataSource;
            AddMod(dt, gridMain);
            DataRow dRow = dt.NewRow();
            dRow["type"] = source;
            if (source.ToUpper() == "TRUST")
            {
                dRow["payment"] = paid;
                dRow["contractValue"] = amount.ObjToDouble();
                dRow["payment"] = 0D;
                dRow["trust_policy"] = contractNumber;
            }
            else
            {
                string report = getPolicyReport(policyRecord);
                double dValue = amount.ObjToDouble();
                dRow["payment"] = amount;
                dRow["contractValue"] = 0D;
                dRow["trust_policy"] = payer;
                if (report.ToUpper() == "NOT THIRD PARTY")
                {
                    if (dValue == 150D || dValue == 300D || dValue == 450D)
                        dRow["type"] = "Class A";
                }
            }
            dRow["status"] = "Pending";
            dRow["description"] = name;
            dRow["dateEntered"] = G1.DTtoMySQLDT(DateTime.Now);
            dRow["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);

            dt.Rows.Add(dRow);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
            int row = dt.Rows.Count - 1;
            gridMain.FocusedRowHandle = row;
            SetupSave();
        }
        /****************************************************************************************/
        private string getPolicyReport ( string policyRecord)
        {
            string report = "";
            if (!String.IsNullOrWhiteSpace(policyRecord))
            {
                string cmd = "Select * from `policies` where `record` = '" + policyRecord + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                    report = dt.Rows[0]["report"].ObjToString();
            }
            return report;
        }
        /****************************************************************************************/
        private bool CheckOkayToDeposit ( DataTable dt, int row )
        {
            bool okay = true;
            string depositNumber = "";
            string status = "";
            string record = dt.Rows[row]["record"].ObjToString();
            string cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + workContract + "' and `paymentRecord` = '" + record + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return false;
            for ( int i = 0; i<dx.Rows.Count; i++)
            {
                status = dx.Rows[i]["status"].ObjToString().ToUpper();
                if (status == "CANCELLED")
                    continue;
                depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( depositNumber))
                {
                    okay = false;
                    break;
                }
            }
            return okay;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox2_EditValueChanged(object sender, EventArgs e)
        {
            if (!workNoEdit)
            {
                btnSavePayments.Show();
                btnSavePayments.Refresh();
            }

            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                ComboBoxEdit combo = (ComboBoxEdit)sender;
                string what = combo.Text;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                string oldStatus = dr["status"].ObjToString();
                string depositNumber = dr["depositNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(depositNumber))
                {
                    if (what.ToUpper() != "DEPOSITED")
                    {
                        if (!G1.isAdminOrSuper() && !G1.isHomeOffice())
                        {
                            MessageBox.Show("Payment has Deposit Numbers!\nYou cannot change the Status once Deposit Numbers have been assigned!", "Status Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            dr["status"] = oldStatus;
                            dt.Rows[row]["status"] = oldStatus;
                            dgv.RefreshDataSource();
                            gridMain.RefreshEditor(true);
                            return;
                        }
                    }
                }
                dr["status"] = what;
                dr["mod"] = "Y";
            }
            catch ( Exception ex)
            {
            }

            //try
            //{
            //    ComboBoxEdit combo = (ComboBoxEdit)sender;
            //    string what = combo.Text;
            //    DataRow dr = gridMain.GetFocusedDataRow();
            //    int row = gridMain.FocusedRowHandle;
            //    row = gridMain.GetDataSourceRowIndex(row);
            //    dr["status"] = what;
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    dt.Rows[row]["status"] = what;
            //    dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
            //    string type = dt.Rows[row]["type"].ObjToString().ToUpper();
            //    if (what.ToUpper() == "DEPOSITED")
            //    {
            //        if ( type != "TRUST" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" )
            //            dt.Rows[row]["grossAmountReceived"] = dt.Rows[row]["payment"].ObjToDouble();
            //        dt.Rows[row]["mod"] = "Y";

            //        type = dr["type"].ObjToString().ToUpper();
            //        string bankAccount = GetDepositBankAccount(type);
            //        if (!String.IsNullOrWhiteSpace(bankAccount))
            //        {
            //            dr["bankAccount"] = bankAccount;
            //            dt.Rows[row]["bankAccount"] = bankAccount;
            //            gridMain.RefreshEditor(true);
            //            dgv.RefreshDataSource();
            //            dgv.Refresh();
            //        }
            //        else
            //        {
            //            gridMain.RefreshEditor(true);
            //            dgv.RefreshDataSource();
            //            dgv.Refresh();
            //        }
            //    }
            //    dgv.RefreshDataSource();
            //}
            //catch (Exception ex)
            //{

            //}
            //CalculateAmountDue();
            //funModified = true;
            //gridMain.RefreshData();
            //btnSavePayments.Show();
            //ResetFocus();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBoxEdit combo = (ComboBoxEdit)sender;
                string type = combo.Text;
                DataRow dr = gridMain.GetFocusedDataRow();
                string what = dr["status"].ObjToString().ToUpper();
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                dr["type"] = type;
                dr["mod"] = "Y";
                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows[row]["type"] = type;
                dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
                dt.Rows[row]["mod"] = "Y";

                string bankAccount = GetDepositBankAccount(type);
                if (!String.IsNullOrWhiteSpace(bankAccount))
                {
                    dr["bankAccount"] = bankAccount;
                    dt.Rows[row]["bankAccount"] = bankAccount;
                }
            }
            catch (Exception ex)
            {

            }
            CalculateAmountDue();
            SetupSave();
            gridMain.RefreshData();
            ResetFocus();
        }
        /****************************************************************************************/
        private void ResetFocus ()
        {
            //int rowHandle = gridMain.FocusedRowHandle;
            rtb.Focus();
            gridMain.Focus();
            //gridMain.FocusedRowHandle = rowHandle;
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void txtAsOf_Enter(object sender, EventArgs e)
        {
            string date = txtAsOf.Text;
            if (G1.validate_date(date))
            {
                DateTime ddate = date.ObjToDateTime();
                txtAsOf.Text = ddate.ToString("MM/dd/yyyy");
            }
            else
            {
                txtAsOf.Text = "";
            }
        }
        /****************************************************************************************/
        private void txtAsOf_Leave(object sender, EventArgs e)
        {
            string date = txtAsOf.Text;
            if (String.IsNullOrWhiteSpace(date))
                return;
            if (G1.validate_date(date))
            {
                DateTime ddate = txtAsOf.Text.ObjToDateTime();
                if (ddate.Year < 1800)
                {
                    //MessageBox.Show("Payment has Deposit Numbers!\nYou cannot change the Status once Deposit Numbers have been assigned!", "Status Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    MessageBox.Show("***ERROR*** Date Entered Year is less than 1800!", "Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                if (ddate.Year > 100)
                {
                    txtAsOf.Text = ddate.ToString("MM/dd/yyyy");
                    CalculateAmountDue();
                }
            }
            else
            {
                MessageBox.Show("***ERROR*** Invalid Date!", "Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void txtAsOf_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtAsOf_Leave(sender, e);
            else if (e.KeyCode == Keys.Tab)
                txtAsOf_Leave(sender, e);
        }
        /****************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            AddMod(dt, gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string type = dr["type"].ObjToString();
            if ( LoginForm.classification.ToUpper() == "FIELD" && type.ToUpper() == "TRUST")
            {
                DialogResult result = MessageBox.Show("***INFORMATION***\nYou cannot double-click on a Trust Payment!", "Trust Payments Restricted Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (LoginForm.classification.ToUpper() == "FIELD" && type.ToUpper() == "INSURANCE UNITY")
            {
                DialogResult result = MessageBox.Show("***INFORMATION***\nYou cannot double-click on a Insurance Unity Payment!", "Trust Payments Restricted Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();

            string record = dr["record"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( record))
            {
                //DialogResult result = MessageBox.Show("***INFORMATION***\nYou MUST save this payment before entering any DETAIL about it!", "Payments Modified Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult result = MessageBox.Show("***QUESTION***\nNew Payment Pending!\nDo you want to save before entering?", "New Payment Pending Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if ( result == DialogResult.No)
                    return;
                btnSavePayments_Click(null, null);
                LoadData();
                dt = (DataTable)dgv.DataSource;
                dr = dt.Rows[row];
                record = dr["record"].ObjToString();
            }
            string str = dr["payment"].ObjToString();
            str = str.Replace("$", "");
            str = str.Replace(",", "");
            double totalDue = str.ObjToDouble();
            this.Cursor = Cursors.WaitCursor;
            if ( workNoEdit )
            {
                FunPaymentDetails funDetails = new FunPaymentDetails(workContract, record, totalDue, dr, workNoEdit);
                funDetails.TopMost = true;
                funDetails.Show();
                this.Cursor = Cursors.Default;
                return;
            }
            using (FunPaymentDetails funDetails = new FunPaymentDetails(workContract, record, totalDue, dr, workNoEdit))
            {
                Form form = G1.IsFormOpen("TrustDeceased");

                if ( form == null )
                    funDetails.TopMost = true;
                DialogResult result = funDetails.ShowDialog();

                if (result == DialogResult.Yes)
                {
                    //dr["payment"] = funDetails.amountReceived;
                    dr["grossAmountReceived"] = funDetails.amountReceived;
                    dr["amountReceived"] = funDetails.amountReceived;
                    dr["amountFiled"] = funDetails.amountFiled;
                    dr["amountDiscount"] = funDetails.amountDiscount;
                    dr["amountGrowth"] = funDetails.amountGrowth;
                    dr["depositNumber"] = funDetails.depositNumbers;
                    dr["mod"] = "Y";
                    if (funDetails.amountDebit != 0D)
                    {
                        if (funDetails.amountDebit > 0D)
                            dr["payment"] = dr["payment"].ObjToDouble() - funDetails.amountDebit;
                        else if (funDetails.amountDebit < 0D)
                            dr["payment"] = dr["payment"].ObjToDouble() + funDetails.amountDebit;
                    }
                    string oldStatus = dr["status"].ObjToString();
                    string newStatus = funDetails.paymentStatus;
                    if (newStatus == "Received")
                        newStatus = "Deposited";
                    dr["status"] = newStatus;
                    AddCustomerPayments();
                    CalculateAmountDue();
                    SetupSave();
                    gridMain.RefreshData();
                    ResetFocus();
                }
            }
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
        private void AddCustomerPayments()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            string pendingComment = rtb.Text;
            string amountFiled = "";
            string amountReceived = "";
            string amountDiscount = "";
            string amountGrowth = "";
            string grossAmountReceived = "";
            totalFiled = 0D;
            totalReceived = 0D;
            totalDiscount = 0D;
            totalAmountDiscount = 0D;
            totalAmountGrowth = 0D;
            double totalGross = 0D;
            double totalPayments = 0D;

            double trustAmountFiled = 0D;
            double insAmountFiled = 0D;
            double trustAmountReceived = 0D;
            double insAmountReceived = 0D;

            string str = "";
            string type = "";
            string cash = "";
            string deposit = "";
            string creditCard = "";
            string ccDepNumber = "";
            double dValue = 0D;
            double balanceDue = 0D;
            double discount = 0D;
            double classa = 0D;
            string approvedBy = "";
            string status = "";
            DateTime dateEntered = DateTime.Now;
            DateTime dateModified = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    status = dt.Rows[i]["status"].ObjToString().ToUpper();
                    if (status == "CANCELLED")
                        continue;

                    amountFiled = dt.Rows[i]["amountFiled"].ObjToString();
                    amountReceived = dt.Rows[i]["amountReceived"].ObjToString();
                    amountDiscount = dt.Rows[i]["amountDiscount"].ObjToString();
                    amountGrowth = dt.Rows[i]["amountGrowth"].ObjToString();
                    grossAmountReceived = dt.Rows[i]["grossAmountReceived"].ObjToString();
                    totalFiled += amountFiled.ObjToDouble();
                    totalReceived += amountReceived.ObjToDouble();
                    totalAmountDiscount += amountDiscount.ObjToDouble();
                    totalAmountGrowth += amountGrowth.ObjToDouble();
                    totalGross += grossAmountReceived.ObjToDouble();

                    type = dt.Rows[i]["type"].ObjToString().ToUpper();

                    if (status == "DEPOSITED" || type == "DISCOUNT" )
                        totalPayments += dt.Rows[i]["payment"].ObjToDouble();
                    else
                    {
                        if ( status == "ACCEPT")
                        {
                            if ( type == "CASH" || type == "CHECK" || type == "CREDIT CARD" )
                                totalPayments += dt.Rows[i]["payment"].ObjToDouble();
                        }
                    }

                    if (type == "CASH")
                    {
                        dValue = dt.Rows[i]["payment"].ObjToDouble();
                        str = G1.ReformatMoney(dValue);
                        cash += "CA - " + str + " ";
                    }
                    else if (type == "CREDIT CARD")
                    {
                        dValue = dt.Rows[i]["payment"].ObjToDouble();
                        str = G1.ReformatMoney(dValue);
                        creditCard += "CC - " + str + " ";
                    }
                    else if (type == "CLASSA")
                    {
                        classa += dt.Rows[i]["payment"].ObjToDouble();
                    }
                    str = dt.Rows[i]["depositNumber"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        if (type == "CASH")
                            deposit += str + " ";
                        else if (type == "CREDIT CARD")
                            ccDepNumber += str + " ";
                    }
                    if (type == "DISCOUNT")
                    {
                        discount += dt.Rows[i]["payment"].ObjToDouble();
                        str = dt.Rows[i]["approvedBy"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                            approvedBy += str + " ";
                    }
                    if (type == "TRUST")
                    {
                        if ( status == "PENDING")
                            trustAmountFiled += amountFiled.ObjToDouble();
                        else if ( status == "DEPOSITED")
                            trustAmountReceived += amountReceived.ObjToDouble();
                    }
                    else if (type.ToUpper().IndexOf ("INSURANCE") >= 0 )
                    {
                        if ( status == "PENDING")
                            insAmountFiled += amountFiled.ObjToDouble();
                        else if ( status == "DEPOSITED")
                            insAmountReceived += amountReceived.ObjToDouble();
                    }
                }
                catch (Exception ex)
                {
                }
            }
            totalModified = true;
            if (!String.IsNullOrWhiteSpace(custExtendedRecord))
            {
                string cmd = "Select * from `fcust_extended` where `record` = '" + custExtendedRecord + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                {
                    double dbr = ProcessDBR();
                    string contractType = cmbFunType.Text.Trim();
                    double customerPrice = ddx.Rows[0]["custPrice"].ObjToDouble();
                    //balanceDue = customerPrice - totalReceived - discount;
                    balanceDue = customerPrice - totalPayments;
                    balanceDue = G1.RoundValue(balanceDue);
                    totalModified = true;
                    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "pendingComment", pendingComment, "amountFiled", totalFiled.ToString(), "amountReceived", totalReceived.ToString(), "cash", cash, "depositNumber", deposit, "balanceDue", balanceDue.ToString(), "additionalDiscount", discount.ToString(), "approvedBy", approvedBy, "creditCard", creditCard, "ccDepNumber", ccDepNumber, "grossAmountReceived", totalGross.ObjToString(), "classa", classa.ToString(), "amountDiscount", totalAmountDiscount.ObjToString(), "amountGrowth", totalAmountGrowth.ObjToString() });
                    G1.update_db_table("fcust_extended", "record", custExtendedRecord, new string[] { "trustAmountFiled", trustAmountFiled.ObjToString(), "trustAmountReceived", trustAmountReceived.ObjToString(), "insAmountFiled", insAmountFiled.ObjToString(), "insAmountReceived", insAmountReceived.ObjToString(), "contractType", contractType, "dbr", dbr.ToString() });
                    double totalDue = GetTotalDue();
                    if (totalReceived != totalDue)
                    {
                        SetupSave();
                    }
                }
            }
        }
        /***************************************************************************************/
        public delegate void d_void_PaymentClosing( string record, double amountFiled, double amountReceived, double amountDiscount, double amountGrowth );
        public event d_void_PaymentClosing paymentClosing;
        protected void OnPaymentClosing()
        {
            if ( totalModified )
                paymentClosing?.Invoke( custExtendedRecord, totalFiled, totalReceived, totalAmountDiscount, totalAmountGrowth );
        }
        /****************************************************************************************/
        private void FunPayments_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!workNoEdit)
            {
                bool okay = validateClosing();
                if (!okay)
                {
                    e.Cancel = true;
                    return;
                }
                OnPaymentClosing();
            }
        }
        /****************************************************************************************/
        private bool validateClosing()
        {
            bool okay = true;
            string type = "";
            string depositNumber = "";
            string status = "";
            string pendingComment = rtb.Text.Trim();
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return true;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                status = dt.Rows[i]["status"].ObjToString();
                if ( status.Trim().ToUpper() == "PENDING" && String.IsNullOrWhiteSpace ( pendingComment))
                {
                    MessageBox.Show("***ERROR*** Pending Payments Require Explantion in Box Above!", "Pending Verification Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    okay = false;
                    break;
                }
                //depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                //if (type == "CASH" || type == "CHECK-LOCAL" || type == "CHECK-REMOTE" || type == "CREDIT CARD")
                //{
                //    if (String.IsNullOrWhiteSpace(depositNumber))
                //    {
                //        MessageBox.Show("***ERROR*** You must provide a Deposit Number for all Cash, Check, or Credit Card Desposits!", "Deposit Verification Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        okay = false;
                //        break;
                //    }
                //}
            }
            return okay;
        }
        /****************************************************************************************/
        private string GetDepositBankAccount(string paymentType )
        {
            string location = EditCustomer.activeFuneralHomeName;
            string bankAccount = "";
            if (!String.IsNullOrWhiteSpace(location))
            {
                string cmd = "Select * from `funeralhomes` where `locationCode` = '" + location + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    if (String.IsNullOrWhiteSpace(paymentType))
                        paymentType = "CASH";
                    if (paymentType.ToUpper() == "CASH")
                        bankAccount = SetupBanks(dt, "cashLocal");
                    else if ( paymentType.ToUpper() == "CHECK")
                        bankAccount = SetupBanks(dt, "checkLocal");
                    else if (paymentType.ToUpper().IndexOf ("INSURANCE") >= 0 )
                        bankAccount = SetupMainBanks("insUnity");
                    //else if (paymentType.ToUpper() == "CHECK-LOCAL")
                    //    bankAccount = SetupBanks(dt, "checkLocal");
                    //else if (paymentType.ToUpper() == "INS - UNITY")
                    //    bankAccount = SetupMainBanks("insUnity");
                    //else if (paymentType.ToUpper() == "CHECK-REMOTE")
                    //    bankAccount = SetupMainBanks("checkRemote");
                    else if (paymentType.ToUpper() == "CREDIT CARD")
                        bankAccount = SetupMainBanks("funeral");
                    else
                    {
                        gridMain.Columns["localDescription"].ColumnEdit = null;
                        int focusedRow = gridMain.FocusedRowHandle;

                        int row = gridMain.GetDataSourceRowIndex(focusedRow);
                        gridMain.SelectRow(focusedRow);

                        DataRow dr = gridMain.GetFocusedDataRow();
                        dr["bankAccount"] = "";
                        dr["localDescription"] = "";

                        dt = (DataTable)dgv.DataSource;
                        dt.Rows[row]["bankAccount"] = "";
                        dt.Rows[row]["localDescription"] = "";
                        gridMain.RefreshData();
                        gridMain.RefreshEditor(true);
                    }
                }
            }
            return bankAccount;
        }
        /***********************************************************************************************/
        private DataTable bankDt = null;
        private string SetupMainBanks ( string what )
        {
            string bankAccount = "";
            string bankList = "";
            string localDescription = "";
            if (bankDt == null)
            {
                bankDt = G1.get_db_data("Select * from `bank_accounts` ORDER BY `order`;");
                for ( int i=0; i<bankDt.Rows.Count; i++)
                {
                    localDescription = bankDt.Rows[i]["localDescription"].ObjToString();
                    if (String.IsNullOrWhiteSpace(localDescription))
                        bankDt.Rows[i]["localDescription"] = bankDt.Rows[i]["account_title"].ObjToString();
                }
            }
            if (bankDt.Rows.Count <= 0)
                return bankAccount;
            DataRow[] dRows = bankDt.Select(what + "='1'");
            for ( int i=0; i<dRows.Length; i++)
            {
                bankList += dRows[i]["localDescription"].ObjToString() + "~";
                if (String.IsNullOrWhiteSpace(bankAccount))
                    bankAccount = dRows[i]["account_no"].ObjToString();
            }

            if ( !String.IsNullOrWhiteSpace ( bankList ))
                FixBank(bankList);

            return bankAccount;
        }
        /***********************************************************************************************/
        private string SetupBanks(DataTable dt, string what)
        {
            string bankAccount = "";
            string bankList = "";
            string cmd = "";
            string[] account = null;
            string str = dt.Rows[0][what].ObjToString();
            str = str.TrimEnd('~');
            if (!String.IsNullOrWhiteSpace(str))
            {
                DataTable dx = null;
                string record = "";
                string general_ledger_no = "";
                string account_no = "";
                string[] Lines = str.Split('~');
                for (int i = 0; i < Lines.Length; i++)
                {
                    record = Lines[i].Trim();
                    account = record.Split('/');
                    if (account.Length < 2)
                        continue;
                    general_ledger_no = account[0].Trim();
                    account_no = account[1].Trim();
                    //cmd = "Select * from `bank_accounts` where `record` = '" + record + "';";
                    cmd = "Select * from `bank_accounts` where `general_ledger_no` = '" + general_ledger_no + "' and `account_no` = '" + account_no + "';";
                    //cmd = "Select * from `bank_accounts` where `account_no` = '" + account_no + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        bankList += dx.Rows[0]["localDescription"].ObjToString() + "~";
                        if (String.IsNullOrWhiteSpace(bankAccount))
                            bankAccount = dx.Rows[0]["account_no"].ObjToString();
                    }
                }
                FixBank(bankList);
            }
            return bankAccount;
        }
        /***********************************************************************************************/
        RepositoryItemComboBox ciLookup2 = null;
        private void FixBank(string bankList, string paymentType = "")
        {
            bankList = bankList.TrimEnd('~');
            string[] Lines = bankList.Split('~');

            if (ciLookup2 == null)
            {
                ciLookup2 = new RepositoryItemComboBox();
                ciLookup2.SelectedIndexChanged += repositoryItemComboBox3_SelectedIndexChanged;
            }

            //GridColumn currCol = gridMain.FocusedColumn;
            //currentColumn = currCol.FieldName;
            //currentColumn = "paymentType";
            int focusedRow = gridMain.FocusedRowHandle;

            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            gridMain.SelectRow(focusedRow);

            DataTable dt = (DataTable)dgv.DataSource;
            string saveLocation = dt.Rows[row]["localDescription"].ObjToString();
            dt.Rows[row]["initialized"] = "1";

            ciLookup2.Items.Clear();
            string localDescription = "";
            string saveDescription = "";
            bool first = true;
            for (int i = 0; i < Lines.Length; i++)
            {
                localDescription = Lines[i].Trim();
                if (!String.IsNullOrWhiteSpace(localDescription))
                {
                    ciLookup2.Items.Add(localDescription);
                    if (first)
                    {
                        dt.Rows[row]["localDescription"] = localDescription;
                        saveDescription = localDescription;
                    }
                    first = false;
                }
            }
            gridMain.Columns["localDescription"].ColumnEdit = ciLookup2;

            //gridMain.RefreshData();

            if (!String.IsNullOrWhiteSpace(paymentType))
            {
                string location = EditCustomer.activeFuneralHomeName;
                if (!String.IsNullOrWhiteSpace(location))
                {
                    string bankAccount = GetDepositBankAccount ( paymentType );
                    dt.Rows[row]["localDescription"] = saveLocation;
                    if (!String.IsNullOrWhiteSpace(saveDescription))
                    {
                        string cmd = "Select * from `bank_accounts` where `localDescription` = '" + saveDescription + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            cmd = "Select * from `bank_accounts` where `account_title` = '" + saveDescription + "';";
                            dx = G1.get_db_data(cmd);
                        }
                        if (dx.Rows.Count > 0)
                        {
                            bankAccount = dx.Rows[0]["account_no"].ObjToString();
                            dt.Rows[row]["bankAccount"] = bankAccount;
                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                        }
                    }
                }
            }
            //gridMain.SelectRow(focusedRow);
            //gridMain.FocusedRowHandle = row;
            //gridMain.UnselectRow(focusedRow);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            if (dx == null)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            ComboBoxEdit edit = (ComboBoxEdit)sender;
            string str = edit.Text;

            string cmd = "Select * from `bank_accounts` where `localDescription` = '" + str + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                cmd = "Select * from `bank_accounts` where `account_title` = '" + str + "';";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count > 0)
            {
                string bankAccount = dt.Rows[0]["account_no"].ObjToString();
                dx.Rows[row]["bankAccount"] = bankAccount;
                dx.Rows[row]["mod"] = "Y";
                dr["mod"] = "Y";
                //gridMain.RefreshData();
            }
            SetupSave();
            ResetFocus();
        }
        /****************************************************************************************/
        private void SetupSave ()
        {
            if (workNoEdit)
                return;
            funModified = true;
            btnSavePayments.Show();
            btnSavePayments.Refresh();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox3_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            //int focusedRow = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(focusedRow);
            //string initialized = dt.Rows[row]["initialized"].ObjToString();
            //if (initialized != "1")
            //{
            //    try
            //    {
            //        DataRow dr = gridMain.GetFocusedDataRow();
            //        string type = dr["type"].ObjToString().ToUpper();
            //        string what = dr["status"].ObjToString().ToUpper();
            //        row = gridMain.GetDataSourceRowIndex(row);
            //        dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
            //        if (what.ToUpper() == "DEPOSITED")
            //        {
            //            string bankAccount = GetDepositBankAccount(type);
            //            if (!String.IsNullOrWhiteSpace(bankAccount))
            //            {
            //                dr["bankAccount"] = bankAccount;
            //                dt.Rows[row]["bankAccount"] = bankAccount;
            //                gridMain.RefreshEditor(true);
            //                dgv.RefreshDataSource();
            //                dgv.Refresh();
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ComboBoxEdit edit = (ComboBoxEdit)sender;
            //string str = edit.Text;
            //DataTable dx = (DataTable)dgv.DataSource;
            //if (dx == null)
            //    return;
            //int focusedRow = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(focusedRow);
            //dx.Rows[row]["type"] = str;
            //gridMain.SelectRow(focusedRow);
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //repositoryItemComboBox1.ShowDropDown = DevExpress.XtraEditors.Controls.ShowDropDown.SingleClick;
            //repositoryItemComboBox1_SelectedIndexChanged(sender, null);
            //int focusedRow = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(focusedRow);
            //dgv.Select();
            //gridMain.SelectRow(focusedRow);
            //gridMain.FocusedRowHandle = row;
            //gridMain.SelectRow(focusedRow);
            //gridMain.FocusedRowHandle = row;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_Popup(object sender, EventArgs e)
        {
            //int focusedRow = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(focusedRow);
            ////dgv.Select();
            //gridMain.SelectRow(focusedRow);
            //gridMain.OptionsSelection.EnableAppearanceFocusedCell = true;
            //// Draw a dotted focus rectangle around the entire row.
            //gridMain.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.CellFocus;
            ////gridMain.FocusedRowHandle = row;
            ////gridMain.SelectRow(focusedRow);
            ////gridMain.FocusedRowHandle = row;
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

            string initialized = dt.Rows[row]["initialized"].ObjToString();

            string saveDescription = dr["localDescription"].ObjToString();
            string saveBank = dr["bankAccount"].ObjToString();

            try
            {
                string type = dr["type"].ObjToString().ToUpper();
                string what = dr["status"].ObjToString().ToUpper();
                row = gridMain.GetDataSourceRowIndex(row);
                //if ( !loading )
                //    dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
                if (what.ToUpper() == "DEPOSITED")
                {
                    string bankAccount = GetDepositBankAccount(type);
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        dr["bankAccount"] = bankAccount;
                        dt.Rows[row]["bankAccount"] = bankAccount;
                        gridMain.RefreshEditor(true);
                        dgv.RefreshDataSource();
                        dgv.Refresh();
                    }
                }
                else
                {
                    saveBank = "";
                    saveDescription = "";
                    dr["bankAccount"] = "";
                    dr["localDescription"] = "";
                    dt.Rows[row]["bankAccount"] = "";
                    dt.Rows[row]["localDescription"] = "";
                }
                if (!String.IsNullOrWhiteSpace(saveDescription))
                {
                    dr["bankAccount"] = saveBank;
                    dr["localDescription"] = saveDescription;
                    dt.Rows[row]["bankAccount"] = saveBank;
                    dt.Rows[row]["localDescription"] = saveDescription;
                }
            }
            catch (Exception ex)
            {
            }
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

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

            font = new Font("Ariel", 10, FontStyle.Regular);
            //            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(5, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void cmbFunType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            SetupSave();
        }
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "STATUS")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                string status = dr["status"].ObjToString().ToUpper();
                if ( status == "CANCELLED")
                {
                    string record = dr["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            for (int i = 0; i < dx.Rows.Count; i++)
                            {
                                record = dx.Rows[0]["record"].ObjToString();
                                G1.update_db_table("cust_payment_details", "record", record, new string[] { "status", "Cancelled" });

                                btnSavePayments_Click(null, null);
                                btnSavePayments.Hide();
                                btnSavePayments.Refresh();
                                justSaved = true;
                            }
                        }
                    }
                }
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DATEENTERED")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["dateEntered"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "TRUST_POLICY")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "PAYMENT")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();

                string record = dr["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    string cmd = "Select * from `cust_payment_details` WHERE `paymentRecord` = '" + record + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        double payment = dr["payment"].ObjToDouble();
                        payment = oldWhat.ObjToDouble();
                        record = dx.Rows[0]["record"].ObjToString();
                        G1.update_db_table("cust_payment_details", "record", record, new string[] {"paid", payment.ToString() });

                        btnSavePayments_Click(null, null);
                        btnSavePayments.Hide();
                        btnSavePayments.Refresh();
                        funModified = false;
                        justSaved = true;
                    }
                }
            }
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private void repositoryItemComboBox2_Validating(object sender, CancelEventArgs e)
        {
            //e.Cancel = true;
            //if (1 == 1)
            //    return;
            try
            {
                ComboBoxEdit combo = (ComboBoxEdit)sender;
                string what = combo.Text;
                DataRow dr = gridMain.GetFocusedDataRow();
                int row = gridMain.FocusedRowHandle;
                row = gridMain.GetDataSourceRowIndex(row);
                dr["status"] = what;
                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows[row]["status"] = what;
                dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
                string type = dt.Rows[row]["type"].ObjToString().ToUpper();
                if (what.ToUpper() == "DEPOSITED")
                {
                    if (type.ToUpper() != "CLASS A")
                    {
                        if (!CheckOkayToDeposit(dt, row))
                        {
                            MessageBox.Show("***ERROR*** Not all Payment Details have Deposit Numbers!", "Payment Deposit Number Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            e.Cancel = true;
                            dt.Rows[row]["status"] = oldWhat;
                            dr["status"] = oldWhat;
                            //e.ErrorText = "Only numeric values are accepted.";
                            return;
                        }
                    }
                    if (type != "TRUST" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY")
                        dt.Rows[row]["grossAmountReceived"] = dt.Rows[row]["payment"].ObjToDouble();
                    dt.Rows[row]["mod"] = "Y";

                    type = dr["type"].ObjToString().ToUpper();
                    string bankAccount = GetDepositBankAccount(type);
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        dr["bankAccount"] = bankAccount;
                        dt.Rows[row]["bankAccount"] = bankAccount;
                        gridMain.RefreshEditor(true);
                        dgv.RefreshDataSource();
                        dgv.Refresh();
                    }
                    else
                    {
                        gridMain.RefreshEditor(true);
                        dgv.RefreshDataSource();
                        dgv.Refresh();
                    }
                }
                dgv.RefreshDataSource();
            }
            catch (Exception ex)
            {

            }
            CalculateAmountDue();
            gridMain.RefreshData();
            SetupSave();
            ResetFocus();
        }
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "CHECKLIST")
            {
                string type = view.GetRowCellValue(e.RowHandle, "type").ObjToString().ToUpper();
                if (type != "INSURANCE" && type != "POLICY" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" && type != "3RD PARTY")
                {
                    e.RepositoryItem = null;
                    return;
                }
                string status = view.GetRowCellValue(e.RowHandle, "status").ObjToString();
                if (status.ToUpper() == "FILED")
                    e.RepositoryItem = this.repositoryItemButtonEdit2;
                else if ( status.ToUpper() == "DEPOSITED")
                    e.RepositoryItem = this.repositoryItemButtonEdit1;
                else
                    e.RepositoryItem = this.repositoryItemButtonEdit2;
            }
        }
        /****************************************************************************************/
        private void repositoryItemButtonEdit1_Click(object sender, EventArgs e)
        {
            if ( String.IsNullOrWhiteSpace ( workServiceId ))
            {
                MessageBox.Show("***ERROR***\nThere is no Service ID!\nYou cannot edit the Insurance Claims Form", "Claims Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            GridView view = sender as GridView;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                string status = dt.Rows[row]["status"].ObjToString().ToUpper();
                if (status == "CANCELLED" || status == "REJECTED")
                {
                    MessageBox.Show("***ERROR***\nStatus is either Cancelled or Rejected!\nYou cannot edit the Insurance Claims Form", "Claims Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                string type = dt.Rows[row]["type"].ObjToString().ToUpper();
                if (type != "INSURANCE" && type != "POLICY" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" && type != "3RD PARTY")
                {
                    MessageBox.Show("***ERROR***\nPayment Type does not appear to be an Insurance!\nYou cannot edit the Insurance Claims Form", "Claims Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                string policyNumber = dt.Rows[row]["trust_policy"].ObjToString();
                if ( policyNumber.IndexOf ( "/" ) > 0 )
                {
                    string[] Lines = policyNumber.Split('/');
                    if (Lines.Length >= 2)
                        policyNumber = Lines[1].Trim();
                }
                DateTime dateReceived = DateTime.MinValue;
                if ( status == "DEPOSITED")
                    dateReceived = dt.Rows[row]["dateModified"].ObjToDateTime();
                double payment = dt.Rows[row]["grossAmountReceived"].ObjToDouble();
                InsuranceChecklist newInsForm = new InsuranceChecklist(workContract, workServiceId, policyNumber, dateReceived, payment);
                newInsForm.TopMost = true;
                newInsForm.ShowDialog();
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private string oldPolicy = "";
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string field = view.FocusedColumn.FieldName.ToUpper();
            if (field.ToUpper() == "TRUST_POLICY")
            {
                //string serialNumber = dr["SerialNumber"].ObjToString();
                DataTable dt = (DataTable)dgv.DataSource;
                oldPolicy = dt.Rows[row]["trust_policy"].ObjToString();
            }
        }
        /****************************************************************************************/
    }
}