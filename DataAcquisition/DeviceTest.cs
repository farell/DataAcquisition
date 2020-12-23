using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataAcquisition
{
    public partial class DeviceTest : Form
    {
        private Dictionary<string, IMonitorDevice> deviceList;
        public DeviceTest()
        {
            InitializeComponent();
            deviceList = new Dictionary<string, IMonitorDevice>();
            LoadConfig();
            treeView1.Nodes[0].Expand();
            checkBoxEnableModify.Checked = false;
            groupBox1.Enabled = false;
        }

        //从数据库载入配置信息
        private void LoadConfig()
        {
            string database = "Data Source = config.db";

            using (SQLiteConnection connection = new SQLiteConnection(database))
            {
                connection.Open();

                Dictionary<int, string> bridge = new Dictionary<int, string>();
                string sqlStatement = "SELECT BridgeId,Description from BridgeInfo";
                SQLiteCommand command = new SQLiteCommand(sqlStatement, connection);
                using (SQLiteDataReader reader1 = command.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        int id = reader1.GetInt32(0);
                        string desc = reader1.GetString(1);
                        bridge.Add(id, desc);
                    }
                }
                int roadIndex = 0;
                int bridgeIndex = 1;
                int settlementIndex = 3;
                int strainIndex = 2;
                int inclinationIndex = 4;
                int temperatureIndex = 5;
                int distanceIndex = 6;
                TreeNode root = treeView1.Nodes.Add("root", "", roadIndex, roadIndex);
                foreach (KeyValuePair<int, string> kvp in bridge)
                {
                    TreeNode subRoot = root.Nodes.Add(kvp.Value, kvp.Value, bridgeIndex, bridgeIndex);

                    string sqlStatement0 = "SELECT SensorId,Description,DeviceType from SensorInfo where BridgeId=" + kvp.Key.ToString();
                    SQLiteCommand command0 = new SQLiteCommand(sqlStatement0, connection);
                    using (SQLiteDataReader reader1 = command0.ExecuteReader())
                    {
                        while (reader1.Read())
                        {
                            string sensorId = reader1.GetString(0);
                            //string sensorId = reader1.GetString(0);
                            string desc = reader1.GetString(1);
                            string deviceType = reader1.GetString(2);
                            int imgIndex = 0;
                            switch (deviceType)
                            {
                                case "SerialACA826T":
                                    imgIndex = inclinationIndex;
                                    break;
                                case "SerialBGK3475DM":
                                    imgIndex = settlementIndex;
                                    break;
                                case "SerialMDL62XXAT":
                                    imgIndex = settlementIndex;
                                    break;
                                case "SerialRS_WS_N01_2x":
                                    imgIndex = temperatureIndex;
                                    break;
                                case "SerialJMWS1D":
                                    imgIndex = temperatureIndex;
                                    break;
                                case "SerialAS109":
                                    imgIndex = temperatureIndex;
                                    break;
                                case "SerialSKD100":
                                    imgIndex = distanceIndex;
                                    break;
                                case "SerialSankeDemo":
                                    imgIndex = distanceIndex;
                                    break;
                                case "SerialACT4238":
                                    imgIndex = strainIndex;
                                    break;
                                default:
                                    imgIndex = strainIndex;
                                    break;

                            }
                            subRoot.Nodes.Add(sensorId, desc, imgIndex, imgIndex);
                        }
                    }
                }

                string sqlStatement1 = "select DeviceId,ComPort,BaudeRate,TimeOut,DeviceType,SensorId,Description,Period,NumOfRetry,ChannelNo from SensorInfo ";
                SQLiteCommand command1 = new SQLiteCommand(sqlStatement1, connection);
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
                        int period = reader.GetInt32(7);
                        int numOfRetry = reader.GetInt32(8);
                        int ChannelNo = reader.GetInt32(9);
                        IMonitorDevice device = CreateDevice(comPort, Int32.Parse(baudeRate), Int32.Parse(timeOut), deviceId, sensorId, deviceType, ChannelNo);

                        if (device != null)
                        {
                            deviceList.Add(sensorId, device);
                        }
                    }
                }
                connection.Close();
            }
        }

        private IMonitorDevice CreateDevice(string port, int baudrate, int timeout, string deviceId, string sensorId, string deviceType, int channelNo)
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
                        device = new RS485SankeDemo(port, baudrate, timeout,deviceId, sensorId);
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
                        device = new RS485Micro_40A(port, baudrate, timeout, deviceId, sensorId, channelNo);
                        break;
                    }
                default: break;
            }
            return device;
        }

        public void UpdateDeviceId(string oldId, string newId,string sensorId)
        {
            string database = "Data Source = config.db";
            using (SQLiteConnection connection = new SQLiteConnection(database))
            {
                connection.Open();

                //string strainStatement = "update SensorInfo set DeviceId='" + newId + "' where ComPort='" + comPort + "' and DeviceId='" + oldId + "'";
                string strainStatement = "update SensorInfo set DeviceId='" + newId + "' where SensorId='" + sensorId + "' and DeviceId='" + oldId + "'";
                SQLiteCommand command = new SQLiteCommand(strainStatement, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //要求父节点被勾选，则子节点全部被勾选；父节点不被勾选，则子节点不全不被勾选
            if (e.Node.Checked == true)
            {
                if (e.Action != TreeViewAction.Unknown)
                {
                    if (e.Node.GetNodeCount(true) == 0)
                    {
                        //MessageBox.Show(e.Node.Name);

                        if (!this.listView2.Items.ContainsKey(e.Node.Name))
                        {
                            string type = deviceList[e.Node.Name].GetObjectType();
                            type = type.Remove(0, 5);
                            string[] item = { e.Node.Text,type, "", "", "", "" };

                            ListViewItem listItem = new ListViewItem(item);
                            listItem.Name = e.Node.Name;
                            this.listView2.Items.Add(listItem);
                        }

                    }

                    cycleChild(e.Node, true);
                }
                if (e.Node.Parent != null)
                {
                    if (nextCheck(e.Node))
                    {
                        cycleParent(e.Node, true);
                    }
                    else
                    {
                        cycleParent(e.Node, false);
                    }
                }
            }

            if (e.Node.Checked == false)
            {
                if (e.Action != TreeViewAction.Unknown)
                {
                    if (e.Node.GetNodeCount(true) == 0)
                    {
                        //MessageBox.Show(e.Node.Name);
                        listView2.Items.RemoveByKey(e.Node.Name);
                    }
                    cycleChild(e.Node, false);  //中间节点不选中则子节点全部不选中
                    cycleParent(e.Node, false);       //父节点不选中
                }
                //bCheck = false;
            }
            return;
        }

        private void buttonTestSelected_Click(object sender, EventArgs e)
        {
            buttonTestSelected.Enabled = false;
            foreach (ListViewItem item in listView2.Items)
            {
                string key = item.Name;
                IMonitorDevice spd = deviceList[key];
                bool result = spd.Acquisit();
                if (!result)
                {
                    item.SubItems[5].Text = spd.GetErrorMsg();
                    item.SubItems[5].BackColor = Color.Red;
                }
                else
                {
                    Dictionary<string, double> vals = spd.GetResult();
                    List<double> vd = new List<double>();
                    foreach (string k in vals.Keys)
                    {
                        vd.Add(vals[k]);
                    }
                    if (vd.Count > 1)
                    {
                        item.SubItems[3].Text = vd[0].ToString();
                        item.SubItems[4].Text = vd[1].ToString();
                    }
                    else
                    {
                        item.SubItems[3].Text = vd[0].ToString();
                    }
                    item.SubItems[5].Text = "成功";
                    item.SubItems[5].BackColor = Color.Green;
                }
                item.SubItems[2].Text = DateTime.Now.ToString();
            }
            buttonTestSelected.Enabled = true;
        }

        #region check选择事件

        private bool nextCheck(TreeNode n)   //判断同级的节点是否全选
        {
            foreach (TreeNode tn in n.Parent.Nodes)
            {
                if (tn.Checked == false) return false;
            }
            return true;
        }

        private bool nextNotCheck(TreeNode n)  //判断同级的节点是否全不选
        {
            if (n.Checked == true)
            {
                return false;
            }
            if (n.NextNode == null)
            {
                return true;
            }

            return this.nextNotCheck(n.NextNode);
        }

        private void cycleChild(TreeNode tn, bool check)    //遍历节点下的子节点
        {
            if (tn.Nodes.Count != 0)
            {
                foreach (TreeNode child in tn.Nodes)
                {
                    child.Checked = check;
                    if (check)
                    {
                        if (child.Nodes.Count == 0)
                        {
                            //MessageBox.Show(e.Node.Name);

                            if (!this.listView2.Items.ContainsKey(child.Name))
                            {
                                string type = deviceList[child.Name].GetObjectType();
                                type = type.Remove(0, 5);
                                string[] item = { child.Text,type, "", "", "", "" };

                                ListViewItem listItem = new ListViewItem(item);
                                listItem.Name = child.Name;
                                this.listView2.Items.Add(listItem);
                            }
                        }
                    }
                    else
                    {
                        if (child.Nodes.Count == 0)
                        {
                            //MessageBox.Show(e.Node.Name);
                            listView2.Items.RemoveByKey(child.Name);
                        }
                    }
                    //MessageBox.Show(child.Name);
                    if (child.Nodes.Count != 0)
                    {
                        cycleChild(child, check);
                    }
                }
            }
            else
                return;
        }

        private void cycleParent(TreeNode tn, bool check)    //遍历节点上的父节点
        {
            if (tn.Parent != null)
            {
                if (nextCheck(tn))
                {
                    tn.Parent.Checked = true;
                }
                else
                {
                    tn.Parent.Checked = false;
                }
                cycleParent(tn.Parent, check);
            }
            return;
        }
        #endregion

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.GetNodeCount(true) == 0)
            {
                //MessageBox.Show(e.Node.Name);

                string key = e.Node.Name;
                if (deviceList.ContainsKey(key))
                {
                    //SerialPortDevice spd = deviceList[key];
                    IMonitorDevice spd = deviceList[key];
                    string deviceId = spd.GetDeviceId();
                    string sensorId = spd.GetSensorId();
                    string portName = spd.GetPortName();
                    textBoxDeviceId.Text = deviceId;
                    textBoxSensorId.Text = sensorId;
                }
            }
        }

        private void buttonModifyDeviceId_Click(object sender, EventArgs e)
        {
            
            string id = textBoxDeviceId.Text;
            string sensorId = textBoxSensorId.Text;

            if (String.IsNullOrEmpty(sensorId))
            {
                MessageBox.Show("传感器ID错误");
                return;
            }

            IMonitorDevice device = deviceList[sensorId];
            if (device != null)
            {
                string oldId = device.GetDeviceId();
                
                device.SetDeviceId(id);
                UpdateDeviceId(oldId, id, sensorId);
                MessageBox.Show("修改成功！ 新 DeviceId 为： "+device.GetDeviceId()+" 周期采集软件重启后生效");
            }
            else
            {
                MessageBox.Show("设备不存在");
            }
            
        }

        private void checkBoxEnableModify_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableModify.Checked)
            {
                groupBox1.Enabled = true;
            }
            else
            {
                groupBox1.Enabled = false;
            }
        }
    }
}
