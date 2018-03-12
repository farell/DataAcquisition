using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataAcquisition
{
    public class CsvAcquisitorHongHe : Acquisitor
    {
        private Dictionary<string, SensorInitVal> initValue;
        private string stampFileName;
        private string initFileName;
        private DateTime dateTime;
        private Dictionary<string, MidValueFilter> myDictionary;

        public CsvAcquisitorHongHe(string initFile)
        {
            this.initFileName = initFile;
            myDictionary = new Dictionary<string, MidValueFilter>();
            this.initValue = LoadSensorInit(initFile);
        }

        private Dictionary<string, SensorInitVal> LoadSensorInit(string fileName)
        {
            Dictionary<string, SensorInitVal> map = new Dictionary<string, SensorInitVal>();
            try
            {
                StreamReader sr = new StreamReader(fileName);

                string data = sr.ReadToEnd();
                Console.WriteLine(data);
                string[] objs = data.Split(';');
                foreach (string i in objs)
                {
                    SensorInitVal siv = JsonConvert.DeserializeObject<SensorInitVal>(i);

                    map.Add(siv.SENSOR_ID, siv);
                }
                Console.WriteLine("map.Count: " + map.Count);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
                //return DateTime.Now;
            }
            return map;
        }

        private DateTime LoadStamp(string fileName)
        {
            try
            {
                StreamReader sr = new StreamReader(fileName);
                string dateString = sr.ReadLine();
                DateTime dt = DateTime.Parse(dateString);
                return dt;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
                return DateTime.Now;
            }

        }

        private void PrintDataValue(DataValue dv)
        {
            Console.WriteLine("--------------------------");
            Console.WriteLine("SensorId: " + dv.SensorId);
            Console.WriteLine("TimeStamp: " + dv.TimeStamp);
            Console.WriteLine("ValueType: " + dv.ValueType);
            Console.WriteLine("Value: " + dv.Value);
        }

        private void GetInclination(string fileName, ref Queue<DataValue> dataQueue)
        {
            uint count = 0;
            uint totalRecords = 0;

            string SelectedBridge = "HongHe";
            try
            {
                StreamReader sr = new StreamReader(fileName);
                //StreamReader sr = new StreamReader("F:\\宝龙\\2-24\\预处理\\飞龙大桥2014-2015.csv");
                //string dateString = sr.ReadLine();
                //DateTime dt = DateTime.Parse(dateString);

                string[] type = null;
                string[] property = null;
                string[] id = null;
                string date = null;

                string line = sr.ReadLine();
                while (line != null)
                {
                    count++;
                    if (count == 2)//date,,,,,,,,,,,
                    {
                        date = line.Split(',')[1];
                        Console.WriteLine("date:" + date);
                    }

                    if (count == 7)//AVE,AVE,MIN,,,,,,,,,,,
                    {
                        type = line.Split(',');
                        id = new string[type.Length];
                    }

                    if (count == 9)//T1X (xxx),T2X (xxx),
                    {
                        property = line.Split(',');
                        for (int i = 0; i < property.Length; i++)
                        {
                            property[i] = property[i].Split('(')[0].Trim();

                            //type[i] = property[i] + "-" + type[i];

                            if (property[i].Length > 2)
                            {
                                id[i] = property[i].Substring(0, 2);
                            }
                            else
                            {
                                id[i] = property[i];
                            }

                            Console.Write(property[i] + ",");
                        }
                        Console.WriteLine("");
                        for (int i = 0; i < property.Length; i++)
                        {
                            Console.Write(type[i] + ",");
                        }
                        Console.WriteLine("");
                        for (int i = 0; i < property.Length; i++)
                        {
                            //id[i] = property[i].Substring(0, 2);
                            Console.Write(id[i] + ",");
                        }
                    }
                    //忽略文件信息
                    if (count < 10)
                    {
                        //Console.WriteLine(line);
                        line = sr.ReadLine();
                        continue;
                    }

                    string[] datas = line.Split(',');

                    string cur_date = null;
                    for (int i = 0; i < type.Length; i++)
                    {
                        if (i == 0)
                        {
                            cur_date = date + " " + datas[0];
                            //date = datas[0];
                            continue;
                        }
                        totalRecords++;
                        string longId = this.initValue[id[i]].Parameter2;
                        DataValue dv = new DataValue();
                        dv.SensorId = longId;
                        dv.TimeStamp = cur_date;
                        dv.Value = datas[i];
                        dv.ValueType = property[i] + "-" + type[i];

                        dataQueue.Enqueue(dv);

                        if (id[i] == "T1" || id[i] == "T3" || id[i] == "T4" || id[i] == "T6")
                        {
                            totalRecords++;
                            double def = Double.Parse(datas[i]);
                            double coefficent = 0;
                            if (SelectedBridge == "FeiLong")
                            {
                                //std::cout << " FeiLong " << std::endl;
                                if (id[i] == "T1")
                                {
                                    coefficent = 77 * 1000 / 4;
                                }
                                if (id[i] == "T3")
                                {
                                    coefficent = 140 * 1000 / 2;
                                }
                                if (id[i] == "T4")
                                {
                                    coefficent = 140 * 1000 / 2;
                                }
                                if (id[i] == "T6")
                                {
                                    coefficent = 77 * 1000 / 4;
                                }
                            }
                            else
                            {//SelectedBridge == "HongHe"
                             //std::cout << " HongHe " << std::endl;
                                if (id[i] == "T1")
                                {
                                    coefficent = 182 * 1000 / 4;
                                }
                                if (id[i] == "T3")
                                {
                                    coefficent = 265 * 1000 / 2;
                                }
                                if (id[i] == "T4")
                                {
                                    coefficent = 265 * 1000 / 2;
                                }
                                if (id[i] == "T6")
                                {
                                    coefficent = 194 * 1000 / 4;
                                }
                            }

                            double init = Double.Parse(initValue[id[i]].Parameter1);
                            double deflection = Math.Tan(def * 0.017453293) * coefficent - init;

                            DataValue dv2 = new DataValue();
                            dv2.ValueType = "deflection-" + type[i];
                            dv2.SensorId = longId;
                            dv2.TimeStamp = cur_date;
                            dv2.Value = deflection.ToString();

                            dataQueue.Enqueue(dv2);
                            /*
                            Console.WriteLine("date: " + date);
                            Console.WriteLine("id: " + id[i]);
                            Console.WriteLine("type: " + type[i]);
                            Console.WriteLine("init: " + init);
                            Console.WriteLine("coefficent: " + coefficent);
                            Console.WriteLine("def: " + def);
                            Console.WriteLine("deflection: " + deflection);
                            */
                        }
                        /*
                        if (datas[i].Contains("E"))
                        {
                            PrintDataValue(dv);
                        }
                        */
                    }

                    //Console.WriteLine(line);

                    line = sr.ReadLine();
                }
                Console.WriteLine("totalRecords :" + totalRecords);
                Console.WriteLine("Queue  Records :" + dataQueue.Count);
                sr.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                // return DateTime.Now;
            }
        }

        private void GetAccAndVibr(string fileName, string mode, ref Queue<DataValue> dataQueue)
        {
            uint count = 0;
            uint totalRecords = 0;

            try
            {
                StreamReader sr = new StreamReader(fileName);
                //string dateString = sr.ReadLine();
                //DateTime dt = DateTime.Parse(dateString);

                string[] type = null;
                string[] property = null;
                string[] id = null;
                string date = null;

                string line = sr.ReadLine();
                while (line != null)
                {
                    count++;
                    if (count == 2)//date,,,,,,,,,,,
                    {
                        date = line.Split(',')[1];
                        Console.WriteLine("date:" + date);
                    }

                    if (count == 7)//AVE,AVE,MIN,,,,,,,,,,,
                    {
                        type = line.Split(',');
                        id = new string[type.Length];
                    }

                    if (count == 9)//T1X (xxx),T2X (xxx),
                    {
                        property = line.Split(',');
                        for (int i = 0; i < property.Length; i++)
                        {
                            property[i] = property[i].Split('(')[0].Trim();
                            property[i] = property[i].Replace("CH", mode);
                            //type[i] = property[i] + "-" + type[i];

                            if (property[i].Length > 2)
                            {
                                id[i] = property[i].Substring(0, 2);
                            }
                            else
                            {
                                id[i] = property[i];
                            }

                            Console.Write(property[i] + ",");
                        }
                        Console.WriteLine("");
                        for (int i = 0; i < property.Length; i++)
                        {
                            Console.Write(type[i] + ",");
                        }
                        Console.WriteLine("");
                        for (int i = 0; i < property.Length; i++)
                        {
                            //id[i] = property[i].Substring(0, 2);
                            Console.Write(id[i] + ",");
                        }
                    }
                    //忽略文件信息
                    if (count < 10)
                    {
                        //Console.WriteLine(line);
                        line = sr.ReadLine();
                        continue;
                    }

                    string[] datas = line.Split(',');
                    string cur_date = null;
                    for (int i = 0; i < type.Length; i++)
                    {
                        if (i == 0)
                        {
                            cur_date = date + " " + datas[0];
                            continue;
                        }
                        totalRecords++;
                        string longId = this.initValue[id[i]].Parameter2;
                        DataValue dv = new DataValue();
                        dv.SensorId = longId;
                        dv.TimeStamp = cur_date;
                        dv.Value = datas[i];
                        dv.ValueType = property[i] + "-" + type[i];

                        dataQueue.Enqueue(dv);

                        if (datas[i].Contains("E"))
                        {
                            PrintDataValue(dv);
                        }

                    }

                    //Console.WriteLine(line);

                    line = sr.ReadLine();
                }
                Console.WriteLine("totalRecords :" + totalRecords);
                Console.WriteLine("Queue  Records :" + dataQueue.Count);
                sr.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void AcquisitData(ref Queue<DataValue> dataQueue, string prefix)
        {
            //string prefix = this.dateTime.Day > 9 ? "D" + this.dateTime.Day + "_02_" : "D0" + this.dateTime.Day + "_02_";
            string accFilename = prefix + "02.csv";
            string incFilename = prefix + "01.csv";
            string vibFilename = prefix + "03.csv";
            GetInclination(incFilename, ref dataQueue);
            GetAccAndVibr(accFilename, "A", ref dataQueue);
            GetAccAndVibr(vibFilename, "V", ref dataQueue);
        }
    }
}
