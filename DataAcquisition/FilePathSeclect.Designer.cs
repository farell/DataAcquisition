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
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelDecription = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonModifyDeviceId = new System.Windows.Forms.Button();
            this.textBoxSensorId = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxComPort = new System.Windows.Forms.TextBox();
            this.textBoxDeviceId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "csv 文件路径：";
            // 
            // textBoxCsvFilePath
            // 
            this.textBoxCsvFilePath.Location = new System.Drawing.Point(127, 25);
            this.textBoxCsvFilePath.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxCsvFilePath.Multiline = true;
            this.textBoxCsvFilePath.Name = "textBoxCsvFilePath";
            this.textBoxCsvFilePath.Size = new System.Drawing.Size(328, 54);
            this.textBoxCsvFilePath.TabIndex = 1;
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Location = new System.Drawing.Point(476, 25);
            this.buttonSelectPath.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(100, 34);
            this.buttonSelectPath.TabIndex = 2;
            this.buttonSelectPath.Text = "选择";
            this.buttonSelectPath.UseVisualStyleBackColor = true;
            this.buttonSelectPath.Click += new System.EventHandler(this.buttonSelectPath_Click);
            // 
            // buttonSelectPathOK
            // 
            this.buttonSelectPathOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSelectPathOK.Location = new System.Drawing.Point(748, 30);
            this.buttonSelectPathOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelectPathOK.Name = "buttonSelectPathOK";
            this.buttonSelectPathOK.Size = new System.Drawing.Size(100, 29);
            this.buttonSelectPathOK.TabIndex = 3;
            this.buttonSelectPathOK.Text = "确定";
            this.buttonSelectPathOK.UseVisualStyleBackColor = true;
            this.buttonSelectPathOK.Click += new System.EventHandler(this.buttonSelectPathOK_Click);
            // 
            // buttonSelectPathCancel
            // 
            this.buttonSelectPathCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonSelectPathCancel.Location = new System.Drawing.Point(613, 30);
            this.buttonSelectPathCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelectPathCancel.Name = "buttonSelectPathCancel";
            this.buttonSelectPathCancel.Size = new System.Drawing.Size(100, 29);
            this.buttonSelectPathCancel.TabIndex = 4;
            this.buttonSelectPathCancel.Text = "取消";
            this.buttonSelectPathCancel.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(13, 98);
            this.listView1.Margin = new System.Windows.Forms.Padding(4);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(965, 153);
            this.listView1.TabIndex = 6;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "设备描述";
            this.columnHeader1.Width = 168;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "串口号";
            this.columnHeader2.Width = 106;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "波特率";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "设备地址";
            this.columnHeader4.Width = 139;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "设备类型";
            this.columnHeader5.Width = 148;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "超时";
            this.columnHeader6.Width = 76;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "传感器ID";
            this.columnHeader7.Width = 177;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonTest);
            this.groupBox2.Controls.Add(this.buttonStop);
            this.groupBox2.Controls.Add(this.buttonStart);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.numericUpDown1);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(21, 413);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(861, 66);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "控制";
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(645, 18);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(85, 39);
            this.buttonTest.TabIndex = 9;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(485, 18);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(4);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(123, 39);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "停止周期采集";
            this.buttonStop.UseVisualStyleBackColor = true;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(327, 20);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(4);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(129, 39);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "开始周期采集";
            this.buttonStart.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(235, 29);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "分钟";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(127, 24);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(100, 25);
            this.numericUpDown1.TabIndex = 8;
            this.numericUpDown1.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 29);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "采集周期：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelDecription);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.buttonModifyDeviceId);
            this.groupBox1.Controls.Add(this.textBoxSensorId);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.textBoxComPort);
            this.groupBox1.Controls.Add(this.textBoxDeviceId);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.labelPath);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Location = new System.Drawing.Point(21, 324);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(861, 81);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "调试";
            // 
            // labelDecription
            // 
            this.labelDecription.AutoSize = true;
            this.labelDecription.Location = new System.Drawing.Point(91, 24);
            this.labelDecription.Name = "labelDecription";
            this.labelDecription.Size = new System.Drawing.Size(0, 15);
            this.labelDecription.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 15);
            this.label6.TabIndex = 16;
            this.label6.Text = "已选设备：";
            // 
            // buttonModifyDeviceId
            // 
            this.buttonModifyDeviceId.Location = new System.Drawing.Point(708, 24);
            this.buttonModifyDeviceId.Name = "buttonModifyDeviceId";
            this.buttonModifyDeviceId.Size = new System.Drawing.Size(97, 29);
            this.buttonModifyDeviceId.TabIndex = 15;
            this.buttonModifyDeviceId.Text = "修改设备ID";
            this.buttonModifyDeviceId.UseVisualStyleBackColor = true;
            // 
            // textBoxSensorId
            // 
            this.textBoxSensorId.Location = new System.Drawing.Point(335, 49);
            this.textBoxSensorId.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSensorId.Name = "textBoxSensorId";
            this.textBoxSensorId.Size = new System.Drawing.Size(131, 25);
            this.textBoxSensorId.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(253, 54);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 15);
            this.label5.TabIndex = 13;
            this.label5.Text = "传感器ID：";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(508, 26);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 29);
            this.button1.TabIndex = 2;
            this.button1.Text = "单次测量";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // textBoxComPort
            // 
            this.textBoxComPort.Location = new System.Drawing.Point(77, 49);
            this.textBoxComPort.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxComPort.Name = "textBoxComPort";
            this.textBoxComPort.Size = new System.Drawing.Size(47, 25);
            this.textBoxComPort.TabIndex = 12;
            this.textBoxComPort.Text = "COM1";
            // 
            // textBoxDeviceId
            // 
            this.textBoxDeviceId.Location = new System.Drawing.Point(208, 49);
            this.textBoxDeviceId.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDeviceId.Name = "textBoxDeviceId";
            this.textBoxDeviceId.Size = new System.Drawing.Size(40, 25);
            this.textBoxDeviceId.TabIndex = 11;
            this.textBoxDeviceId.Text = "2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(132, 54);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "设备ID：";
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(124, 51);
            this.labelPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(0, 15);
            this.labelPath.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 54);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "串口号:";
            // 
            // FilePathSeclect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 557);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.buttonSelectPathCancel);
            this.Controls.Add(this.buttonSelectPathOK);
            this.Controls.Add(this.buttonSelectPath);
            this.Controls.Add(this.textBoxCsvFilePath);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FilePathSeclect";
            this.Text = "FilePathSeclect";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCsvFilePath;
        private System.Windows.Forms.Button buttonSelectPath;
        private System.Windows.Forms.Button buttonSelectPathOK;
        private System.Windows.Forms.Button buttonSelectPathCancel;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelDecription;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonModifyDeviceId;
        private System.Windows.Forms.TextBox textBoxSensorId;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBoxComPort;
        private System.Windows.Forms.TextBox textBoxDeviceId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonTest;
    }
}