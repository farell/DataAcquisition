﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

//Here is the once-per-application setup information
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace DataAcquisition
{
    //static class Program
    //{
    //    /// <summary>
    //    /// 应用程序的主入口点。
    //    /// </summary>
    //    [STAThread]
    //    static void Main()
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);
    //        Application.Run(new Form1());
    //    }
    //}

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Form1());
        //}

        [STAThread]
        static void Main()
        {
            try
            {
                //处理未捕获的异常  
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                //处理UI线程异常  
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                //处理非UI线程异常  
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                //ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                string str = "";
                string strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now.ToString() + "\r\n";
                if (ex != null)
                {
                    str = string.Format(strDateInfo + "异常类型：{0}\r\n异常消息：{1}\r\n异常信息：{2}\r\n",
                       ex.GetType().Name, ex.Message, ex.StackTrace);
                }
                else
                {
                    str = string.Format("应用程序线程错误:{0}", ex);
                }

                writeLog(str);
                List<string> keys = GetConfig();
                SendEmail("DataAcquisition Main() crashes", str, keys);
                MessageBox.Show("发生致命错误，请及时联系作者！", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        /// <summary>
        ///这就是我们要在发生未处理异常时处理的方法，我这是写出错详细信息到文本，如出错后弹出一个漂亮的出错提示窗体，给大家做个参考
        ///做法很多，可以是把出错详细信息记录到文本、数据库，发送出错邮件到作者信箱或出错后重新初始化等等
        ///这就是仁者见仁智者见智，大家自己做了。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            //ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            string str = "";
            string strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now.ToString() + "\r\n";
            Exception error = e.Exception as Exception;
            if (error != null)
            {
                str = string.Format(strDateInfo + "异常类型：{0}\r\n异常消息：{1}\r\n异常信息：{2}\r\n",
                   error.GetType().Name, error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("应用程序线程错误:{0}", e);
            }
            writeLog(str);
            List<string> keys = GetConfig();
            SendEmail("DataAcquisition Application_ThreadException", str, keys);
            MessageBox.Show("发生致命错误，请及时联系作者！", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static List<string> GetConfig()
        {
            StreamReader sr = new StreamReader("config.csv", Encoding.UTF8);
            String line;
            List<string> keys = new List<string>();
            while ((line = sr.ReadLine()) != null)
            {
                keys.Add(line);
            }
            sr.Close();
            return keys;
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            string str = "";
            Exception error = e.ExceptionObject as Exception;
            string strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now.ToString() + "\r\n";
            if (error != null)
            {
                str = string.Format(strDateInfo + "Application UnhandledException:{0};\n\r堆栈信息:{1}", error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("Application UnhandledError:{0}", e);
            }
            writeLog(str);
            List<string> keys = GetConfig();
            SendEmail("DataAcquisition CurrentDomain_UnhandledException", str,keys);
            MessageBox.Show("发生致命错误，请停止当前操作并及时联系作者！", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="str"></param>
        static void writeLog(string str)
        {
            if (!Directory.Exists("ErrLog"))
            {
                Directory.CreateDirectory("ErrLog");
            }
            using (StreamWriter sw = new StreamWriter(@"ErrLog\ErrLog.txt", true))
            {
                sw.WriteLine(str);
                sw.WriteLine("---------------------------------------------------------");
                sw.Close();
            }
        }

        static void SendEmail(string subject, string body,List<string> keys)
        {
            string senderServerIp = "smtp.163.com";
            string toMailAddress = "987522363@qq.com";
            string fromMailAddress = keys[0];

            MailMessage mMailMessage;
            SmtpClient mSmtpClient;
            mMailMessage = new MailMessage();
            mMailMessage.To.Add(toMailAddress);
            mMailMessage.From = new MailAddress(fromMailAddress);
            mMailMessage.Subject = subject;
            mMailMessage.Body = body;
            mMailMessage.IsBodyHtml = false;
            mMailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mMailMessage.Priority = MailPriority.Normal;

            mSmtpClient = new SmtpClient();
            mSmtpClient.Host = senderServerIp;
            mSmtpClient.Port = 25;
            mSmtpClient.UseDefaultCredentials = false;
            mSmtpClient.EnableSsl = true;

            mSmtpClient.Credentials = new System.Net.NetworkCredential(keys[1], keys[2]);
            mSmtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            mSmtpClient.Timeout = 20000;
            mSmtpClient.Send(mMailMessage);
        }
    }
}
