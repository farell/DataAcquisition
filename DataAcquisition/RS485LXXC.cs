using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{
    class RS485LXXC : IMonitorDevice
    {
        private static readonly string[] availablePorts = SerialPort.GetPortNames();
        protected SerialPort comPort;
        private byte[] buffer;
        private string portName;
        private int baudrate;
        private int bytesRead;
        private int bytesTotall;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ManualResetEvent mre;
        private double temperature;
        private double humidity;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private const int frameLength = 9;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;

        public RS485LXXC(string portName, int baudrate, int timeout, string id, string sensorId)
        {
            comPort = new SerialPort(portName, baudrate, Parity.None, 8, StopBits.One);
            dataRecieveHandler = new SerialDataReceivedEventHandler(ComDataReceive);
            comPort.DataReceived += dataRecieveHandler;
            errorRecieveHandler = new SerialErrorReceivedEventHandler(ComErrorReceive);
            comPort.ErrorReceived += errorRecieveHandler;
            this.buffer = new byte[1024 * 4];
            this.portName = portName;
            this.baudrate = baudrate;
            this.sensorId = sensorId;
            this.bytesRead = 0;
            this.bytesTotall = 0;
        }

        private void ComErrorReceive(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        public void ClosePort()
        {
            comPort.DtrEnable = false;
            comPort.RtsEnable = false;
            comPort.ErrorReceived -= errorRecieveHandler;
            comPort.DataReceived -= dataRecieveHandler;
            Thread.Sleep(500);comPort.Close();
        }

        public void OpenPort()
        {
            comPort.DtrEnable = false;
            comPort.RtsEnable = false;
            comPort.ErrorReceived += errorRecieveHandler;
            comPort.DataReceived += dataRecieveHandler;
            comPort.Open();
        }

        private void ComDataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                bytesRead = this.comPort.Read(this.buffer, bytesTotall, 100);
                bytesTotall += bytesRead;
                if (bytesTotall >= frameLength)
                {
                    //this.ProcessData(this.buffer, bytesTotall);
                    this.bytesRead = 0;
                    this.bytesTotall = 0;
                }
            }
            catch (Exception ex)
            {
                this.bytesRead = 0;
                this.bytesTotall = 0;
                //using (StreamWriter sw = new StreamWriter(@"ErrLog\ErrLog.txt", true))
                //{
                //    sw.WriteLine(this.portName + " bytesToRead :" + bytesRead + "\n");
                //    sw.WriteLine(ex.ToString());
                //    sw.WriteLine("---------------------------------------------------------");
                //    sw.Close();
                //}
            }
        }

        private byte[] GetAcquisitionFrame()
        {
            //read all meter info eg. pressure,temperature etc.
            byte deviceId = Byte.Parse(this.deviceId);

            byte startAddress = 0x0a;
            ushort numOfPoints = 21;
            byte[] frame = ModbusUtility.GetReadHoldingRegisterFrame(deviceId, startAddress, numOfPoints);

            return frame;
        }

        private void SendFrame(byte[] buffer)
        {
            if (comPort.IsOpen)
            {
                comPort.Write(buffer, 0, buffer.Length);
            }
        }

        public bool Acquisit()
        {
            this.isSuccess = false;
            bool ret;

            if (!availablePorts.Contains<string>(this.portName))
            {
                this.errMsg = this.portName + " not exist";
                return false;
            }

            try
            {
                this.bytesRead = 0;
                this.bytesTotall = 0;
                //OpenPort();

                byte[] frame = GetAcquisitionFrame();
                this.SendFrame(frame);
                bool receivedEvent = this.mre.WaitOne(this.timeout * 1000);
                if (receivedEvent)
                {
                    if (isSuccess)
                    {
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                else
                {
                    log.Warn(this.sensorId + " Receiving Get Measure Reslut Response Timeout");
                    errMsg = "Receiving Get Measure Reslut Response Timeout";
                    ret = false;
                }
                mre.Reset();
                //ClosePort();
                return ret;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                errMsg = ex.Message;
                ret = false;
                return false;
            }


        }

        public string GetDeviceId()
        {
            return deviceId;
        }

        public void SetDeviceId(string newId)
        {
            deviceId = newId;
        }

        public Dictionary<string, double> GetResult()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add(sensorId + "-005", temperature);
            result.Add(sensorId + "-006", humidity);
            return result;
        }

        public string GetObjectType()
        {
            return "SerialRS_WS_N01_2x";
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                return GetObjectType() + " " + sensorId + " temperature: " + temperature + " humidity: " + humidity;
            }
            else
            {
                return GetObjectType() + " " + sensorId + " : " + errMsg;
            }
        }

        public string GetSensorId()
        {
            return sensorId;
        }

        public string GetErrorMsg()
        {
            return errMsg;
        }

        public string GetPortName()
        {
            return comPort.PortName;
        }
    }
}
