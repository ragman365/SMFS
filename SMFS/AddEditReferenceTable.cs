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
using java.awt;
using DevExpress.CodeParser;
/****************************************************************************************/

namespace SMFS
{
    /****************************************************************************************/
    public partial class AddEditReferenceTable : DevExpress.XtraEditors.XtraForm
    {
        private string workReference = "";
        private string workRecord = "";
        private bool workProtect = false;
        private bool modified = false;
        /****************************************************************************************/
        public AddEditReferenceTable( string record, string referenceIn, bool protect )
        {
            InitializeComponent();
            workRecord = record;
            workReference = referenceIn;
            workProtect = protect;
        }
        /****************************************************************************************/
        private void AddEditReferenceTable_Load(object sender, EventArgs e)
        {
            string cmd = "Show Tables;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string table = "";
            DataRow[] dRows = dt.Select("Tables_in_SMFS LIKE 'ref_%'");
            for (int i = 0; i < dRows.Length; i++)
            {
                table = dRows[i]["Tables_in_SMFS"].ObjToString();
                table = table.Replace("ref_", "");
                cmbReference.Items.Add(table);
            }
            cmbReference.Text = workReference;
            chkProtect.Checked = workProtect;
            modified = false;
        }
        /****************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            modified = false;
            this.Close();
        }
        /****************************************************************************************/
        private void AddEditReferenceTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
            {
                return;
            }
            if (MessageBox.Show("Selection Modified.\nAre you sure you want to exit without saving?", "Selection Modified Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Cancel;
                return;
            }
            modified = false;
            SaveResult();
            this.DialogResult = DialogResult.Yes;
        }
        /****************************************************************************************/
        private void chkProtect_CheckedChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void cmbReference_SelectedIndexChanged(object sender, EventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            SaveResult();
            modified = false;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }
        /****************************************************************************************/
        private void btnRemove_Click(object sender, EventArgs e)
        {
            cmbReference.Text = "";
            SaveResult();
            modified = false;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }
        /****************************************************************************************/
        private string DetermineTableField ( string reference)
        {
            string column = "";
            string field = reference;
            string table = "ref_" + reference;
            string cmd = "Select * from `" + table + "`;";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Columns.Count; i++)
            {
                try
                {
                    column = dx.Columns[i].ColumnName.Trim();
                    if (column.ToUpper() == "RECORD")
                    {
                        if (i < dx.Columns.Count)
                        {
                            field = dx.Columns[i + 1].ColumnName.Trim();
                            break;
                        }
                    }
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** Bad Reference Column for Table " + reference);
                }
            }
            if (String.IsNullOrWhiteSpace(field))
                field = reference;
            return field;
        }
        /****************************************************************************************/
        private void SaveResult ()
        {
            string reference = cmbReference.Text;
            bool protect = chkProtect.Checked;
            if ( String.IsNullOrWhiteSpace ( reference ))
                G1.update_db_table("cust_extended_layout", "record", workRecord, new string[] { "reference", "" });
            else
            {
                string field = DetermineTableField(reference);
                reference = "$ref_" + reference + "=" + field;
                if (protect)
                    reference += "/protect";
                G1.update_db_table("cust_extended_layout", "record", workRecord, new string[] { "reference", reference });
            }
        }
        /****************************************************************************************/
    }
}