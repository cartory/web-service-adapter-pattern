using System;
using System.IO;
using System.Xml;
using System.Web;
using System.Net;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace utils.XmlAdapter
{
    public class RestRequestOptions : RequestOptions
    {
        public static RestRequestOptions DefaultOptions => new RestRequestOptions();

        public int Timeout = -1;
        public bool SrvFormat = true;

        public string URL = "";
        public string Body = "{}";
        public string Method = "GET";
        public string Accept = "application/json";
        public string UserAgent = "Studio Manager";
        public string ContentType = "application/json; charset=UTF-8";

        public Func<string, RestResponse> JsonParser;
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
    }

    public class RestXmlAdapter : XmlAdapter
    {
        protected override string Fetch(RequestOptions requestOptions)
        {
            try
            {
                RestRequestOptions options = (RestRequestOptions)requestOptions;

                string json = this.CallRestAPI(options.URL, options);
                RestResponse res = options.JsonParser(json);

                return this.ToXml(options.SrvFormat ? res.ToSRV() : res.ToJson());
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Error on API CALL : {e.Message}");
            }
        }

        public string Fetch(string URL, RestRequestOptions requestOptions)
        {
            RestRequestOptions options = requestOptions ?? RestRequestOptions.DefaultOptions;
            requestOptions.URL = URL;

            return this.Fetch(options);
        }

        private string CallRestAPI(string URL, RestRequestOptions options)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(options.Body);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);

            req.Accept = options.Accept;
            req.Method = options.Method;
            req.UserAgent = options.UserAgent;
            req.ContentType = options.ContentType;

            req.ContentLength = jsonBytes.Length;

            foreach (KeyValuePair<string, string> header in options.Headers)
            {
                req.Headers.Add(header.Key, header.Value);
            }

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(jsonBytes, 0, jsonBytes.Length);
            reqStream.Close();

            WebResponse res = req.GetResponse();
            Stream resStream = res.GetResponseStream();

            string jsonRes = new StreamReader(resStream).ReadToEnd();

            res.Close();
            resStream.Close();

            return jsonRes;
        }
    }
}