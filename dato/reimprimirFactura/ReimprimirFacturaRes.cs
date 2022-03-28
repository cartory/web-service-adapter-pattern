using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;

using Newtonsoft.Json;

using utils.XmlAdapter;

namespace dato.reimprimirFactura
{
    public partial class ReimprimirFacturaRes : RestResponse
    {
        public static ReimprimirFacturaRes FromJson(string json) => FromJson<ReimprimirFacturaRes>(json);

        public override string ToSRV() => this.ToSRV(ErrMsg);

        [JsonProperty("errCode")]
        public string ErrCode { get; set; }

        [JsonProperty("errMsg")]
        public string ErrMsg { get; set; }

        [JsonProperty("data")]
        public DataRes Data { get; set; }
    }

    public partial class DataRes
    {
        [JsonProperty("serviceId")]
        public long ServiceId { get; set; }

        [JsonProperty("impresiones")]
        public Impresione[] Impresiones { get; set; }
    }

    public partial class Impresione
    {
        [JsonProperty("impresion")]
        public Impresion[] Impresion { get; set; }
    }

    public partial class Impresion
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("mandatory")]
        public bool Mandatory { get; set; }

        [JsonProperty("editable")]
        public string Editable { get; set; }
    }
}