using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;

namespace DataAcquisition
{
    class Acquisitor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //private List<SerialPortDevice> devices;
        private List<IMonitorDevice> devices;
        private BackgroundWorker backgroundWorker;
        private System.Timers.Timer timer;
        private IDatabase db;
        private ConnectionMultiplexer redis;
        private int retryTimes;
        private string comPort;
        private ListView listView;
        private TextBox logTextBox;
        public Acquisitor(string port,List<IMonitorDevice> deviceList, ConnectionMultiplexer client,int period,int retry,int redisDbIndex,ListView view,TextBox logText)
        {
            listView = view;
            logTextBox = logText;
            devices = deviceList;
            comPort = port;
            db = client.GetDatabase(redisDbIndex);
            redis = client;
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            timer = new System.Timers.Timer(1000*period);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            this.retryTimes = retry;
        }

        public void Start()
        {
            //log.Debug(comPort + " Acquisitor Start()");
            timer.Start();
            if (!backgroundWorker.IsBusy)
            {
                //log.Debug(comPort + " Start() Start Acquisitor Worker");
                try
                {
                    backgroundWorker.RunWorkerAsync();
                }
                catch(Exception ex)
                {
                    log.Error(comPort + " Start() RunWorkerAsync() failed "+ex.Message);
                    UpdateLogTextBox(comPort + " Start() RunWorkerAsync() failed " + ex.Message);
                }
                
            }
            else
            {
                UpdateLogTextBox(comPort + " Acquisitor Worker is busy");
                log.Warn(comPort + " Acquisitor Worker is busy");
            }
        }

        public void Stop()
        {
            //log.Debug(comPort + " Acquisitor Stop() ");
            timer.Stop();
            if (backgroundWorker.IsBusy)
            {
                //log.Warn(comPort + " Stop() Stop Acquisitor Worker");
                backgroundWorker.CancelAsync();
            }
            else
            {
                UpdateLogTextBox(comPort + " aquisition worker CancellationPending finished!");
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //log.Debug(comPort + "timer elapsed!");
            if (backgroundWorker.IsBusy)
            {
                log.Warn(comPort + " Acquisitor Worker is busy,new cycle canceled");
                UpdateLogTextBox(comPort + " Acquisitor Worker is busy,new cycle canceled");
            }
            else
            {
                //log.Debug(comPort + " Timer Start Acquisitor Worker");
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void UpdateLogTextBox(string content)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.BeginInvoke(new MethodInvoker(() =>
                {
                    logTextBox.AppendText(content + "\r\n");
                }));
            }
        }

        private void UpdateListView(bool success,string key,string stamp,Dictionary<string,double> vals,string errStr)
        {
            if (listView.InvokeRequired)
            {
                listView.BeginInvoke(new MethodInvoker(() =>
                {
                    
                    if (listView.Items.ContainsKey(key))
                    {
                        ListViewItem item = listView.Items[key];

                        if (success)
                        {
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
                            item.SubItems[5].ForeColor = Color.Green;
                            listView.Invalidate();
                        }
                        else
                        {
                            item.SubItems[5].Text = errStr;
                            item.SubItems[5].BackColor = Color.Red;
                            listView.Invalidate();
                        }

                        item.SubItems[2].Text = stamp;
                    }
                }));
            }
        }

        private string GetDataValues()
        {
            IDatabase db = this.redis.GetDatabase(0);
            string[] ks = { "","",""};//list.Keys.ToArray<string>();

            RedisKey[] keys = ks.Select(key => (RedisKey)key).ToArray();

            RedisValue[] vals = db.StringGet(keys);
            RedisValuesToDataValues(vals);
            return null;
        }

        private DataValue[] RedisValuesToDataValues(RedisValue[] vals)
        {
            List<DataValue> dv_list = new List<DataValue>();
            foreach (RedisValue rv in vals)
            {
                if (!rv.IsNull)
                {
                    DataValue dv = JsonConvert.DeserializeObject<DataValue>((string)rv);
                    dv_list.Add(dv);
                }
                else
                {
                    //error log
                }
            }
            return dv_list.ToArray();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            try
            {
                string stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Dictionary<RedisKey, RedisValue> pair = new Dictionary<RedisKey, RedisValue>();
                List<DataValue> dvList = new List<DataValue>();
                foreach (IMonitorDevice spd in devices)
                {
                    spd.OpenPort();
                    bool acquisitSuccess = false;
                    for (int i = 0; i < retryTimes; i++)
                    {
                        bool isSuccess = spd.Acquisit();
                        if (isSuccess)
                        {
                            acquisitSuccess = true;
                            string result = spd.GetResultString(stamp);
                            string key = spd.GetSensorId();
                            pair[key] = result;
                            UpdateListView(acquisitSuccess, spd.GetSensorId(), stamp, spd.GetResult(), spd.GetErrorMsg());
                            break;
                        }

                        if (bgWorker.CancellationPending == true)
                        {
                            log.Warn(comPort+" aquisition worker CancellationPending finished!");
                            UpdateLogTextBox(comPort + " aquisition worker CancellationPending finished!");
                            e.Cancel = true;
                            return;
                        }
                        Thread.Sleep(2000);
                    }
                    if (!acquisitSuccess)
                    {
                        UpdateListView(acquisitSuccess, spd.GetSensorId(), stamp, spd.GetResult(), spd.GetErrorMsg());
                        UpdateLogTextBox(stamp + ": " + comPort + " " + spd.GetSensorId() + " " + spd.GetObjectType() + " Error: " + spd.GetErrorMsg());
                    }
                }
                if (redis.IsConnected)
                {
                    if (pair.Count > 0)
                    {
                        db.StringSet(pair.ToArray());
                    }
                    if (dvList.Count > 0)
                    {
                        //string dvPacket = JsonConvert.SerializeObject(dvList);
                        //save to queue
                    }
                }
                else
                {
                    log.Warn("Redis is down");
                    UpdateLogTextBox(stamp + ": " + redis.GetStatus());
                }
            }catch(Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
