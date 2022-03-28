using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace utils.XmlAdapter
{
    public abstract class JSON
    {
        public virtual string ToJson() => JsonConvert.SerializeObject(this);
        protected virtual string ToJson(JsonSerializerSettings settings) => JsonConvert.SerializeObject(this, settings);

        public static T FromJson<T>(string json) => JsonConvert.DeserializeObject<T>(json);
        protected static T FromJson<T>(string json, JsonSerializerSettings settings) => JsonConvert.DeserializeObject<T>(json, settings);
    }
}