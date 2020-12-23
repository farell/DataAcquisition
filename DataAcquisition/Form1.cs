using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataAcquisition
{
    public partial class Form1 : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
        private System.Timers.Timer timer;

        private ConnectionMultiplexer redis;

        private Dictionary<string, IMonitorDevice> deviceList;
        private List<Acquisitor> acquisitorCollection;
        public Form1()
        {
            InitializeComponent();
            this.isReallyExit = false;
            deviceList = new Dictionary<string, IMonitorDevice>();
            acquisitorCollection = new List<Acquisitor>();
            timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            string dbName = GetAppSetting("Sqlite3Name");
            string tableName = GetAppSetting("table");
            int redisIndex = int.Parse(GetAppSetting("RedisDbIndex"));
            string redisHost = GetAppSetting("RedisServerAddress");

            ConfigurationOptions option = new ConfigurationOptions() {
                EndPoints =
                            {
                                { redisHost, 6379 }
                            },
                AbortOnConnectFail = false
            };  
            
            redis = ConnectionMultiplexer.Connect(option);
            LoadConfig(dbName, tableName,redisIndex);
            StopAcquisit();
        }

        public static string GetAppSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                string value = ConfigurationManager.AppSettings[key];
                return value;
            }
            else
            {
                return null;
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                this.Invoke((EventHandler)(
                delegate
                {
                    textBox1.AppendText("正在采集数据，无法开始新的采集周期"+"\r\n");
                }));
            }
            else
            {
                //backgroundWorker1.RunWorkerAsync(numericUpDown1.Value);
            }
        }

        private void AddDevice()
        {
            //SerialPortDevice device;// = new SerialLaser("COM1", 9600, null, 10);
            //deviceList.Add(device);
           // device = new SerialInclination("COM1", 9600,"1", null, 10);
            //deviceList.Add(device);
            //device = new SerialMDL62XXAT("COM5", 9600, 10, "703057");
            //deviceList.Add(device);
            //device = new SerialMDL62XXAT("COM5", 9600, 10, "703055");
            //deviceList.Add(device);
            //device = new SerialMDL62XXAT("COM5", 9600, 10, "703044");
            //deviceList.Add(device);
            //device = new SerialMDL62XXAT("COM5", 9600, 10, "703060");
            //deviceList.Add(device);

            //device = new SerialJMWS1D("COM5", 9600, 10, "52308003");
            //deviceList.Add(device);
            //device = new SerialJMWS1D("COM5", 9600, 10, "52308004");
            //deviceList.Add(device);
            //device = new SerialJMWS1D("COM5", 9600, 10, "52308002");
            //deviceList.Add(device);
            //device = new SerialJMWS1D("COM5", 9600, 10, "52308007");
            //deviceList.Add(device);
            //device = new SerialJMWS1D("COM2", 9600, 10, "69");
            //deviceList.Add(device);
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

        //从数据库载入配置信息
        private void LoadConfig(string databaseFile,string table,int redisIndex)
        {
            string database = "Data Source = "+databaseFile;

            using (SQLiteConnection connection = new SQLiteConnection(database))
            {
                connection.Open();
                
                List<string> comPorts = new List<string>();

                string strainStatement = "SELECT DISTINCT ComPort from "+table;
                SQLiteCommand command1 = new SQLiteCommand(strainStatement, connection);
                using (SQLiteDataReader reader1 = command1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        string port = reader1.GetString(0);
                        comPorts.Add(port);
                    }
                }

                foreach (string port in comPorts)
                {
                    List<IMonitorDevice> devices = new List<IMonitorDevice>();
                    int period = 0;
                    int numOfRetry = 0;
                    strainStatement = "select DeviceId,ComPort,BaudeRate,TimeOut,DeviceType,SensorId,Description,Period,NumOfRetry,ChannelNo from "+table+" where ComPort ='" + port+"'";
                    command1 = new SQLiteCommand(strainStatement, connection);
                    using (SQLiteDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string deviceId = reader.GetString(0);
                            string comPort = reader.GetString(1);
                            string baudeRate = reader.GetString(2);
                            string timeOut = reader.GetString(3);
                            string deviceType = reader.GetString(4);
                            string description = reader.GetString(6);
                            string sensorId = reader.GetString(5);
                            period = reader.GetInt32(7);
                            numOfRetry = reader.GetInt32(8);
                            int ChannelNo = reader.GetInt32(9);
                            IMonitorDevice device = CreateDevice(comPort, Int32.Parse(baudeRate), Int32.Parse(timeOut), deviceId, sensorId, deviceType, ChannelNo);

                            if (device != null)
                            {
                                deviceList.Add(sensorId, device);
                                devices.Add(device);
                                string type = device.GetObjectType();
                                type = type.Remove(0, 5);
                                string[] item = { description,type, "", "", "", "" };
                                ListViewItem listItem = new ListViewItem(item);
                                listItem.Name = sensorId;
                                this.listView2.Items.Add(listItem);
                            }
                        }
                        this.listView2.Sort();
                    }

                    Acquisitor ss = new Acquisitor(port,devices, redis,period, numOfRetry,redisIndex,listView2,textBox1);
                    acquisitorCollection.Add(ss);
                }
                connection.Close();
            }
        }

        private IMonitorDevice CreateDevice(string port, int baudrate, int timeout, string deviceId, string sensorId, string deviceType,int channelNo)
        {
            IMonitorDevice device = null;
            switch (deviceType)
            {
                case "SerialRS_WS_N01_2x":
                    {
                        device = new RS485RS_WS_N01_2x(port, baudrate, timeout, deviceId, sensorId);
                        break;
                    }
                case "SerialACA826T":
                    {
                        device = new RS485ACA826T(port, baudrate, timeout, deviceId, sensorId);
                        break;
                    }
                case "SerialACT4238":
                    {
                        device = new RS485ACT4238(port, baudrate, timeout, deviceId, sensorId,channelNo);
                        break;
                    }
                case "SerialBGK3475DM":
                    {
                        device = new RS485BGK3475DM(port, baudrate, timeout, deviceId, sensorId);
                        break;
                    }
                case "SerialJMWS1D":
                    {
                        device = new RS485JMWS1D(port, baudrate, timeout, deviceId, sensorId);
                        break;
                    }
                case "SerialMDL62XXAT":
                    {
                        device = new RS485MDL62XXAT(port, baudrate, timeout, deviceId, sensorId);
                        break;
                    }
                case "SerialSankeDemo":
                    {
                        device = new RS485SankeDemo(port, baudrate, timeout, deviceId,sensorId);
                        break;
                    }
                case "SerialSKD100":
                    {
                        device = new RS485SKD100(port, baudrate, timeout, deviceId, sensorId);
                        break;
                    }
                case "SerialAS109":
                    {
                        device = new RS485AS109(port, baudrate, timeout, deviceId, sensorId);
                        break;
                    }
                case "RS485Micro_40A":
                    {
                        device = new RS485Micro_40A(port, baudrate, timeout, deviceId, sensorId,channelNo);
                        break;
                    }
                default: break;
            }
            return device;
        }

        private void SaveToDatabase(string sensorId, string type, double value, string stamp)
        {
            SQLiteConnection conn = null;
            SQLiteCommand cmd;

            string database = "Data Source = Laser.db";

            try
            {
                conn = new SQLiteConnection(database);
                conn.Open();

                cmd = conn.CreateCommand();

                {
                    cmd.CommandText = "insert into data values('" + sensorId + "','" + stamp + "','" + type + "'," + value.ToString() + ")";
                }

                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                conn.Close();
                //this.Invoke((EventHandler)(
                //    delegate {
                //        textBoxLog.AppendText(ex.ToString() + "\r\n");
                //    }));
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

        private void ToolStripMenuCsvPath_Click(object sender, EventArgs e)
        {
            FilePathSeclect fps = new FilePathSeclect();
            fps.ShowDialog(this);
            if(fps.DialogResult == DialogResult.OK)
            {
                string path = fps.GetFilePath();
                dataSourcePath = path;
                //labelPath.Text = path;
                //MessageBox.Show(path);
                // save path to sqlite database and update variable
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            StartAcquisit(); 
        }

        private void StartAcquisit()
        {
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            numericUpDown1.Enabled = false;
            ToolStripMenuItemTestDevice.Enabled = false;
            foreach(Acquisitor item in acquisitorCollection)
            {
                item.Start();
            }
        }

        private void StopAcquisit()
        {
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            numericUpDown1.Enabled = true;
            ToolStripMenuItemTestDevice.Enabled = true;
            foreach (Acquisitor item in acquisitorCollection)
            {
                item.Stop();
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopAcquisit();
        }

        private double Process4Bytes(byte[] buffer, int index)
        {
            byte sign = (byte)(buffer[index + 0] & 0x80);

            int main1 = (buffer[index + 0] & 0x7F) << 16;
            int main2 = buffer[index + 1] << 8;
            int main3 = buffer[index + 2];
            double digits = buffer[index + 3] / 1000.0;
            double value = main1 + main2 + main3 + digits;
            if(sign == 0x80)
            {
                value = -value;
            }
            return value;
        }

        private double Process8Bytes(byte[] buffer, int index)
        {
            int main1 = buffer[index + 0] << 40;
            int main2 = buffer[index + 1] << 32;
            int main3 = buffer[index + 2] << 24;
            int main4 = buffer[index + 3] << 16;
            int main5 = buffer[index + 4] << 8;
            int main6 = buffer[index+5];
            int digit1 = buffer[6] << 8;
            int digit2 = buffer[7];
            double digits = (digit1+digit2)/ 65536.0;
            return main1+main2+main3+main4+main5+main6 + digits;
        }

        public void UpdateDeviceId(string oldId,string newId,string comPort)
        {
            string database = "Data Source = config.db";
            using (SQLiteConnection connection = new SQLiteConnection(database))
            {
                connection.Open();

                string strainStatement = "update SensorInfo set DeviceId='" + newId + "' where ComPort='" + comPort + "' and DeviceId='" + oldId + "'";
                SQLiteCommand command = new SQLiteCommand(strainStatement, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private void ToolStripMenuItemTestDevice_Click(object sender, EventArgs e)
        {
            DeviceTest dt = new DeviceTest();
            dt.ShowDialog(this);
            if (dt.DialogResult == DialogResult.OK)
            {

            }
        }
    }
}
