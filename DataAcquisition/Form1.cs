using HMMS.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using YNHD.Common;

namespace DataAcquisition
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 是否退出，控制最小化托盘balloon 提示
        /// </summary>
        private bool isReallyExit;
        /// <summary>
        /// csv文件所在路径
        /// </summary>
        private string dataSourcePath;
        /// <summary>
        /// 上次采集的时间
        /// </summary>
        private DateTime lastAquiredTime;

        private List<SerialPortDevice> deviceList;
        public Form1()
        {
            InitializeComponent();
            this.isReallyExit = false;
            deviceList = new List<SerialPortDevice>();

            SerialPortDevice device = new SerialLaser("COM1", 9600, null, 10);
            deviceList.Add(device);
            device = new SerialInclination("COM1", 9600, null, 10);
            deviceList.Add(device);
            device = new SerialKingMarSettlement("COM5", 9600, null, 10, "703057");
            deviceList.Add(device);
            device = new SerialKingMarSettlement("COM5", 9600, null, 10, "703055");
            deviceList.Add(device);
            device = new SerialKingMarSettlement("COM5", 9600, null, 10, "703044");
            deviceList.Add(device);
            device = new SerialKingMarSettlement("COM5", 9600, null, 10, "703060");
            deviceList.Add(device);

            device = new SerialKingmarchTemperature("COM5", 9600, null, 10, "52308003");
            deviceList.Add(device);
            device = new SerialKingmarchTemperature("COM5", 9600, null, 10, "52308004");
            deviceList.Add(device);
            device = new SerialKingmarchTemperature("COM5", 9600, null, 10, "52308002");
            deviceList.Add(device);
            device = new SerialKingmarchTemperature("COM5", 9600, null, 10, "52308007");
            deviceList.Add(device);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.notifyIcon1.Visible = true;
                this.Hide();
                if (!this.isReallyExit)
                {
                    this.notifyIcon1.ShowBalloonTip(2000, "提示", "隐藏在任务栏！", ToolTipIcon.Info);
                }

                return;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                this.WindowState = FormWindowState.Minimized;
                this.notifyIcon1.Visible = true;
                this.Hide();
            }
            else
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = true;
            this.Activate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确定要退出？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                this.isReallyExit = true;
                this.notifyIcon1.Visible = false;
                this.Close();
                //this.Dispose();
                System.Environment.Exit(System.Environment.ExitCode);
            }
        }

        private string SaveToDatabase(List<Modle> list)
        {
            string postData = JsonConvert.SerializeObject(list);
            //string postData = "[{\"SensorID\":\"123\",\"StampTime\":\"2017-11-12 20:20:20\",\"VauleType\":\"1\",\"Values\":\"789\"},{\"SensorID\":\"123\",\"StampTime\":\"2017-11-12 20:20:55\",\"VauleType\":\"1\",\"Values\":\"789.001\"}]";

            string url = "http://112.112.16.154:1819/ShaBi/DaShaBi";
            HttpClient1 client = new HttpClient1(url);
            client.PostingData.Add("name", "sjtb");
            client.PostingData.Add("pwd", "zmwc");
            client.PostingData.Add("listStr", postData);
            string jsonString = client.GetString().Replace("\\\"", "\"");
            return jsonString;
        }

        private void ToolStripMenuCsvPath_Click(object sender, EventArgs e)
        {
            FilePathSeclect fps = new FilePathSeclect();
            fps.ShowDialog(this);
            if(fps.DialogResult == DialogResult.OK)
            {
                string path = fps.GetFilePath();
                dataSourcePath = path;
                labelPath.Text = path;
                //MessageBox.Show(path);
                // save path to sqlite database and update variable
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            //SerialPortDevice device = new SerialLaser("COM1",9600,null,10);
            //AcquisitionResult ar = device.Acquisit();

            //if (ar.IsSuccess)
            //{
            //    MessageBox.Show("Result: " + ar.Result1);
            //}
            //else
            //{
            //    MessageBox.Show(ar.ErrMsg);
            //}
            //groupBox1.Enabled = false;
            lastAquiredTime = dateTimePicker1.Value;
            //MessageBox.Show(dateTimePicker1.Value.ToString());
            backgroundWorker1.RunWorkerAsync(lastAquiredTime);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            //SerialPortDevice device = new SerialInclination("COM1", 9600, null, 10);
            //AcquisitionResult ar = device.Acquisit();

            //if (ar.IsSuccess)
            //{
            //    MessageBox.Show("Result1: " + ar.Result1+ " Result2: " + ar.Result2);
            //}
            //else
            //{
            //    MessageBox.Show(ar.ErrMsg);
            //}
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            DateTime lastTime = (DateTime)e.Argument;

            while (true)
            {
                foreach (SerialPortDevice spd in deviceList)
                {
                    AcquisitionResult ar = spd.Acquisit();
                
                    string res = "";

                    string objectType = spd.GetObjectType();

                    if (ar.IsSuccess)
                    {
                        //string objectType = spd.GetObjectType();

                        switch (objectType)
                        {
                            case "SerialInclination": {
                                    res = objectType+" Result1: " + ar.Result1 + " Result2: " + ar.Result2 + "\r\n";
                                    break;
                                };
                            case "SerialLaser": {
                                    res = objectType + " Result1: " + ar.Result1 + "\r\n";
                                    break;
                                };
                            case "SerialKingMarSettlement": {
                                    res = objectType + " Result1: " + ar.Result1 + "\r\n";
                                    break;
                                };
                            case "SerialKingmarchTemperature":
                                {
                                    res = objectType + " Result1: " + ar.Result1 + " Result2: " + ar.Result2 + "\r\n";
                                    break;
                                };
                            default: { break; };
                        }
                        //res = "Result1: " + ar.Result1 + " Result2: " + ar.Result2 + "\r\n";
                        // MessageBox.Show("Result: " + ar.Result1);
                    }
                    else
                    {
                        res = objectType + " "+ar.ErrMsg + "\r\n";
                        //MessageBox.Show(ar.ErrMsg);
                    }
                    this.Invoke((EventHandler)(
                        delegate
                        {
                            this.textBox1.AppendText(res);
                        }));
                    Thread.Sleep(2000);
                    if (bgWorker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                Thread.Sleep(5000);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBox1.AppendText("取消周期采集");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string id = textBox2.Text;
            SerialPortDevice device = new SerialKingMarSettlement("COM5", 9600, null, 10,id);
            //SerialPortDevice device = new SerialKingmarchTemperature("COM5", 9600, null, 10, id);
            AcquisitionResult ar = device.Acquisit();

            if (ar.IsSuccess)
            {
                //MessageBox.Show("Result1: " + ar.Result1+ "  Result2: " + ar.Result2+"\r\n");
                MessageBox.Show("Result: " + ar.Result1);
            }
            else
            {
                MessageBox.Show(ar.ErrMsg);
            }
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    List<Modle> list = new List<Modle>();
        //    Modle m = new Modle();
        //    m.SensorID = "1";
        //    m.StampTime = "2017-11-18 15:23:21";
        //    m.VauleType = "1";
        //    m.Values = "2";
        //    list.Add(m);
        //    list.Add(m);

        //    string jsonString = SaveToDatabase(list);
        //    MessageBox.Show(jsonString);
        //}
    }
}
