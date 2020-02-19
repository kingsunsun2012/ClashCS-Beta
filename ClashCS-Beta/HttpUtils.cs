﻿using System.Net;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Windows.Forms;

namespace ClashCS
{
    public class HttpUtils
    {
        public static string IP = "http://127.0.0.1";
        public static string PORT = "9090";
        public static string logURL = IP + ":" + PORT + "/logs";
        public static string configsURL = IP + ":" + PORT + "/configs";
        public static string proxiesURL = IP + ":" + PORT + "/proxies";
        public static string trafficURL = IP + ":" + PORT + "/traffic";

        public static string Start(string url, string path)
        {
            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            //创建本地文件写入流
            Stream stream = new FileStream(path, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            stream.Close();
            responseStream.Close();
            return "Download Success!";
        }

        public void RestGetStream()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(logURL);
                request.Method = "Get";
                request.ContentLength = 0;
                request.ContentType = "application/json";
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                var sr = new StreamReader(responseStream);
                while (responseStream != null)
                {
                    object _json = sr.ReadLine();
                    LogsForm.context.Send(LogsForm.AddLog, _json);
                }
                object noLog = "Stream is empty!";
                LogsForm.context.Send(LogsForm.AddLog, noLog);
            }
            catch
            {
                return;
            }
        }

        public void RestGetStream(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "Get";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var sr = new StreamReader(responseStream);
            while (responseStream != null)
            {
                try
                {
                    var _json = sr.ReadLine();
                    var _list = _json.ToList();
                    _list.Remove('{');
                    _list.Remove('}');
                    _json = string.Join("", _list.ToArray());
                    var _d = _json.Split(',', ':');
                    object s = ((double)(int.Parse(_d[1]) / 1000)).ToString() + ',' + ((double)(int.Parse(_d[3]) / 1000)).ToString();

                    MainForm.cntx.Send(MainForm.SetUpDown, s);
                }
                catch { return; }
            }
            try { MainForm.cntx.Send(MainForm.SetUpDown, "0,0"); }
            catch { return; }
        }

        public string[] RestGet(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "Get";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var sr = new StreamReader(responseStream);
            var _json = sr.ReadToEnd();
            var list = _json.ToList();
            list.Remove('{');
            list.Remove('}');
            int i = 22;
            while (i != 0)
            {
                list.Remove('\"');
                i--;
            }
            _json = string.Join("", list.ToArray());
            var _d = _json.Split(':', ',');
            return new string[] { _d[1], _d[3], _d[5], _d[9], _d[13] }; //port sport rport allowlan mode            
        }

        public void RestPatch(MainForm.Configs _c)
        {
            string postData = "{\"port\":" + _c.port + ", \"socks-port\":" + _c.sport + ", \"redir-port\":" + _c.rport + ", \"allow-lan\":" + _c.lan + ", \"mode\":" + "\"" + _c.mode + "\"}";
            //MessageBox.Show(postData);
            var encoding = new UTF8Encoding();
            var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(postData);
            var request = (HttpWebRequest)WebRequest.Create(configsURL);
            request.Method = "PATCH";
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json";
            try
            {
                var writeStream = request.GetRequestStream();
                writeStream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    var message = string.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    MessageBox.Show(message);
                    throw new ApplicationException(message);
                }
                else MessageBox.Show("Apply complete!", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}