using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using utils;
using utils.XmlAdapter;

namespace dato.anularFactura
{
    public partial class AnularFacturaReq : RestRequest
    {
        public static AnularFacturaReq FromJson(string json) => FromJson<AnularFacturaReq>(json);

        [Obsolete]
        public static new AnularFacturaReq ValidateAndCreate(object args) 
        {
            var dargs = RestRequest.ValidateAndCreate(args);

            long sucursal = long.Parse(dargs["sucursal"]);

            if (sucursal > 9)
            {
                string sql = $"SELECT CODDEPARTAMENTO FROM GANADERO.SUCURSALES WHERE SUCURSAL = {sucursal}";
                var rows = DBConnection.Instance.Query(sql);

                if (rows.Length < 1) throw new ArgumentException($"GANADERO.SUCURSALES.CODDEPARTAMENTO = {sucursal}, NOT FOUND");
                dargs["sucursal"] = rows[0]["CODDEPARTAMENTO"].ToString();
            }

            return new AnularFacturaReq(dargs);
        }

        public AnularFacturaReq(Dictionary<string, string> args) : base(args)
        {
            Data = new Data()
            {
                ServiceId = 634,
                Filtro =  new Filtro[]
                {
                    new Filtro() {
                        Editable = "N", Mandatory = true,
                        Label = "cuf", Value = args["cuf"]
                    },
                    new Filtro() { 
                        Editable = "N", Mandatory = true,
                        Label = "motivo", Value = args["motivo"]
                    },
                    new Filtro() {
                        Editable = "N", Mandatory = true,
                        Label = "Sucursal", Value = args["sucursal"]
                    },
                }
            };

            Metadata = new Metadata()
            {
                CodSucursal = 70,
                CodAplicacion = 1,
                CodUsuario = "JBK"
            };
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

    [DataContract]
    public partial class Filtro
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

    public partial class Metadata
    {
        [JsonProperty("codUsuario")]
        public string CodUsuario { get; set; }

        [JsonProperty("codSucursal")]
        public long CodSucursal { get; set; }

        [JsonProperty("codAplicacion")]
        public long CodAplicacion { get; set; }
    }
}