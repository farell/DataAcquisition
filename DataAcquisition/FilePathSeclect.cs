﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataAcquisition
{
    public partial class FilePathSeclect : Form
    {
        public FilePathSeclect()
        {
            InitializeComponent();
            buttonSelectPathOK.Enabled = false;
        }

        public string GetFilePath()
        {
            return textBoxCsvFilePath.Text;
        }

        private void buttonSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(fbd.SelectedPath))
                {
                    MessageBox.Show("请选择文件路径");
                    return;
                }
                textBoxCsvFilePath.Text = fbd.SelectedPath;
                buttonSelectPathOK.Enabled = true;
            }
        }

        private void buttonSelectPathOK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxCsvFilePath.Text))
            {
                MessageBox.Show("请选择文件路径");
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            RS485Micro_40A dev = new RS485Micro_40A("COM70", 9600, 10, textBoxDeviceId.Text, "1000000100112",13);
            bool result = dev.Acquisit();
            string msg = "";
            if (result)
            {
                msg = dev.GetResultString("");
            }
            else
            {
                msg = dev.GetErrorMsg();
            }
            MessageBox.Show(msg);
        }
    }
}
