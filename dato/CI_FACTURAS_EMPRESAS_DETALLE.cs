using System;

using utils;

using System.Data.OracleClient;

namespace dato
{
	public class CI_FACTURAS_EMPRESAS_DETALLE
	{
		public string XML;
		public string CUF;
		public string CORREO;
		public string ESTADO = "V";
		public string MOTIVO;
		public string MENSAJE;
		public string ES_VALIDO;
		public string EXTENSION;
		public string ES_FACTURA;
		public string LISTAMENSAJES;
		public string NOMBRE_ARCHIVO;
		public string ID_TRANSACCION;
		public string USUARIO_REGISTRO;
		public string USUARIO_MODIFICACION;

		public decimal CANAL;
		public decimal ASIENTO;
		public decimal TZ_LOCK = 0;
		public decimal NRO_PAGO;
		public decimal SUCURSAL;
		public decimal NUMEROPERSONA;
		public decimal COD_ASEGURADORA;
		public decimal ID_FACTURA_CICLO;
		public decimal CODIGO_REFERENCIA;
		public decimal COD_FACTURAS_DETALLE;
		public decimal COD_EMPRESA_FACTURADORA;

		public byte[] ARCHIVO;
		public DateTime FECHA_PROCESO;
		public DateTime FECHA_REGISTRO;
		public DateTime FECHA_MODIFICACION;

		public CI_FACTURAS_EMPRESAS_DETALLE() { }

        [Obsolete]
        public void UpdateDB()
		{
			string sql = "UPDATE GANADERO.CI_FACTURAS_EMPRESAS_DETALLE SET " +
				$"COD_ASEGURADORA={COD_ASEGURADORA}, ID_TRANSACCION='{ID_TRANSACCION}'," +
				$"USUARIO_MODIFICACION='{USUARIO_MODIFICACION}'," +
				$"ID_FACTURA_CICLO={ID_FACTURA_CICLO}, EXTENSION='{EXTENSION}', ARCHIVO=:ARCHIVO," +
				$"CUF='{CUF}', FECHA_MODIFICACION=CURRENT_TIMESTAMP, ESTADO='{ESTADO}' " +
				$"WHERE COD_FACTURAS_DETALLE={COD_FACTURAS_DETALLE} " +
				$"AND ASIENTO={ASIENTO} AND FECHA_PROCESO=TO_DATE('{FECHA_PROCESO.ToShortDateString()}', 'mm/dd/yyyy') " +
				$"AND CODIGO_REFERENCIA={CODIGO_REFERENCIA} AND NRO_PAGO={NRO_PAGO}";

			DBConnection.Instance.Query((command) =>
			{
				command.CommandText = sql.Trim();
				command.Parameters.Add(new OracleParameter("ARCHIVO", OracleType.Blob) { Value = ARCHIVO });

				command.ExecuteNonQuery();
			});
		}
	}
}