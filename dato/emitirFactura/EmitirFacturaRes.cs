using System;
using System.Web;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using utils.XmlAdapter;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;

namespace dato.emitirFactura
{
    public partial class EmitirFacturaRes : RestResponse
    {
        public override string ToJson() => base.ToJson(ConverterRes.Settings);
        public static EmitirFacturaRes FromJson(string json) => FromJson<EmitirFacturaRes>(json, ConverterRes.Settings);

        public override string ToSRV()
        {
            if (!string.IsNullOrEmpty(ErrCode)) 
            {
                return DefaultSRV(CodeRes.COD003);
            }

            Dictionary<string, string> args = new Dictionary<string, string>();

            Array.ForEach(this.Data.Documentos[0].Documento, doc => {
                string label = doc.Label;
                string value = doc.Value.String;

                if (label == "correo") args.Add(label, value);
                else if (label == "numeroFactura") args.Add(label, value);
                else if (label == "codigoControl") args.Add(label, value);
                else if (label == "NombreArchivo") args.Add(label, value);
                else if (label == "numeroAutorizacion") args.Add(label, value);
            });

            EmitirFacturaReq req = (EmitirFacturaReq)WebService.WebService1.args["req"];

            return JsonConvert.SerializeObject(new {
                CodRes = CodeRes.COD000,
                CodError = CodeRes.COD000,
                DesError = ErrMsg,

                Correo = args["correo"],
                CodControl = args["codigoControl"],
                NroFactura = args["numeroFactura"],
                Cuf = this.Data.Anular[0].Value,
                NroAutorizacion = args["numeroAutorizacion"],
                NombreArchivo = args["NombreArchivo"].Split('.')[0],
                IdFacturaCiclos = this.Data.Anular[0].Value,
                IdTransaccion = Array.Find(req.Data.Filtro, f => f.Alias == "idTransaccion").Valor.String,
            });
        }

        public void UpdateDB()
        {
            Dictionary<string, string> columns = new Dictionary<string, string>();

            Array.ForEach(Data.Documentos[0].Documento, doc => {
                string label = doc.Label, value = doc.Value.String;

                if (label == "cuf") columns.Add(label, value);
                else if (label == "Archivo") columns.Add(label, value);
                else if (label == "NombreArchivo") columns.Add(label, value);
                else if (label == "idFacturaCiclos") columns.Add(label, value);
                else if (doc.Label == "numeroAutorizacion")
                {
                    if (string.IsNullOrEmpty(value)) throw new ArgumentException("nroAutorizacion NOT VALID, NOT UPDATE_DB");
                }
                else if (label == "numeroFactura")
                {
                    if (string.IsNullOrEmpty(value) || value == "0") throw new ArgumentException("nroFactura NOT VALID, NOT UPDATE_DB");
                }
            });

            EmitirFacturaReq req = (EmitirFacturaReq)WebService.WebService1.args["req"];
            Dictionary<string, string> fparams = (Dictionary<string, string>)WebService.WebService1.args["params"];

            Array.ForEach(req.Data.Filtro, filtro => {
                string alias = filtro.Alias;
                string value = filtro.Valor.String;

                if (alias == "esFactura") columns.Add(alias, value);
                else if (alias == "fecha") columns.Add(alias, value);
                else if (alias == "idTransaccion") columns.Add(alias, value);
            });


            var cfed = new CI_FACTURAS_EMPRESAS_DETALLE() 
            {
                COD_FACTURAS_DETALLE = decimal.Parse(fparams["codfacturasdetalle"]),
                
                ASIENTO = decimal.Parse(fparams["asiento"]),
                COD_ASEGURADORA = decimal.Parse(fparams["codaseguradora"]),
                COD_EMPRESA_FACTURADORA = decimal.Parse(fparams["codempresafacturadora"]),
                ES_FACTURA = columns["esFactura"],

                SUCURSAL = req.Metadata.CodSucursal,
                USUARIO_REGISTRO = req.Metadata.CodUsuario,
                USUARIO_MODIFICACION = req.Metadata.CodUsuario,
                FECHA_PROCESO = DateTime.Parse(columns["fecha"]),

                ID_TRANSACCION = columns["idTransaccion"],
                ID_FACTURA_CICLO = decimal.Parse(columns["idFacturaCiclos"]),

                CUF = columns["cuf"],
                EXTENSION = columns["NombreArchivo"].Split('.')[1],
                NOMBRE_ARCHIVO = columns["NombreArchivo"].Split('.')[0],
                ARCHIVO = Convert.FromBase64String(columns["Archivo"]),
                NRO_PAGO = decimal.Parse(fparams["nropago"]),
                CODIGO_REFERENCIA = decimal.Parse(fparams["nrogarantia"]),
            };

            #pragma warning disable CS0612 // Type or member is obsolete
            cfed.UpdateDB();
        }

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

        [JsonProperty("documentos")]
        public DataDocumento[] Documentos { get; set; }

        [JsonProperty("anular")]
        public Anular[] Anular { get; set; }

        [JsonProperty("reimprimir")]
        public Reimprimir[] Reimprimir { get; set; }
    }

    public partial class Anular
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

    public partial class DataDocumento
    {
        [JsonProperty("documento")]
        public DocumentoDocumento[] Documento { get; set; }
    }

    public partial class DocumentoDocumento
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

    public partial class Reimprimir
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("mandatory")]
        public bool Mandatory { get; set; }

        [JsonProperty("editable")]
        public string Editable { get; set; }
    }

    public partial struct Value
    {
        public long? Integer;
        public string String;

        public static implicit operator Value(long Integer) => new Value { Integer = Integer };
        public static implicit operator Value(string String) => new Value { String = String };
    }

    internal static class ConverterRes
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
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new Value { Integer = integerValue };
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
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value);
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