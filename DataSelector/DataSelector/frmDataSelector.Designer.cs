namespace DataSelector
{
    partial class frmDataSelector
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
            this.lblColumns = new System.Windows.Forms.Label();
            this.txtColumns = new System.Windows.Forms.TextBox();
            this.lblWhere = new System.Windows.Forms.Label();
            this.txtWhere = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtGroupBy = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOrderBy = new System.Windows.Forms.TextBox();
            this.chkLogFile = new System.Windows.Forms.CheckBox();
            this.lbTables = new System.Windows.Forms.Label();
            this.lstTables = new System.Windows.Forms.ListBox();
            this.lblOutFormat = new System.Windows.Forms.Label();
            this.cmbOutFormat = new System.Windows.Forms.ComboBox();
            this.chkSymbology = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblColumns
            // 
            this.lblColumns.AutoSize = true;
            this.lblColumns.Location = new System.Drawing.Point(13, 13);
            this.lblColumns.Name = "lblColumns";
            this.lblColumns.Size = new System.Drawing.Size(50, 13);
            this.lblColumns.TabIndex = 0;
            this.lblColumns.Text = "Columns:";
            // 
            // txtColumns
            // 
            this.txtColumns.Location = new System.Drawing.Point(16, 30);
            this.txtColumns.Multiline = true;
            this.txtColumns.Name = "txtColumns";
            this.txtColumns.Size = new System.Drawing.Size(279, 151);
            this.txtColumns.TabIndex = 1;
            // 
            // lblWhere
            // 
            this.lblWhere.AutoSize = true;
            this.lblWhere.Location = new System.Drawing.Point(16, 188);
            this.lblWhere.Name = "lblWhere";
            this.lblWhere.Size = new System.Drawing.Size(42, 13);
            this.lblWhere.TabIndex = 2;
            this.lblWhere.Text = "Where:";
            // 
            // txtWhere
            // 
            this.txtWhere.Location = new System.Drawing.Point(19, 204);
            this.txtWhere.Multiline = true;
            this.txtWhere.Name = "txtWhere";
            this.txtWhere.Size = new System.Drawing.Size(276, 74);
            this.txtWhere.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 281);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Group By:";
            // 
            // txtGroupBy
            // 
            this.txtGroupBy.Location = new System.Drawing.Point(19, 297);
            this.txtGroupBy.Multiline = true;
            this.txtGroupBy.Name = "txtGroupBy";
            this.txtGroupBy.Size = new System.Drawing.Size(276, 68);
            this.txtGroupBy.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 369);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Order By:";
            // 
            // txtOrderBy
            // 
            this.txtOrderBy.Location = new System.Drawing.Point(19, 385);
            this.txtOrderBy.Multiline = true;
            this.txtOrderBy.Name = "txtOrderBy";
            this.txtOrderBy.Size = new System.Drawing.Size(276, 68);
            this.txtOrderBy.TabIndex = 7;
            // 
            // chkLogFile
            // 
            this.chkLogFile.AutoSize = true;
            this.chkLogFile.Checked = true;
            this.chkLogFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogFile.Location = new System.Drawing.Point(24, 460);
            this.chkLogFile.Name = "chkLogFile";
            this.chkLogFile.Size = new System.Drawing.Size(96, 17);
            this.chkLogFile.TabIndex = 8;
            this.chkLogFile.Text = "Clear Log File?";
            this.chkLogFile.UseVisualStyleBackColor = true;
            // 
            // lbTables
            // 
            this.lbTables.AutoSize = true;
            this.lbTables.Location = new System.Drawing.Point(326, 13);
            this.lbTables.Name = "lbTables";
            this.lbTables.Size = new System.Drawing.Size(66, 13);
            this.lbTables.TabIndex = 9;
            this.lbTables.Text = "SQL Tables:";
            // 
            // lstTables
            // 
            this.lstTables.FormattingEnabled = true;
            this.lstTables.Location = new System.Drawing.Point(329, 30);
            this.lstTables.Name = "lstTables";
            this.lstTables.Size = new System.Drawing.Size(206, 95);
            this.lstTables.TabIndex = 10;
            // 
            // lblOutFormat
            // 
            this.lblOutFormat.AutoSize = true;
            this.lblOutFormat.Location = new System.Drawing.Point(326, 139);
            this.lblOutFormat.Name = "lblOutFormat";
            this.lblOutFormat.Size = new System.Drawing.Size(96, 13);
            this.lblOutFormat.TabIndex = 11;
            this.lblOutFormat.Text = "Output File Format:";
            // 
            // cmbOutFormat
            // 
            this.cmbOutFormat.FormattingEnabled = true;
            this.cmbOutFormat.Items.AddRange(new object[] {
            "Geodatabase",
            "Shapefile",
            "Text file",
            "dBASE file"});
            this.cmbOutFormat.Location = new System.Drawing.Point(329, 156);
            this.cmbOutFormat.Name = "cmbOutFormat";
            this.cmbOutFormat.Size = new System.Drawing.Size(206, 21);
            this.cmbOutFormat.TabIndex = 12;
            // 
            // chkSymbology
            // 
            this.chkSymbology.AutoSize = true;
            this.chkSymbology.Checked = true;
            this.chkSymbology.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSymbology.Location = new System.Drawing.Point(329, 188);
            this.chkSymbology.Name = "chkSymbology";
            this.chkSymbology.Size = new System.Drawing.Size(135, 17);
            this.chkSymbology.TabIndex = 13;
            this.chkSymbology.Text = "Set default symbology?";
            this.chkSymbology.UseVisualStyleBackColor = true;
            this.chkSymbology.Visible = false;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(139, 459);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(220, 459);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 15;
            this.btnLoad.Text = "&Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(379, 459);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(460, 459);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 17;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(379, 341);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 41);
            this.button1.TabIndex = 18;
            this.button1.Text = "Functionality Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmDataSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 502);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkSymbology);
            this.Controls.Add(this.cmbOutFormat);
            this.Controls.Add(this.lblOutFormat);
            this.Controls.Add(this.lstTables);
            this.Controls.Add(this.lbTables);
            this.Controls.Add(this.chkLogFile);
            this.Controls.Add(this.txtOrderBy);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtGroupBy);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtWhere);
            this.Controls.Add(this.lblWhere);
            this.Controls.Add(this.txtColumns);
            this.Controls.Add(this.lblColumns);
            this.Name = "frmDataSelector";
            this.Text = "Data Selector 1.0";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.TextBox txtColumns;
        private System.Windows.Forms.Label lblWhere;
        private System.Windows.Forms.TextBox txtWhere;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtGroupBy;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOrderBy;
        private System.Windows.Forms.CheckBox chkLogFile;
        private System.Windows.Forms.Label lbTables;
        private System.Windows.Forms.ListBox lstTables;
        private System.Windows.Forms.Label lblOutFormat;
        private System.Windows.Forms.ComboBox cmbOutFormat;
        private System.Windows.Forms.CheckBox chkSymbology;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button button1;
    }
}