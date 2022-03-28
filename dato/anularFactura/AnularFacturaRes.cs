using System;
using System.Web;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using utils.XmlAdapter;

namespace dato.anularFactura
{
    public partial class AnularFacturaRes : RestResponse
    {
        public override string ToJson() => base.ToJson(Converter.Settings);
        public static AnularFacturaRes FromJson(string json) => FromJson<AnularFacturaRes>(json, Converter.Settings);

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

        [JsonProperty("pagos")]
        public DataPago[] Pagos { get; set; }
    }

    public partial class DataPago
    {
        [JsonProperty("pago")]
        public PagoPago[] Pago { get; set; }
    }

    public partial class PagoPago
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public Value Value { get; set; }

        [JsonProperty("mandatory")]
        public bool Mandatory { get; set; }

        [JsonProperty("editable")]
        public string Editable { get; set; }
    }

    public partial struct Value
    {
        public bool? Bool;
        public string String;

        public static implicit operator Value(bool Bool) => new Value { Bool = Bool };
        public static implicit operator Value(string String) => new Value { String = String };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ValueConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ValueConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Value) || t == typeof(Value?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Boolean:
                    var boolValue = serializer.Deserialize<bool>(reader);
                    return new Value { Bool = boolValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Value { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type Value");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Value)untypedValue;
            if (value.Bool != null)
            {
                serializer.Serialize(writer, value.Bool.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type Value");
        }

        public static readonly ValueConverter Singleton = new ValueConverter();
    }
}