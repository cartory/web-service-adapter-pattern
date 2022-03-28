using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace utils.XmlAdapter
{
    public abstract class RestRequest : JSON
    {
        protected RestRequest(Dictionary<string, string> args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }
        }

        protected static Dictionary<string, string> ValidateAndCreate(object args)
        {
            Dictionary<string, string> dargs = args is Dictionary<string, string>
                ? (Dictionary<string, string>)args
                : JObject.FromObject(args).ToObject<Dictionary<string, string>>();

            string[] keys = dargs.Keys.ToArray();

            Array.ForEach(keys, k => dargs[k] = dargs[k].Trim());

            Array.ForEach(keys, k => {
                if (string.IsNullOrEmpty(dargs[k]))
                {
                    throw new ArgumentException($"'{k}' cannot be null or empty");
                }
            });

            return dargs;
        }
    }
}