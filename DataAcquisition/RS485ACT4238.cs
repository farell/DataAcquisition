using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAcquisition
{                                                          
    class RS485ACT4238: IMonitorDevice
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string[] availablePorts = SerialPort.GetPortNames();
        protected SerialPort comPort;
        private byte[] buffer;
        private string portName;
        private int baudrate;
        private int bytesRead;
        private int bytesTotall;

        private ManualResetEvent mre;
        private double temperature;
        private double frequency;
        private string errMsg;
        private bool isSuccess;
        private int timeout;
        private string deviceId;
        private string sensorId;
        private int channel;
        private Dictionary<string, double> result;
        private Dictionary<string, string> keyMap;
        private ACT4238_Data data;
        private int frameLength = 45;
        private SerialDataReceivedEventHandler dataRecieveHandler;
        private SerialErrorReceivedEventHandler errorRecieveHandler;
        public RS485ACT4238(string portName, int baudrate, int timeout, string deviceId, string sensorId,int channelNo)
        {
            comPort = new SerialPort(portName, baudrate, Parity.None, 8, StopBits.One);
            dataRecieveHandler = new SerialDataReceivedEventHandler(ComDataReceive);
            comPort.DataReceived += dataRecieveHandler;
            errorRecieveHandler = new SerialErrorReceivedEventHandler(ComErrorReceive);
            comPort.ErrorReceived += errorRecieveHandler;
            this.buffer = new byte[1024 * 4];
            this.portName = portName;
            this.baudrate = baudrate;
            this.bytesRead = 0;
            this.bytesTotall = 0;

            this.frequency = 0;
            this.temperature = 0;
            channel = channelNo - 1;
            if (channel < 0)
            {
                channel = 0;
            }
            this.deviceId = deviceId;
            this.sensorId = sensorId;
            this.timeout = timeout;
            this.mre = new ManualResetEvent(false);
            this.data = new ACT4238_Data();
            this.isSuccess = false;
            result = new Dictionary<string, double>();
            keyMap = new Dictionary<string, string>();
            keyMap.Add("005", "temperature: ");
            keyMap.Add("012", "frequency: ");
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
                    this.ProcessData(this.buffer, bytesTotall);
                    this.bytesRead = 0;
                    this.bytesTotall = 0;
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                log.Error(ex.Message);
                this.bytesRead = 0;
                this.bytesTotall = 0;
            }
        }

        private void ProcessData(byte[] buffer, int length)
        {
            this.isSuccess = false;

            bool checkPassed = this.StrainFrameCheck(buffer, length);
            if (checkPassed == true)
            {
                int startIndex = 3;

                string strainId = this.sensorId;

                string indexString = strainId.Substring(strainId.Length - 3, 3);
                string idHead = strainId.Substring(0, strainId.Length - 3);
                int index = Int32.Parse(indexString);

                result.Clear();

               // for (int i = 0; i < this.NumOfChannels; i++)
                {
                    string longId = strainId;// + "-"+(i+1).ToString();
                    //StrainChannel channel = this.strianChannels[longId];
                    //频率
                    frequency = CalculateFrequency(buffer, startIndex, channel);

                    //温度
                    temperature = CalculateTempreture(buffer, startIndex, channel);

                    result.Add(sensorId + "-012", frequency);
                    result.Add(sensorId + "-005", temperature);

                    this.data.SensorId = this.sensorId;
                    this.data.TimeStamp = "";
                    this.data.Frequency = frequency;
                    this.data.Temperature = temperature;
                }
                this.isSuccess = true;
            }
            else
            {
                this.errMsg = this.sensorId + "broken frame: " + ModbusUtility.BytesToHexString(buffer, length);
                log.Error(errMsg);
                //message = this.deviceId + "strain broken frame: " + CVT.ByteToHexStr(by) + "\r\n";
            }
            this.mre.Set();
        }

        private bool StrainFrameCheck(byte[] buffer, int length)
        {
            if (length != 45)
            {
                log.Error(this.sensorId + " frame check error:length ->"+length+" != 45");
                errMsg = this.sensorId + " frame check error:length->"+length+" != 45";
                return false;
            }
            byte[] crc = ModbusUtility.CalculateCrc(buffer, 43);
            if (crc[0] == buffer[43] && crc[1] == buffer[44])
            {
                return true;
            }
            else
            {
                log.Warn(this.sensorId + " frame check error: CRC check failed");
                errMsg = this.sensorId + " frame check error: CRC check failed";
                return false;
            }
        }

        private static double CalculateTempreture(byte[] buffer, int startIndex, int i)
        {
            //double temp = (buffer[startIndex + i * 5 + 3] * 256 + buffer[startIndex + i * 5 + 4]) / 10.0;
            byte sign = (byte)(buffer[startIndex + i * 5 + 3] & 0x80);
            byte higher = (byte)(buffer[startIndex + i * 5 + 3] & 0x7F);
            byte lower = buffer[startIndex + i * 5 + 4];
            double temp = (higher * 256 + lower) / 10.0;
            if (sign == 0x80)
            {
                temp = -temp;
            }
            return Math.Round(temp, 3);
        }

        private static double CalculateFrequency(byte[] buffer, int startIndex, int i)
        {
            double frequency = (buffer[startIndex + i * 5 + 0] * 256 * 256 + buffer[startIndex + i * 5 + 1] * 256 + buffer[startIndex + i * 5 + 2]) / 100.0;

            return Math.Round(frequency, 3);
        }

        private byte[] GetAcquisitionFrame()
        {
            byte deviceId = Byte.Parse(this.deviceId);
            byte startAddress = 0;
            ushort numOfPoints = 16;
            byte[] frame = ModbusUtility.GetReadHoldingRegisterFrame(deviceId, startAddress, numOfPoints);
            return frame;
        }

        private bool RtuFrameCheck(byte[] buffer, int length)
        {
            byte[] crc = ModbusUtility.CalculateCrc(buffer, length - 2);
            if (crc[0] == buffer[length - 2] && crc[1] == buffer[length - 1])
            {
                return true;
            }
            else
            {
                errMsg = this.sensorId + " frame check error";
                return false;
            }
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
                log.Error(errMsg);
                return false;
            }
            if (comPort.IsOpen)
            {
                this.errMsg = this.portName + " is open by another process";
                log.Error(errMsg);
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
                        //ar = new AcquisitionResult(true, "", result, 0);
                    }
                    else
                    {
                        ret = false;
                        //ar = new AcquisitionResult(false, errMsg, result, 0);
                    }
                }
                else
                {
                    log.Warn(this.sensorId + " Response Timeout");
                    errMsg = "Response Timeout";
                    ret = false;
                }
                mre.Reset();
                //ClosePort();
                
            }
            catch (Exception ex)
            {
                mre.Reset();
                //ClosePort();
                errMsg = ex.Message;
                log.Error(errMsg);
                return false;
            }
            return ret;
        }

        public string GetDeviceId()
        {
            return deviceId;
        }

        public string GetErrorMsg()
        {
            return errMsg;
        }

        public string GetObjectType()
        {
            return "RS485ACT4238";
        }

        public Dictionary<string, double> GetResult()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add(sensorId + "-005", temperature);
            result.Add(sensorId + "-012", frequency);
            return result;
        }

        public string GetResultString(string stamp)
        {
            if (isSuccess)
            {
                this.data.TimeStamp = stamp;
                string result = JsonConvert.SerializeObject(this.data);
                return result;
                //return GetObjectType() + " " + sensorId + " temperature: " + temperature + " frequency: " + frequency;
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

        public void SetDeviceId(string newId)
        {
            deviceId = newId;
        }

        public string GetPortName()
        {
            return comPort.PortName;
        }
    }
}
