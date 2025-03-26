namespace SMFS
{
    partial class PlotData
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.zedPlot = new ZedGraph.ZedGraphControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.chkActual = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDegree = new System.Windows.Forms.TextBox();
            this.chkRegress = new System.Windows.Forms.CheckBox();
            this.btnPlot = new System.Windows.Forms.Button();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.zeroBox = new System.Windows.Forms.CheckBox();
            this.multiCheckBox = new System.Windows.Forms.CheckBox();
            this.checkBox_smoothLines = new System.Windows.Forms.CheckBox();
            this.dateTimeBox = new System.Windows.Forms.CheckBox();
            this.checkBox_showTrendLines = new System.Windows.Forms.CheckBox();
            this.metricBox = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.dataBox = new System.Windows.Forms.CheckedListBox();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // zedPlot
            // 
            this.zedPlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedPlot.IsAntiAlias = true;
            this.zedPlot.IsShowPointValues = true;
            this.zedPlot.Location = new System.Drawing.Point(0, 0);
            this.zedPlot.Name = "zedPlot";
            this.zedPlot.ScrollGrace = 0;
            this.zedPlot.ScrollMaxX = 0;
            this.zedPlot.ScrollMaxY = 0;
            this.zedPlot.ScrollMaxY2 = 0;
            this.zedPlot.ScrollMinX = 0;
            this.zedPlot.ScrollMinY = 0;
            this.zedPlot.ScrollMinY2 = 0;
            this.zedPlot.Size = new System.Drawing.Size(927, 715);
            this.zedPlot.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.zedPlot);
            this.panel1.Location = new System.Drawing.Point(198, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(927, 715);
            this.panel1.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.chkActual);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.txtDegree);
            this.panel3.Controls.Add(this.chkRegress);
            this.panel3.Controls.Add(this.btnPlot);
            this.panel3.Controls.Add(this.dateTimePicker2);
            this.panel3.Controls.Add(this.dateTimePicker1);
            this.panel3.Controls.Add(this.zeroBox);
            this.panel3.Controls.Add(this.multiCheckBox);
            this.panel3.Controls.Add(this.checkBox_smoothLines);
            this.panel3.Controls.Add(this.dateTimeBox);
            this.panel3.Controls.Add(this.checkBox_showTrendLines);
            this.panel3.Controls.Add(this.metricBox);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(190, 263);
            this.panel3.TabIndex = 4;
            // 
            // chkActual
            // 
            this.chkActual.AutoSize = true;
            this.chkActual.Location = new System.Drawing.Point(104, 94);
            this.chkActual.Name = "chkActual";
            this.chkActual.Size = new System.Drawing.Size(82, 17);
            this.chkActual.TabIndex = 16;
            this.chkActual.Text = "Actual Data";
            this.chkActual.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(111, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Degree";
            // 
            // txtDegree
            // 
            this.txtDegree.Location = new System.Drawing.Point(93, 48);
            this.txtDegree.Name = "txtDegree";
            this.txtDegree.Size = new System.Drawing.Size(16, 20);
            this.txtDegree.TabIndex = 3;
            this.txtDegree.Text = "3";
            this.txtDegree.TextChanged += new System.EventHandler(this.txtDegree_TextChanged);
            // 
            // chkRegress
            // 
            this.chkRegress.AutoSize = true;
            this.chkRegress.Location = new System.Drawing.Point(9, 49);
            this.chkRegress.Name = "chkRegress";
            this.chkRegress.Size = new System.Drawing.Size(79, 17);
            this.chkRegress.TabIndex = 15;
            this.chkRegress.Text = "Regression";
            this.chkRegress.UseVisualStyleBackColor = true;
            this.chkRegress.CheckedChanged += new System.EventHandler(this.chkRegress_CheckedChanged);
            // 
            // btnPlot
            // 
            this.btnPlot.Location = new System.Drawing.Point(46, 215);
            this.btnPlot.Name = "btnPlot";
            this.btnPlot.Size = new System.Drawing.Size(75, 23);
            this.btnPlot.TabIndex = 3;
            this.btnPlot.Text = "RePlot";
            this.btnPlot.UseVisualStyleBackColor = true;
            this.btnPlot.Click += new System.EventHandler(this.btnPlot_Click);
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.Location = new System.Drawing.Point(3, 189);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(180, 20);
            this.dateTimePicker2.TabIndex = 14;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(4, 163);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(180, 20);
            this.dateTimePicker1.TabIndex = 3;
            // 
            // zeroBox
            // 
            this.zeroBox.AutoSize = true;
            this.zeroBox.Checked = true;
            this.zeroBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.zeroBox.Location = new System.Drawing.Point(9, 140);
            this.zeroBox.Name = "zeroBox";
            this.zeroBox.Size = new System.Drawing.Size(99, 17);
            this.zeroBox.TabIndex = 5;
            this.zeroBox.Text = "Filter Zero Data";
            this.zeroBox.UseVisualStyleBackColor = true;
            // 
            // multiCheckBox
            // 
            this.multiCheckBox.AutoSize = true;
            this.multiCheckBox.Location = new System.Drawing.Point(9, 3);
            this.multiCheckBox.Name = "multiCheckBox";
            this.multiCheckBox.Size = new System.Drawing.Size(79, 17);
            this.multiCheckBox.TabIndex = 13;
            this.multiCheckBox.Text = "Multi-Track";
            this.multiCheckBox.UseVisualStyleBackColor = true;
            this.multiCheckBox.CheckedChanged += new System.EventHandler(this.multiCheckBox_CheckedChanged);
            // 
            // checkBox_smoothLines
            // 
            this.checkBox_smoothLines.AutoSize = true;
            this.checkBox_smoothLines.Location = new System.Drawing.Point(9, 71);
            this.checkBox_smoothLines.Name = "checkBox_smoothLines";
            this.checkBox_smoothLines.Size = new System.Drawing.Size(90, 17);
            this.checkBox_smoothLines.TabIndex = 11;
            this.checkBox_smoothLines.Text = "Smooth Lines";
            this.checkBox_smoothLines.UseVisualStyleBackColor = true;
            this.checkBox_smoothLines.CheckedChanged += new System.EventHandler(this.checkBox_smoothLines_CheckedChanged);
            // 
            // dateTimeBox
            // 
            this.dateTimeBox.AutoSize = true;
            this.dateTimeBox.Checked = true;
            this.dateTimeBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dateTimeBox.Location = new System.Drawing.Point(9, 117);
            this.dateTimeBox.Name = "dateTimeBox";
            this.dateTimeBox.Size = new System.Drawing.Size(123, 17);
            this.dateTimeBox.TabIndex = 4;
            this.dateTimeBox.Text = "Use DateTime Label";
            this.dateTimeBox.UseVisualStyleBackColor = true;
            // 
            // checkBox_showTrendLines
            // 
            this.checkBox_showTrendLines.AutoSize = true;
            this.checkBox_showTrendLines.Location = new System.Drawing.Point(9, 26);
            this.checkBox_showTrendLines.Name = "checkBox_showTrendLines";
            this.checkBox_showTrendLines.Size = new System.Drawing.Size(112, 17);
            this.checkBox_showTrendLines.TabIndex = 12;
            this.checkBox_showTrendLines.Text = "Show Trend Lines";
            this.checkBox_showTrendLines.UseVisualStyleBackColor = true;
            this.checkBox_showTrendLines.CheckedChanged += new System.EventHandler(this.checkBox_showTrendLines_CheckedChanged);
            // 
            // metricBox
            // 
            this.metricBox.AutoSize = true;
            this.metricBox.Location = new System.Drawing.Point(9, 94);
            this.metricBox.Name = "metricBox";
            this.metricBox.Size = new System.Drawing.Size(77, 17);
            this.metricBox.TabIndex = 2;
            this.metricBox.Text = "Use Metric";
            this.metricBox.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Location = new System.Drawing.Point(2, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(190, 263);
            this.panel2.TabIndex = 3;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.dataBox);
            this.panel4.Location = new System.Drawing.Point(2, 264);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(190, 435);
            this.panel4.TabIndex = 3;
            // 
            // dataBox
            // 
            this.dataBox.CheckOnClick = true;
            this.dataBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataBox.FormattingEnabled = true;
            this.dataBox.Location = new System.Drawing.Point(0, 0);
            this.dataBox.Name = "dataBox";
            this.dataBox.Size = new System.Drawing.Size(190, 424);
            this.dataBox.TabIndex = 5;
            this.dataBox.SelectedIndexChanged += new System.EventHandler(this.dataBox_SelectedIndexChanged);
            // 
            // PlotData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1124, 711);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "PlotData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PlotData";
            this.Load += new System.EventHandler(this.PlotData_Load);
            this.Resize += new System.EventHandler(this.PlotData_Resize);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ZedGraph.ZedGraphControl zedPlot;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox zeroBox;
        private System.Windows.Forms.CheckBox multiCheckBox;
        private System.Windows.Forms.CheckBox checkBox_smoothLines;
        private System.Windows.Forms.CheckBox dateTimeBox;
        private System.Windows.Forms.CheckBox checkBox_showTrendLines;
        private System.Windows.Forms.CheckBox metricBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.CheckedListBox dataBox;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button btnPlot;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDegree;
        private System.Windows.Forms.CheckBox chkRegress;
        private System.Windows.Forms.CheckBox chkActual;
    }
}