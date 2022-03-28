using System;
using System.Web;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using utils;
using utils.XmlAdapter;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;


namespace dato.emitirFactura
{
    public partial class EmitirFacturaReq : RestRequest
    {
        public override string ToJson() => base.ToJson(EmitirConverter.Settings);
        public static EmitirFacturaReq FromJson(string json) => FromJson<EmitirFacturaReq>(json, EmitirConverter.Settings);

        /// <summary>
        /// args = new {
        ///    codfacturasdetalle,
        ///    codempresafacturadora, asiento, codaseguradora,
        ///    nombre, nit, fecha, sucursal, esfactura, conceptos,
        ///    correo, codusuario, codaplicacion, tipopago, nrogarantia, nropago
        /// } or Dictionary<string, string> Conversion
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Obsolete]
        public static new EmitirFacturaReq ValidateAndCreate(object args)
        {
            Dictionary<string, string> dargs = RestRequest.ValidateAndCreate(args);
            const string emailPattern = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

            long sucursal = long.Parse(dargs["sucursal"]);
            float tipoPago = float.Parse(dargs["tipopago"]);

            if (!Regex.IsMatch(dargs["correo"]?.ToLower() ?? "", emailPattern)) dargs["correo"] = "";

            if (tipoPago < 1 || tipoPago > 3) throw new ArgumentException($"tipopago NOT VALID");
            if (string.IsNullOrEmpty(dargs["nit"]) || dargs["nit"] == "0") throw new ArgumentNullException($"nit NOT VALID");

            if (sucursal > 9) 
            {
                string sql = $"SELECT CODDEPARTAMENTO FROM GANADERO.SUCURSALES WHERE SUCURSAL = {sucursal}";
                var rows = DBConnection.Instance.Query(sql);

                if (rows.Length < 1) throw new ArgumentException($"GANADERO.SUCURSALES.CODDEPARTAMENTO = {sucursal}, NOT FOUND");
                dargs["sucursal"] = rows[0]["CODDEPARTAMENTO"].ToString();
            }

            WebService.WebService1.args["params"] = dargs;
            return new EmitirFacturaReq(dargs);
        }

        [Obsolete]
        private decimal TipoCambio_USD_BOB
        {
            get
            {
                string sql = "SELECT CIERRE_OFICIAL_VENTA_BCB FROM GANADERO.MONEDAS WHERE C6399 = 2225";
                var rows = DBConnection.Instance.Query(sql);

                if (rows.Length < 1) throw new ArgumentException("GANADERO.MONEDAS.CIERRE_OFICIAL_VENTA_BCB NOT FOUND");

                return decimal.Parse(rows[0]["CIERRE_OFICIAL_VENTA_BCB"].ToString());
            }
        }

        [Obsolete]
        public EmitirFacturaReq(Dictionary<string, string> args) : base(args) 
        {
            Guid guid = Guid.NewGuid();
            args["fecha"] = DateTime.ParseExact(args["fecha"], "ddMMyyyy", CultureInfo.InvariantCulture).ToString("o");

            Dictionary<int, string> campos = new Dictionary<int, string>() {
                { 11, $"correo;{args["correo"]}" },
                { 23, $"idTransaccion;{guid}" },
                { 24, $"nombre;{args["nombre"]}" },
                { 25, $"nit;{args["nit"]}" },
                { 26, $"fecha;{args["fecha"]}" },
                { 27, $"sucursal;{args["sucursal"]}" },
                { 28, $"esFactura;{args["esfactura"] == "1"}" },
                { 29, $"conceptos;{args["conceptos"]}" },
                { 30, $"producto;" },
                { 31, $"auxiliar;" },
                { 32, $"compania;" },
                { 33, $"medioPago;{args["tipopago"].Split('.')[0]}" },
            };

            Filtro[] filtros = new Filtro[campos.Count];

            int i = 0;
            foreach (KeyValuePair<int, string> s in campos)
            {
                int identificador = s.Key;
                string[] aliasValor = s.Value.Split(';');

                filtros[i] = new Filtro() { Alias = aliasValor[0], Identificador = identificador };

                if (aliasValor[0] == "esFactura")
                {
                    filtros[i].Valor = new ValorUnion() { String = aliasValor[1].ToLower() };
                }
                else if (aliasValor[0] == "conceptos")
                {
                    // 1 element => item?detalle?montoUsd
                    string[] tmp = aliasValor[1].Split('?');

                    decimal tipoCambio = this.TipoCambio_USD_BOB;
                    decimal montoUsd = decimal.Parse(tmp[2].Replace(',', '.'));
                    decimal montoBob = tipoCambio * montoUsd;

                    Dictionary<string, string> filaCampos = new Dictionary<string, string>()
                    {
                        { "item", tmp[0] },
                        { "detalle", tmp[1] },
                        { "montoBob", montoBob.ToString()},
                        { "montoUsd", montoUsd.ToString() },
                        { "tipoCambio", tipoCambio.ToString() },
                        { "ramo", "14" },
                    };

                    int id = 0;
                    var filas = new Fila[filaCampos.Count];

                    foreach (KeyValuePair<string, string> fc in filaCampos)
                    {
                        filas[id++] = new Fila() { Identificador = id, Alias = fc.Key, Valor = fc.Value };
                    }

                    var valorElementArray = new ValorElement[1] { new ValorElement() { Fila = filas } };
                    filtros[i].Valor = new ValorUnion() { ValorElementArray = valorElementArray };
                }
                else
                {
                    filtros[i].Valor = new ValorUnion() { String = aliasValor[1] };
                }

                i++;
            }
 
            Data = new Data() {
                ServiceId = 634,
                Filtro = filtros,
            };

            Metadata = new Metadata() {
                CodUsuario = args["codusuario"],
                CodSucursal = long.Parse(args["sucursal"]),
                CodAplicacion = long.Parse(args["codaplicacion"])
            };

            WebService.WebService1.args["req"] = this;
            WebService.WebService1.args["guid"] = guid.ToString();
        }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("serviceId")]
        public long ServiceId { get; set; }

        [JsonProperty("filtro")]
        public Filtro[] Filtro { get; set; }
    }

    public partial class Filtro
    {
        [JsonProperty("identificador")]
        public long Identificador { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("valor")]
        public ValorUnion Valor { get; set; }
    }

    public partial class ValorElement
    {
        [JsonProperty("fila")]
        public Fila[] Fila { get; set; }
    }

    public partial class Fila
    {
        [JsonProperty("identificador")]
        public long Identificador { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("valor")]
        public string Valor { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("codUsuario")]
        public string CodUsuario { get; set; }

        [JsonProperty("codSucursal")]
        public long CodSucursal { get; set; }

        [JsonProperty("codAplicacion")]
        public long CodAplicacion { get; set; }
    }

    public partial struct ValorUnion
    {
        public string String;
        public ValorElement[] ValorElementArray;

        public static implicit operator ValorUnion(string String) => new ValorUnion { String = String };
        public static implicit operator ValorUnion(ValorElement[] ValorElementArray) => new ValorUnion { ValorElementArray = ValorElementArray };
    }

    internal static class EmitirConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ValorUnionConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ValorUnionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ValorUnion) || t == typeof(ValorUnion?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new ValorUnion { String = stringValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<ValorElement[]>(reader);
                    return new ValorUnion { ValorElementArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type ValorUnion");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (ValorUnion)untypedValue;
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            if (value.ValorElementArray != null)
            {
                serializer.Serialize(writer, value.ValorElementArray);
                return;
            }
            throw new Exception("Cannot marshal type ValorUnion");
        }

        public static readonly ValorUnionConverter Singleton = new ValorUnionConverter();
    }
}