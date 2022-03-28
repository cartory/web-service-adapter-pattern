using System;
using System.Web;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

using System.Web.Services;

using dato.anularFactura;
using dato.emitirFactura;
using dato.reimprimirFactura;

using utils;
using utils.XmlAdapter;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebService
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private static readonly string user = ConfigurationManager.AppSettings["user"];
        private static readonly string pass = ConfigurationManager.AppSettings["pass"];
        private static readonly string URL = ConfigurationManager.AppSettings["API_URL"];
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["dev"].ConnectionString;

        [Obsolete]
        public static DBConnection connection = DBConnection.Instance.SetConnectionString(connectionString);

        private readonly RestXmlAdapter restXmlAdapter = new RestXmlAdapter();
        //public static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, object> args = new Dictionary<string, object>() { { "req", null }, { "params", null } };
        private readonly Dictionary<string, string> Headers = new Dictionary<string, string>()
        {
            { "Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"))}"}
        };

        [Obsolete]
        [WebMethod]
        public string AnularFactura
        (
            string cuf,
            string motivo,
            string sucursal
        )
        {
            object args = new { cuf, motivo, sucursal };
            //log.Info(JsonConvert.SerializeObject(args));

            try
            {
                RestRequest req = AnularFacturaReq.ValidateAndCreate(args);

                return restXmlAdapter.Fetch($"{URL}/int/ps/router/documentos/revertir", new RestRequestOptions() {
                    Method = "POST",
                    Headers = Headers,
                    Body = req.ToJson(),
                    JsonParser = json => AnularFacturaRes.FromJson(json)
                });
            }
            catch (Exception e)
            {
                //log.Error(e.Message, e);
                return restXmlAdapter.ToXml(RestResponse.DefaultSRV(CodeRes.COD003, e.Message));
            }
        }

        [Obsolete]
        [WebMethod]
        public string EmitirFactura
        (
            string codfacturasdetalle,

            string codempresafacturadora,
            string asiento,
            string codaseguradora,

            string nombre,
            string nit,
            string fecha,

            string sucursal,
            string esfactura,
            string conceptos,

            string correo,
            string codusuario,
            string codaplicacion,
            string tipopago,
            string nrogarantia,
            string nropago
        )
        {
            args["params"] = JObject.FromObject(new {
                codfacturasdetalle, 
                codempresafacturadora, asiento, codaseguradora, 
                nombre, nit, fecha, sucursal, esfactura, conceptos, 
                correo, codusuario, codaplicacion, tipopago, nrogarantia, nropago
            }).ToObject<Dictionary<string, string>>();

            try
            {
                RestRequest req = EmitirFacturaReq.ValidateAndCreate(args["params"]);

                return restXmlAdapter.Fetch($"{URL}/int/ps/router/documentos/generar", new RestRequestOptions() {
                    Method = "POST",
                    Headers = Headers,
                    Body = req.ToJson(),
                    JsonParser = json => {
                        EmitirFacturaRes res = EmitirFacturaRes.FromJson(json);
                        res.UpdateDB();

                        return res;
                    }
                });
            }
            catch (Exception e)
            {
                return restXmlAdapter.ToXml(RestResponse.DefaultSRV(CodeRes.COD003, e.Message));
            }
        }

        [WebMethod]
        public string ReimprimirFactura
        (
            string idFacturaCiclos,
            string codUsuario,
            string codSucursal,
            string codAplicacion
        )
        {
            object args = new { idFacturaCiclos, codUsuario, codSucursal, codAplicacion };

            try
            {
                RestRequest req = ReimprimirFacturaReq.ValidateAndCreate(args);

                return restXmlAdapter.Fetch($"{URL}/int/ps/router/documentos", new RestRequestOptions() {
                    Method = "POST",
                    Headers = Headers,
                    Body = req.ToJson(),
                    JsonParser = json => ReimprimirFacturaRes.FromJson(json)
                });
            }
            catch (Exception e)
            {
                return restXmlAdapter.ToXml(RestResponse.DefaultSRV(CodeRes.COD003, e.Message));
            }
        }
    }
}
