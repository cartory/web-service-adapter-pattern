using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using System.Xml.Serialization;

namespace utils.XmlAdapter
{
    public static class CodeRes
    {
        public const string COD000 = "COD000";
        public const string COD003 = "COD003";
    }

    public abstract class RestResponse : JSON
    {
        public abstract string ToSRV();
        public virtual string ToSRV(string errMsg) 
        {
            if (string.IsNullOrEmpty(errMsg)) 
            {
                DefaultSRV(CodeRes.COD000);
            }

            return DefaultSRV(CodeRes.COD003, errMsg);
        }

        public static string DefaultSRV(string code, string message = "")
        {
            return JsonConvert.SerializeObject(new { CodRes = code, CodError = code, DesError = message });
        }
    }
}