using System;
using System.Collections.Generic;
using System.Text;

namespace DataAcquisition
{
    public class MidValueFilter
    {
        /**
         * 窗口大小为 9
         */
        private int filterLength;
        private Queue<double> dataQueue;
        private double[] sortArr;
        private int count = 0;

        public MidValueFilter(int length)
        {
            this.filterLength = length;
            this.dataQueue = new Queue<double>();
            /*
            this.dataQueue = new Queue<double>(this.filterLength);
            for (int i = 0; i < filterLength; i++)
            {
                this.dataQueue.Enqueue(0);
            }*/
        }
        /*
        public double GetFilteredValue(double newData)
        {
            if (this.count < this.filterLength / 2 + 1)
            {
                this.count++;
            }
            dataQueue.Dequeue();
            dataQueue.Enqueue(newData);
            sortArr = dataQueue.ToArray();
            Array.Sort(sortArr);
            Array.Reverse(sortArr);

            //double avg = (sortArr.Sum()-sortArr[0]-sortArr[filterLength-1])/(filterLength - 2);
            if (this.count < this.filterLength / 2 + 1)
            {
                return this.sortArr[this.filterLength - this.count];
            }
            else
            {
                return sortArr[filterLength / 2];
            }
        }*/

        public double GetFilteredValue(double newData)
        {
            if (this.count < this.filterLength)
            {
                this.count++;
                //dataQueue.Enqueue(newData);
            }
            else
            {
                dataQueue.Dequeue();
            }

            dataQueue.Enqueue(newData);
            sortArr = dataQueue.ToArray();
            Array.Sort(sortArr);
            //Array.Reverse(sortArr);

            //double avg = (sortArr.Sum()-sortArr[0]-sortArr[filterLength-1])/(filterLength - 2);
            if (this.count < this.filterLength)
            {
                //return (sortArr.Sum() - sortArr[0] - sortArr[1]) / (sortArr.Length - 2);
                return this.sortArr[this.sortArr.Length / 2];
            }
            else
            {
                //return (sortArr.Sum() - sortArr[0] - sortArr[1]) / (filterLength - 2);
                return this.sortArr[filterLength / 2];
            }
        }

        public void PrintQueue()
        {
            double[] temp = this.dataQueue.ToArray();
            for (int i = 0; i < temp.Length; i++)
            {
                Console.Write(temp[i] + ",");
            }
            Console.WriteLine("");
            for (int i = 0; i < this.sortArr.Length; i++)
            {
                Console.Write(this.sortArr[i] + ",");
            }
        }
    }
}
