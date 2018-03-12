using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace YNHD.Common
{
    public static class ModelConvertHelper<T> where T : new()
    {



        /// <summary>
        /// 格式化字符型、日期型、布尔型
        /// </summary>
        private static string StringFormat(string str, Type type)
        {
            if (type == typeof(string))
            {
               
                str = "\"" + str + "\"";
            }
            else if (type == typeof(DateTime))
            {
                str = "\"" + str + "\"";
            }
            else if (type == typeof(bool))
            {
                str = str.ToLower();
            }
            else if (type != typeof(string) && string.IsNullOrEmpty(str))
            {
                str = "\"" + str + "\"";
            }
            return str;
        }




        public static IList<T> ConvertToModel(DataTable dt)
        {

            // 定义集合    
            IList<T> ts = new List<T>();

            // 获得此模型的类型   
            Type type = typeof(T);
            string tempName = "";

            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;  // 检查DataTable是否包含此列    

                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite) continue;

                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
                ts.Add(t);
            }
            return ts;
        }

        



        #region List转换成Json
        /// <summary>
        /// List转换成Json
        /// </summary>
        //public static string ListToJson<T>(IList<T> list)
        //{
        //    object obj = list[0];
        //    return ListToJson<T>(list, obj.GetType().Name);
        //}

        /// <summary>
        /// List转换成Json 
        /// </summary>
        //public static string ListToJson<T>(IList<T> list, string jsonName)
        //{
        //    StringBuilder Json = new StringBuilder();
        //    if (string.IsNullOrEmpty(jsonName)) jsonName = list[0].GetType().Name;
        //    Json.Append("{\"" + jsonName + "\":[");
        //    if (list.Count > 0)
        //    {
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            T obj = Activator.CreateInstance<T>();
        //            PropertyInfo[] pi = obj.GetType().GetProperties();
        //            Json.Append("{");
        //            for (int j = 0; j < pi.Length; j++)
        //            {
        //                Type type = pi[j].GetValue(list, null).GetType();
        //                Json.Append("\"" + pi[j].Name.ToString() + "\":" + StringFormat(pi[j].GetValue(list, null).ToString(), type));

        //                if (j < pi.Length - 1)
        //                {
        //                    Json.Append(",");
        //                }
        //            }
        //            Json.Append("}");
        //            if (i < list.Count - 1)
        //            {
        //                Json.Append(",");
        //            }
        //        }
        //    }
        //    Json.Append("]}");
        //    return Json.ToString();
        //}
        #endregion






    }


    public class Modle
    {
        public string SensorID { get; set; }

        public string StampTime { get; set; }

        public string VauleType { get; set; }
        public string Values { get; set; }
    }


}
