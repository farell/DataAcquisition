using System;
using System.Collections.Generic;
using System.Text;

namespace DataAcquisition
{
    public struct DataValue
    {
        public string SensorId;
        public string TimeStamp;
        public string ValueType;
        public string Value;
    }

    class SensorInitVal
    {
        public string SENSOR_ID;
        public string Parameter1;
        public string Parameter2;
        public string Parameter3;
        public string Parameter4;
    }
}