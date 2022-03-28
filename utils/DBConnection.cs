using System;
using System.Web;
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System.Data.OracleClient;

namespace utils
{
    public class DBConnection
    {
        public static DBConnection Instance => _instance;
        private static readonly DBConnection _instance = new DBConnection();
        
        [Obsolete]
        private OracleConnection Connection;

        private DBConnection() { }

        [Obsolete]
        public DBConnection SetConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) 
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
            }

            Connection = new OracleConnection(connectionString);

            return this;
        }

        [Obsolete]
        private void OpenConnection()
        {
            try
            {
                if (Connection?.State != ConnectionState.Open)
                {
                    Connection.Open();
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"{e.GetType()}, Error on OpenConnection!", nameof(e));
            }
        }

        [Obsolete]
        private void CloseConnection()
        {
            try
            {
                if (Connection?.State != ConnectionState.Closed) 
                {
                    Connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"{e.GetType()}, Error on CloseConnection!", nameof(e));
            }
        }

        [Obsolete]
        public T Query<T>(Func<OracleCommand, T> callback)
        {
            try
            {
                OpenConnection();
                OracleCommand command = Connection.CreateCommand();

                T result = callback(command);

                command.Dispose();
                CloseConnection();

                return result;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"{e.GetType()}, Error on Query!", nameof(e));
            }
        }

        [Obsolete]
        public void Query(Action<OracleCommand> callback)
        {
            this.Query((command) =>
            {
                callback(command);
                return 0;
            });
        }

        [Obsolete]
        public int NonQuery(string sql)
        {
            return this.Query((command) => {
                command.CommandText = sql.Trim();
                command.CommandType = CommandType.Text;
                
                command.Transaction = Connection.BeginTransaction();

                try
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    command.Transaction.Commit();

                    return rowsAffected;
                }
                catch (Exception e)
                {
                    command.Transaction.Rollback();
                    throw e;
                }
            });
        }

        [Obsolete]
        public Dictionary<string, object>[] Query(string sql)
        {
            return this.Query((command) => {
                command.CommandText = sql.Trim();
                command.CommandType = CommandType.Text;

                OracleDataReader reader = command.ExecuteReader();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
                    }

                    rows.Add(row);
                }

                reader.Dispose();

                return rows.ToArray();
            });
        }
    }
}