namespace DataAcquisition
{
    partial class FilePathSeclect
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCsvFilePath = new System.Windows.Forms.TextBox();
            this.buttonSelectPath = new System.Windows.Forms.Button();
            this.buttonSelectPathOK = new System.Windows.Forms.Button();
            this.buttonSelectPathCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "csv 文件路径：";
            // 
            // textBoxCsvFilePath
            // 
            this.textBoxCsvFilePath.Location = new System.Drawing.Point(95, 20);
            this.textBoxCsvFilePath.Multiline = true;
            this.textBoxCsvFilePath.Name = "textBoxCsvFilePath";
            this.textBoxCsvFilePath.Size = new System.Drawing.Size(247, 44);
            this.textBoxCsvFilePath.TabIndex = 1;
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Location = new System.Drawing.Point(357, 18);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(75, 27);
            this.buttonSelectPath.TabIndex = 2;
            this.buttonSelectPath.Text = "选择";
            this.buttonSelectPath.UseVisualStyleBackColor = true;
            this.buttonSelectPath.Click += new System.EventHandler(this.buttonSelectPath_Click);
            // 
            // buttonSelectPathOK
            // 
            this.buttonSelectPathOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSelectPathOK.Location = new System.Drawing.Point(267, 77);
            this.buttonSelectPathOK.Name = "buttonSelectPathOK";
            this.buttonSelectPathOK.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectPathOK.TabIndex = 3;
            this.buttonSelectPathOK.Text = "确定";
            this.buttonSelectPathOK.UseVisualStyleBackColor = true;
            this.buttonSelectPathOK.Click += new System.EventHandler(this.buttonSelectPathOK_Click);
            // 
            // buttonSelectPathCancel
            // 
            this.buttonSelectPathCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonSelectPathCancel.Location = new System.Drawing.Point(357, 77);
            this.buttonSelectPathCancel.Name = "buttonSelectPathCancel";
            this.buttonSelectPathCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectPathCancel.TabIndex = 4;
            this.buttonSelectPathCancel.Text = "取消";
            this.buttonSelectPathCancel.UseVisualStyleBackColor = true;
            // 
            // FilePathSeclect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 112);
            this.Controls.Add(this.buttonSelectPathCancel);
            this.Controls.Add(this.buttonSelectPathOK);
            this.Controls.Add(this.buttonSelectPath);
            this.Controls.Add(this.textBoxCsvFilePath);
            this.Controls.Add(this.label1);
            this.Name = "FilePathSeclect";
            this.Text = "FilePathSeclect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCsvFilePath;
        private System.Windows.Forms.Button buttonSelectPath;
        private System.Windows.Forms.Button buttonSelectPathOK;
        private System.Windows.Forms.Button buttonSelectPathCancel;
    }
}