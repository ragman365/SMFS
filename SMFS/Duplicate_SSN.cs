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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.Drawing.Text;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System.Configuration;
using DevExpress.RichEdit.Export;
using DevExpress.XtraGrid.Columns;
using DevExpress.CodeParser;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Duplicate_SSN : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private string workSSN = "";
        private string workContract = "";
        public Duplicate_SSN( string contract, string ssn )
        {
            InitializeComponent();
            workSSN = ssn;
            workContract = contract;
        }
        /****************************************************************************************/
        private void Duplicate_SSN_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            string contractNumber = "";
            DataRow[] dRows = null;
            string cmd = "Select * from `fcustomers` c JOIN `fcontracts` x ON c.`contractNumber` = x.`contractNumber` where `ssn` = '" + workSSN + "';";
            DataTable dx = G1.get_db_data(cmd);

            cmd = "Select * from `customers` c JOIN `contracts` x ON c.`contractNumber` = x.`contractNumber` where `ssn` = '" + workSSN + "';";
            DataTable ddx = G1.get_db_data(cmd);
            for ( int i=0; i<ddx.Rows.Count; i++)
            {
                contractNumber = ddx.Rows[i]["contractNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(contractNumber))
                    continue;
                dRows = dx.Select("contractNumber='" + contractNumber + "'");
                if (dRows.Length <= 0)
                    G1.copy_dt_row(ddx, i, dx, dx.Rows.Count);
            }
            if ( dx.Rows.Count <= 0 )
                return;

            //DataView tempview = dx.DefaultView;
            //tempview.Sort = "issueDate8 desc";
            //dx = tempview.ToTable();


            string contract = "";
            string service = "";
            ddx = null;
            DataTable cDt = null;
            DataRow dR = null;
            string str = "";
            string type = "";
            string price = "";
            string name = "";
            string cName = "";
            string issueDate = "";
            double dValue = 0D;
            string lastCasket = "";
            string lastVault = "";
            int rowNum = 0;
            string[] Lines = null;
            DateTime contractDate = DateTime.Now;
            DateTime date = DateTime.Now;
            DataTable dt = new DataTable();
            DataTable custDt = null;

            string casketCode = "";
            double casketPrice = 0D;
            string vaultCode = "";
            double vaultPrice = 0D;

            string casketDetail = "";
            string vaultDetail = "";

            string data = "";

            bool oldContractTable = false;
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                oldContractTable = false;
                casketCode = "";
                casketPrice = 0D;
                vaultCode = "";
                vaultPrice = 0D;
                casketDetail = "";
                vaultDetail = "";

                contract = dx.Rows[i]["contractNumber"].ObjToString();
                contractDate = dx.Rows[i]["contractDate"].ObjToDateTime();
                cmd = "Select * from `fcontracts` where `contractNumber` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                if ( ddx.Rows.Count <= 0 )
                {
                    oldContractTable = true;
                    cmd = "Select * from `contracts` where `contractNumber` = '" + contract + "';";
                    ddx = G1.get_db_data(cmd);
                }
                cmd = "Select * from `fcustomers` where `contractNumber` = '" + contract + "';";
                if ( oldContractTable )
                    cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                custDt = G1.get_db_data(cmd);
                if ( custDt.Rows.Count > 0 )
                {
                    casketCode = custDt.Rows[0]["extraItemAmtMI1"].ObjToString();
                    if (casketCode.ToUpper().IndexOf("-BAD") >= 0)
                        casketCode = "";
                    vaultCode = custDt.Rows[0]["extraItemAmtMI2"].ObjToString();
                    if (vaultCode.ToUpper().IndexOf("-BAD") >= 0)
                        vaultCode = "";
                    casketPrice = custDt.Rows[0]["extraItemAmtMR1"].ObjToDouble();
                    vaultPrice = custDt.Rows[0]["extraItemAmtMR2"].ObjToDouble();

                    cmd = "Select * from `casket_master` where `casketcode` = '" + casketCode + "';";
                    cDt = G1.get_db_data(cmd);
                    if (cDt.Rows.Count > 0)
                    {
                        price = G1.ReformatMoney(casketPrice);
                        price = price.Replace(".00", "");

                        casketDetail = "(M-$" + price + ") ";
                        casketDetail += cDt.Rows[0]["casketdesc"].ObjToString();
                    }
                    else if ( !String.IsNullOrWhiteSpace ( casketCode ))
                    {
                        price = G1.ReformatMoney(casketPrice);
                        price = price.Replace(".00", "");

                        casketDetail = "(M-$" + price + ") ";
                        casketDetail += casketCode;
                    }
                    cmd = "Select * from `casket_master` where `casketcode` = '" + vaultCode + "';";
                    cDt = G1.get_db_data(cmd);
                    if (cDt.Rows.Count > 0)
                    {
                        price = G1.ReformatMoney(vaultPrice);
                        price = price.Replace(".00", "");

                        vaultDetail = "(M-$" + price + ") ";
                        vaultDetail += cDt.Rows[0]["casketdesc"].ObjToString();
                    }
                    else if ( !String.IsNullOrWhiteSpace ( vaultCode))
                    {
                        price = G1.ReformatMoney(vaultPrice);
                        price = price.Replace(".00", "");

                        vaultDetail = "(M-$" + price + ") ";
                        vaultDetail += vaultCode;
                    }
                }
                issueDate = "IssueDate = NONE";
                if ( ddx.Rows.Count > 0 )
                {
                    date = ddx.Rows[0]["issueDate8"].ObjToDateTime();
                    if (date.Year > 1000)
                        issueDate = "IssueDate = " + date.ToString("MM/dd/yyyy");
                    else if (contractDate.Year > 0)
                        issueDate = "Contract Date = " + contractDate.ToString("MM/dd/yyyy");
                }
                cmd = "Select * from `fcust_services` WHERE `contractNumber` = '" + contract + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    cmd = "Select * from `cust_services` WHERE `contractNumber` = '" + contract + "';";
                    ddx = G1.get_db_data(cmd);
                }
                cName = "C" + (i + 1).ToString();
                dt.Columns.Add(cName);
                name = "S" + (i + 1).ToString();
                dt.Columns.Add(name);

                if (dt.Rows.Count <= 0)
                {
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                    dR = dt.NewRow();
                    dt.Rows.Add(dR);
                }
                dt.Rows[0][name] = "Primary-" + contract;
                dt.Rows[1][name] = issueDate;
                rowNum = 5;

                if ( !String.IsNullOrWhiteSpace ( casketDetail ))
                {
                    if (dt.Rows.Count <= (rowNum))
                    {
                        dR = dt.NewRow();
                        dt.Rows.Add(dR);
                    }
                    dt.Rows[rowNum][name] = casketDetail;
                    rowNum++;
                }
                if (!String.IsNullOrWhiteSpace(vaultDetail))
                {
                    if (dt.Rows.Count <= (rowNum))
                    {
                        dR = dt.NewRow();
                        dt.Rows.Add(dR);
                    }
                    dt.Rows[rowNum][name] = vaultDetail;
                    rowNum++;
                }





                lastCasket = "";
                lastVault = "";

                dt.Rows[2][name] = "Select Merchandise";
                dt.Rows[3][name] = "Select Services";
                dt.Rows[4][name] = "Select Cash Advance";
                for ( int j=0; j<ddx.Rows.Count; j++)
                {
                    service = ddx.Rows[j]["service"].ObjToString();
                    type = ddx.Rows[j]["type"].ObjToString();
                    data = ddx.Rows[j]["data"].ObjToString();
                    price = ddx.Rows[j]["data"].ObjToString();
                    dValue = price.ObjToDouble();
                    if ( dValue <= 0D)
                    {
                        price = ddx.Rows[j]["price"].ObjToString();
                        dValue = price.ObjToDouble();
                    }
                    if ( type.ToUpper() == "MERCHANDISE")
                    {
                    }    
                    if (dValue <= 0D)
                    {
                        if ( service.ToUpper() != "CASKET NAME" && service.ToUpper() != "OUTER CONTAINER NAME")
                            continue;
                    }
                    if (String.IsNullOrWhiteSpace(type))
                        type = "S";
                    else
                    {
                        str = type.Substring(0, 1);
                        type = str.ToUpper();
                    }
                    service = ddx.Rows[j]["service"].ObjToString();
                    if ( dt.Rows.Count <= (rowNum))
                    {
                        dR = dt.NewRow();
                        dt.Rows.Add(dR);
                    }
                    if (service.ToUpper() == "CASKET NAME" && String.IsNullOrWhiteSpace(lastCasket))
                    {
                        lastCasket = data;
                        continue;
                    }
                    if (service.ToUpper() == "OUTER CONTAINER NAME" && String.IsNullOrWhiteSpace(lastVault))
                    {
                        lastVault = data;
                        continue;
                    }
                    if ( type == "M" && service.Trim().ToUpper() == "CASKET PRICE" && !String.IsNullOrEmpty ( price))
                    {
                        Lines = lastCasket.Split(' ');
                        if (Lines.Length <= 0)
                            continue;

                        service = Lines[0].Trim();
                        cmd = "Select * from `casket_master` where `casketcode` = '" + Lines[0].Trim() + "';";
                        cDt = G1.get_db_data(cmd);
                        if ( cDt.Rows.Count <= 0 && Lines[0].Trim().Length > 3 )
                        {
                            lastCasket = Lines[0].Trim().Substring(0, 3);
                            cmd = "Select * from `casket_master` where `casketcode` LIKE '" + lastCasket + "%';";
                            cDt = G1.get_db_data(cmd);
                        }
                        if (cDt.Rows.Count > 0)
                            service = cDt.Rows[0]["casketdesc"].ObjToString();
                    }
                    else if (type == "M" && service.Trim().ToUpper() == "OUTER CONTAINER PRICE" && !String.IsNullOrEmpty(price))
                    {
                        service = lastVault;
                        cmd = "Select * from `casket_master` where `casketcode` = '" + service + "';";
                        cDt = G1.get_db_data(cmd);
                        if (cDt.Rows.Count > 0)
                            service = cDt.Rows[0]["casketdesc"].ObjToString();
                    }
                    str = "(" + type;
                    if (!String.IsNullOrWhiteSpace(price))
                    {
                        if ( G1.validate_numeric ( price))
                        {
                            dValue = price.ObjToDouble();
                            price = G1.ReformatMoney(dValue);
                            price = price.Replace(".00", "");
                        }
                        str += "-$" + price;
                    }
                    str += ") " + service;
                    dRows = dt.Select(name + "='" + str + "'");
                    if ( dRows.Length <= 0 )
                        dt.Rows[rowNum][name] = str;
                    rowNum++;
                }
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                if (i < 4)
                {
                    contract = dx.Rows[i]["contractNumber"].ObjToString();
                    name = "S" + (i + 1).ToString();
                    gridMain.Columns[name].Caption = contract;
                }
                else
                {
                    G1.AddNewColumn(gridMain, "S" + (i + 1).ToString(), "S" + (i + 1).ToString(), "", FormatType.None, 300, true);
                    G1.SetColumnWidth(gridMain, "S" + (i + 1).ToString(), 300);
                    G1.AddNewColumn(gridMain, "C" + (i+1).ToString(), "C" + (i+1).ToString(), "", FormatType.None, 30, true);
                    G1.SetColumnWidth(gridMain, "C" + (i+1).ToString(), 30 );

                    contract = dx.Rows[i]["contractNumber"].ObjToString();
                    name = "S" + (i + 1).ToString();
                    gridMain.Columns[name].Caption = contract;
                }

                gridMain.Columns["C" + (i + 1).ToString()].ColumnEdit = this.repositoryItemCheckEdit4;
                SetupCheckColumn(this.repositoryItemCheckEdit4, dt, "C" + (i + 1).ToString());
            }

            SetDefaultPositions(dt);

            SetupCheckColumn(this.repositoryItemCheckEdit1, dt, "C1");
            SetupCheckColumn(this.repositoryItemCheckEdit2, dt, "C2");
            if ( dx.Rows.Count > 2 )
                SetupCheckColumn(this.repositoryItemCheckEdit3, dt, "C3");
            else
            {
                gridMain.Columns["C3"].Visible = false;
                gridMain.Columns["S3"].Visible = false;
            }
            if (dx.Rows.Count > 3)
                SetupCheckColumn(this.repositoryItemCheckEdit4, dt, "C4");
            else
            {
                gridMain.Columns["C4"].Visible = false;
                gridMain.Columns["S4"].Visible = false;
            }
            //for (int i = 0; i < dx.Rows.Count; i++)
            //{
            //    contract = dx.Rows[i]["contractNumber"].ObjToString();
            //    if (contract == workContract)
            //    {
            //        name = "C" + (i + 1).ToString();
            //        if ( dt.Rows.Count <= 0 )
            //        {
            //            DataRow dRR = dt.NewRow();
            //            dRR[name] = "1";
            //        }
            //        else
            //            dt.Rows[0][name] = "1";
            //    }
            //}

            //CleanupServices(dt);

            dgv.DataSource = dt;
            lblCurrentPrimary.Text = "Current Primary Contract Number : " + workContract;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void SetDefaultPositions( DataTable dt )
        {
            string name = "";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                name = dt.Columns[i].ColumnName.ToString();
                G1.SetColumnPosition(gridMain, name, (i+1));
            }
        }
        /***********************************************************************************************/
        private DataTable CleanupServices ( DataTable dt)
        {
            string column = "";
            int col = 0;
            string service = "";
            int lastRow = 0;
            for ( int i=1; i<=4; i++)
            {
                column = "C" + i.ToString();
                if (G1.get_column_number(dt, column) < 0)
                    break;
                col = G1.get_column_number(dt, column);
                col++;
                if (col >= dt.Columns.Count)
                    continue;
                lastRow = 0;
                for ( int j=0; j<dt.Rows.Count; j++)
                {
                    service = dt.Rows[j][col].ObjToString();
                    if (service.IndexOf("-$0") > 0)
                        dt.Rows[j][col] = "";
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void SetupCheckColumn(DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew, DataTable dt, string columnName )
        {
            //DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, columnName) < 0)
                dt.Columns.Add(columnName);
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i][columnName] = "0";
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            OnSelectDone( dt );
            btnSave.Visible = false;
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain.FocusedColumn;
            string contract = currCol.Caption;
            if ( contract == "C1" || contract == "C2" || contract == "C3" || contract == "C4")
            {
                int col = gridMain.FocusedColumn.AbsoluteIndex + 1;
                contract = gridMain.Columns[col].Caption;
            }
            if ( !String.IsNullOrWhiteSpace ( contract ))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt);
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            SelectDone?.Invoke(dt);
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            btnSave.Show();
            try
            {
                bool isChecked = true;
                DevExpress.XtraEditors.CheckEdit select = (DevExpress.XtraEditors.CheckEdit)sender;
                if (select.Checked)
                    isChecked = false;

                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName;
                string column = currentColumn.Replace("C", "S");

                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                string service = dr[column].ObjToString();
                if ( service.IndexOf ( "Primary-") >= 0 && isChecked )
                {
                    int rowHandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    for ( int i=1; i<=dt.Columns.Count; i=i+2)
                    {
                        if (column != dt.Columns[i].ColumnName)
                            DoSelectAll(dt.Columns[i-1].ColumnName, false);
                    }
                    //if (column != "S1")
                    //{
                    //    //dt.Rows[row]["C1"] = 0;
                    //    DoSelectAll("C1", false);
                    //}
                    //if (column != "S2")
                    //{
                    //    //dt.Rows[row]["C2"] = 0;
                    //    DoSelectAll("C2", false);
                    //}
                    //if (column != "S3")
                    //{
                    //    //dt.Rows[row]["C3"] = 0;
                    //    DoSelectAll("C3", false);
                    //}
                    //if (column != "S4")
                    //{
                    //    //dt.Rows[row]["C4"] = 0;
                    //    DoSelectAll("C4", false);
                    //}
                    column = currentColumn.Replace("S", "C");
                    DoSelectAll(column, true);
                    gridMain.RefreshData();
                }
                if (service == "Select Merchandise")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        service = dt.Rows[i][column].ObjToString();
                        if (service.IndexOf("(M") == 0)
                        {
                            if (isChecked)
                                dt.Rows[i][currentColumn] = 1;
                            else
                                dt.Rows[i][currentColumn] = 0;
                        }
                    }
                }
                else if (service == "Select Services")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        service = dt.Rows[i][column].ObjToString();
                        if (service.IndexOf("(S") == 0)
                        {
                            if (isChecked)
                                dt.Rows[i][currentColumn] = 1;
                            else
                                dt.Rows[i][currentColumn] = 0;
                        }
                    }
                }
                else if (service == "Select Cash Advance")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        service = dt.Rows[i][column].ObjToString();
                        if (service.IndexOf("(C") == 0)
                        {
                            if (isChecked)
                                dt.Rows[i][currentColumn] = 1;
                            else
                                dt.Rows[i][currentColumn] = 0;
                        }
                    }
                }
                else
                {
                    int idx = service.IndexOf(')');
                    if (idx < 0)
                        return;
                    string what = service.Substring(idx + 1);
                    string c = "";
                    int count = dt.Columns.Count / 2;
                    for ( int j=1; j<=count; j++ )
                    {
                        c = "S" + j.ToString();
                        if (c == column)
                            continue;
                        if (G1.get_column_number(dt, c) < 0)
                            continue;
                        DataRow[] dRows = dt.Select(c + " LIKE '%" + what + "'");
                        if (dRows.Length > 0)
                            dRows[0]["C" + j.ToString()] = "0";
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show ("***ERROR*** Selecting Services!", "Selecting Services Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /****************************************************************************************/
        private void DoSelectAll ( string column, bool select )
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int checkCol = G1.get_column_number(dt, column);
            if ( checkCol < 0)
                return;
            checkCol++;
            string service = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                if (select)
                {
                    service = dt.Rows[i][checkCol].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( service))
                        dt.Rows[i][column] = "1";
                }
                else
                    dt.Rows[i][column] = "0";
            }
        }
        /****************************************************************************************/
        private void DuplicateSSN_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnSave.Visible)
                return;
            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if ( result == DialogResult.Cancel )
            {
                e.Cancel = true;
                return;
            }
            if (result == DialogResult.Yes)
            {
                btnSave.Visible = false;
                btnSave_Click(null, null);
            }
        }
        /***********************************************************************************************/
        private void ProcessImportedData(DataTable dt, string casketCode, double casketPrice, string type)
        {
            if (String.IsNullOrWhiteSpace(casketCode))
                return;
            if (G1.get_column_number(dt, "data") >= 0)
            {
                DataRow[] dRows = dt.Select("data='" + casketCode + "'");
                if (dRows.Length > 0)
                {
                    if (dRows[0]["service"].ObjToString().ToUpper() == "CASKET NAME")
                    {
                        dRows = dt.Select("service='Casket Price'");
                        if (dRows.Length > 0)
                            return;
                    }
                    else if (dRows[0]["service"].ObjToString().ToUpper() == "OUTER CONTAINER NAME")
                    {
                        dRows = dt.Select("service='Outer Container Price'");
                        if (dRows.Length > 0)
                            return;
                    }
                }
            }
            string cmd = "Select * from `casket_master` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                cmd = "Select * from `casket_master` where `casketcode` LIKE '" + casketCode + "%';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    AddDefaultMerchandise(dt, casketCode, casketPrice, type);
                    return;
                }
            }

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            string masterRecord = dx.Rows[0]["record"].ObjToString();
            double cost = dx.Rows[0]["casketcost"].ObjToDouble();
            double rounding = dx.Rows[0]["round"].ObjToDouble();
            string service = dx.Rows[0]["casketdesc"].ObjToString();

            bool gotVault = false;

            double currentPrice = 0D;
            DataRow dR = null;
            DataRow[] ddR = null;

            string chr = casketCode.Substring(0, 1).ToUpper();
            if (chr == "V" || casketCode.IndexOf("URN") >= 0)
            {
                //                currentPrice = dx.Rows[0]["casketprice"].ObjToDouble();
                if (service.IndexOf(casketCode) < 0)
                    service = casketCode + " " + service;
                ddR = dt.Select("service LIKE'" + casketCode + "%'");
                if (ddR.Length > 0)
                    gotVault = true;
                else
                    ddR = dt.Select("service='" + service + "'");
                if (ddR.Length > 0)
                {
                    if (currentPrice <= 0D)
                    {
                        cmd = "Select * from `casket_master` where `casketCode` = '" + casketCode + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            currentPrice = dx.Rows[0]["casketcost"].ObjToDouble();
                            if (currentPrice <= 0D)
                                currentPrice = dx.Rows[0]["casketprice"].ObjToDouble();
                        }
                    }
                    ddR[0]["currentprice"] = currentPrice;
                    //                ddR[0]["price"] = casketPrice;
                    ddR[0]["type"] = "Merchandise";
                    ddR[0]["status"] = "Imported";
                }
                else
                {
                    dR = dt.NewRow();
                    dR["service"] = service;
                    dR["currentprice"] = cost;
                    dR["price"] = casketPrice;
                    dR["type"] = "Merchandise";
                    dR["status"] = "Imported";
                    if (G1.get_column_number(dt, "mod") >= 0)
                        dR["mod"] = "1";
                    dt.Rows.Add(dR);
                }
                return;
            }

            cmd = "Select * from `casket_packages` where `!masterRecord` = '" + masterRecord + "' and `groupname` = '" + group + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            if (rounding > 0D)
                cost = Caskets.RoundTo(cost, rounding);

            double markup = dx.Rows[0]["markup"].ObjToDouble();
            currentPrice = cost * markup;
            currentPrice = G1.RoundValue(currentPrice);
            ddR = dt.Select("service='" + service + "'");
            if (ddR.Length > 0)
            {
                ddR[0]["currentprice"] = currentPrice;
                //                ddR[0]["price"] = casketPrice;
                ddR[0]["type"] = "Merchandise";
                ddR[0]["status"] = "Imported";
            }
            else
            {
                dR = dt.NewRow();
                dR["service"] = service;
                dR["currentprice"] = currentPrice;
                dR["price"] = casketPrice;
                dR["type"] = "Merchandise";
                dR["status"] = "Imported";
                if (G1.get_column_number(dt, "mod") >= 0)
                    dR["mod"] = "1";
                dt.Rows.Add(dR);
            }
        }
        /***********************************************************************************************/
        private void AddDefaultMerchandise(DataTable dt, string casketCode, double casketPrice, string type)
        {
            DataRow dR = dt.NewRow();
            string what = "Casket";
            if (casketCode.Length > 0)
            {
                string dd = casketCode.Substring(0, 1).ToUpper();
                if (dd == "V")
                    what = "VAULT";
                else if (dd == "U")
                    what = "URN";
                else
                    what = type;
            }
            dR["service"] = casketCode + " " + what;
            dR["currentprice"] = casketPrice;
            dR["price"] = casketPrice;
            dR["type"] = "Merchandise";
            dR["status"] = "Imported";
            if (G1.get_column_number(dt, "mod") >= 0)
                dR["mod"] = "1";
            dt.Rows.Add(dR);
        }
        /****************************************************************************************/
    }
}