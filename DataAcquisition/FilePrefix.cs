using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataAcquisition
{
    class FilePrefix
    {
        private string basePath;
        private string stampFile;
        private DateTime dt;
        private string mode;

        public FilePrefix(string stamp, string mod, string basePath)
        {
            this.stampFile = stamp;
            this.dt = LoadStamp(stamp);
            this.basePath = basePath;
            this.mode = mod;
        }

        public bool IsDateValid()
        {
            DateTime now = DateTime.Now;
            int result = DateTime.Compare(now, this.dt);
            if (result <= 0)
            {
                return false;
            }
            return true; ;
        }

        public string GetFilePrefix()
        {
            string prefix = null;
            string folder = "Y" + this.dt.Year;
            string moth = this.dt.Month > 9 ? "_" + this.dt.Month : "_0" + this.dt.Month;
            folder = folder + moth + "\\";
            string date = this.dt.Day > 9 ? "D" + this.dt.Day + "_" + this.mode + "_" : "D0" + this.dt.Day + "_" + this.mode + "_";
            prefix = this.basePath + folder + date;

            using (StreamWriter sw = new StreamWriter(this.stampFile))
            {
                sw.WriteLine(this.dt.ToShortDateString());
                sw.Close();
            }
            this.dt = this.dt.AddDays(1);
            return prefix;
        }

        private DateTime LoadStamp(string fileName)
        {
            try
            {
                StreamReader sr = new StreamReader(fileName);
                string dateString = sr.ReadLine();
                sr.Close();
                DateTime dt = DateTime.Parse(dateString);
                return dt;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
                return DateTime.Now;
            }

        }
    }
}
