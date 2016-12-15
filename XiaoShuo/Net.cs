using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace XiaoShuo
{
    public class Net
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource">key=要下载的资源的web地址，value=下载到本地的地址</param>
        /// <returns></returns>
        public static List<string> LoadWebResource(Dictionary<string, string> resource)
        {
            if (resource != null && resource.Any())
            {
                foreach (var item in resource)
                {
                    try
                    {
                        WebClient mywebclient = new WebClient();
                        mywebclient.DownloadFile(item.Key, item.Value);
                    }
                    catch
                    {
                    }
                }
            }
            return resource.Select(c => c.Value).ToList();
        }

        public static string PostRequest(string Url, IDictionary<string, string> paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            Stream stream = null;
            HttpWebResponse response = null;
            StreamReader sr = null;
            try
            {
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(Url);
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                if (paramData != null && paramData.Any())
                {
                    StringBuilder parms = new StringBuilder();
                    foreach (var item in paramData)
                    {
                        parms.Append(String.Format("{0}={1}&", item.Key, item.Value));
                    }
                    byte[] byteArray = dataEncode.GetBytes(parms.ToString().Substring(0, parms.ToString().Length - 1));
                    webReq.ContentLength = byteArray.Length;
                    stream = webReq.GetRequestStream();
                    stream.Write(byteArray, 0, byteArray.Length);
                    stream.Close();
                }
                response = (HttpWebResponse)webReq.GetResponse();
                sr = new StreamReader(response.GetResponseStream(), dataEncode);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (sr != null)
                {
                    sr.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                throw ex;
            }
            return ret;
        }

        public static string GetRequest(string url, IDictionary<string, string> paramData, Encoding dataEncode, int timeout = 5000)
        {
            try
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "http://" + url;
                }
                if (paramData != null && paramData.Any())
                {
                    if (!url.EndsWith("?"))
                    {
                        url += "?";
                    }
                    StringBuilder parms = new StringBuilder();
                    foreach (var item in paramData)
                    {
                        parms.Append(String.Format("{0}={1}&", item.Key, HttpUtility.UrlEncode(item.Value)));
                    }
                    url += parms.ToString();
                    url = url.Remove(url.Length - 1);
                }
                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.Method = "GET";
                webRequest.Timeout = timeout;

                using (var response = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), dataEncode))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(url);
                return "";
            }
        }
    }
}
