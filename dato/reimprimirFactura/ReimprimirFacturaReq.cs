using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using utils.XmlAdapter;

namespace dato.reimprimirFactura
{
    public partial class ReimprimirFacturaReq : RestRequest
    {
        public static ReimprimirFacturaReq FromJson(string json) => FromJson<ReimprimirFacturaReq>(json);
        public static new ReimprimirFacturaReq ValidateAndCreate(object args) => new ReimprimirFacturaReq(RestRequest.ValidateAndCreate(args));

        public ReimprimirFacturaReq(Dictionary<string, string> args) : base(args) 
        {
            Data = new Data() {
                ServiceId = 634,
                Filtro = new Filtro[1] {
                    new Filtro() {
                        Editable = "N",
                        Mandatory = true,
                        Label = "idFacturaCiclos",
                        Value = args["idFacturaCiclos"],
                    }
                },
            };

            Metadata = new Metadata() { 
                CodUsuario = args["codUsuario"],
                CodSucursal = long.Parse(args["codSucursal"]),
                CodAplicacion = long.Parse(args["codAplicacion"])
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