using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PDK.AdvSql
{
    public class AdvSql
    {
        public SqlConnection SqlConnection { get; internal set; }
        public string ConnectionString { get; internal set; }
        public AdvSql() => Setup(null, null);
        public AdvSql(string connectionString) => Setup(null, connectionString);
        public AdvSql(SqlConnection sqlConnection) => Setup(sqlConnection, null);
        void Setup(SqlConnection sqlConnection, string connectionString)
        {
            ConnectionString = connectionString;
            SqlConnection = sqlConnection;
        }
        public SqlConnection Connection()
        {
            if (SqlConnection != null)
                return SqlConnection;
            else
            {
                SqlConnection = new SqlConnection(ConnectionString);
                return SqlConnection;
            }
        }
        public bool ConnectionOpen(SqlConnection sqlConnection)
        {
            try
            {
                switch (CheckState(sqlConnection))
                {
                    case IsConnectionUsable.Openable:
                        sqlConnection.Open();
                        return true;
                    case IsConnectionUsable.Busy:
                        if (sqlConnection.State == ConnectionState.Connecting || sqlConnection.State == ConnectionState.Executing || sqlConnection.State == ConnectionState.Fetching)
                            return WaitProccess(sqlConnection) && ConnectionOpen(sqlConnection);
                        else
                            return ConnectionOpen(sqlConnection);
                    case IsConnectionUsable.NeedRestart:
                        sqlConnection.Close();
                        sqlConnection.Open();
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void ConnectionClose() => ConnectionClose(Connection());
        public void ConnectionClose(SqlConnection sqlConnection)
        {
            switch (CheckState(sqlConnection))
            {
                case IsConnectionUsable.Openable:
                    return;
                case IsConnectionUsable.Busy:
                    if (WaitProccess(sqlConnection))
                        sqlConnection.Close();
                    break;
                case IsConnectionUsable.NeedRestart:
                    sqlConnection.Close();
                    break;
            }
        }
        public IsConnectionUsable CheckState(SqlConnection sqlConnection)
        {
            switch (sqlConnection.State)
            {
                case ConnectionState.Closed: return IsConnectionUsable.Openable;
                case ConnectionState.Open: return IsConnectionUsable.Openable;
                case ConnectionState.Connecting: return IsConnectionUsable.Busy;
                case ConnectionState.Executing: return IsConnectionUsable.Busy;
                case ConnectionState.Fetching: return IsConnectionUsable.Busy;
                case ConnectionState.Broken: return IsConnectionUsable.NeedRestart;
                default: return IsConnectionUsable.None;
            };
        }
        public bool WaitProccess(SqlConnection sqlConnection)
        {
            bool isChange = false;

            void stateChange(object s, StateChangeEventArgs e)
            {
                if (sqlConnection.State != ConnectionState.Connecting && sqlConnection.State != ConnectionState.Executing && sqlConnection.State != ConnectionState.Fetching)
                    isChange = true;
            }

            sqlConnection.StateChange += stateChange;

            var time = Stopwatch.StartNew();

            while (time.ElapsedMilliseconds < sqlConnection.ConnectionTimeout)
                if (isChange)
                    break;

            time.Stop();

            sqlConnection.StateChange -= stateChange;

            return isChange;
        }
        public SqlCommand Commander(string query, SqlConnection sqlConnection) => new SqlCommand(query, sqlConnection);
        public T CloseMiddleWare<T>(Func<SqlConnection, T> func, SqlConnection sqlConnection)
        {
            T t = func.Invoke(sqlConnection);

            ConnectionClose(sqlConnection);

            return t;
        }
        public SqlDataAdapter Adapter(string query) => Adapter(query, Connection());
        public SqlDataAdapter Adapter(string query, SqlConnection sqlConnection)
        {
            try
            {
                if (!ConnectionOpen(sqlConnection))
                    return null;

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                return sqlDataAdapter;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public SqlDataReader Reader(string query) => Reader(query, Connection());
        public SqlDataReader Reader(string query, SqlConnection sqlConnection)
        {
            try
            {
                if (!ConnectionOpen(sqlConnection))
                    return null;

                SqlDataReader sqlDataReader = Commander(query, sqlConnection).ExecuteReader();

                return sqlDataReader;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public int NonQuery(string query) => NonQuery(query, Connection());
        public int NonQuery(string query, SqlConnection sqlConnection)
        {
            try
            {
                if (!ConnectionOpen(sqlConnection))
                    return -1;

                int affectedRows = Commander(query, sqlConnection).ExecuteNonQuery();

                ConnectionClose(sqlConnection);

                return affectedRows;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public object Scalar(string query) => Scalar(query, Connection());
        public object Scalar(string query, SqlConnection sqlConnection)
        {
            try
            {
                if (!ConnectionOpen(sqlConnection))
                    return default;

                object returnValue = Commander(query, sqlConnection).ExecuteScalar();

                ConnectionClose(sqlConnection);

                return returnValue;
            }
            catch (Exception)
            {
                return default;
            }
        }
        public DataSet AdapterToDataSet(SqlDataAdapter sqlDataAdapter)
        {
            try
            {
                DataSet dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet);

                return dataSet;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataSet ReaderToDataSet(SqlDataReader sqlDataReader)
        {
            try
            {
                DataSet dataSet = new DataSet();

                while (!sqlDataReader.IsClosed)
                    dataSet.Tables.Add().Load(sqlDataReader);

                return dataSet;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataTable AdapterToDataTable(SqlDataAdapter sqlDataAdapter)
        {
            try
            {
                DataTable dataTable = new DataTable();

                sqlDataAdapter.Fill(dataTable);

                return dataTable;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataTable ReaderToDataTable(SqlDataReader sqlDataReader)
        {
            try
            {
                DataTable dataTable = new DataTable();

                if (!sqlDataReader.IsClosed)
                    dataTable.Load(sqlDataReader);

                return dataTable;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public DataSet AdapterToDataSet(string query) => AdapterToDataSet(CloseMiddleWare((c) => Adapter(query, c), Connection()));
        public DataSet AdapterToDataSet(string query, SqlConnection sqlConnection) => AdapterToDataSet(CloseMiddleWare((c) => Adapter(query, c), sqlConnection));
        public DataSet ReaderToDataSet(string query) => ReaderToDataSet(CloseMiddleWare((c) => Reader(query, c), Connection()));
        public DataSet ReaderToDataSet(string query, SqlConnection sqlConnection) => ReaderToDataSet(CloseMiddleWare((c) => Reader(query, c), sqlConnection));
        public DataTable AdapterToDataTable(string query) => AdapterToDataTable(CloseMiddleWare((c) => Adapter(query, c), Connection()));
        public DataTable AdapterToDataTable(string query, SqlConnection sqlConnection) => AdapterToDataTable(CloseMiddleWare((c) => Adapter(query, c), sqlConnection));
        public DataTable ReaderToDataTable(string query) => ReaderToDataTable(CloseMiddleWare((c) => Reader(query, c), Connection()));
        public DataTable ReaderToDataTable(string query, SqlConnection sqlConnection) => ReaderToDataTable(CloseMiddleWare((c) => Reader(query, c), sqlConnection));
        public IEnumerable<T> ReaderToT<T>(string query) where T : class, new() => ReaderToT<T>(CloseMiddleWare((c) => Reader(query, c), Connection()));
        public IEnumerable<T> ReaderToT<T>(string query, SqlConnection sqlConnection) where T : class, new() => ReaderToT<T>(CloseMiddleWare((c) => Reader(query, c), sqlConnection));
        public IEnumerable<T> ReaderToT<T>(SqlDataReader sqlDataReader) where T : class, new()
        {
            List<T> list = new List<T>();

            try
            {

                List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

                while (!sqlDataReader.IsClosed && sqlDataReader.Read())
                {
                    T t = new T();

                    for (int a = 0; a < sqlDataReader.FieldCount; a++)
                    {
                        string columnName = sqlDataReader.GetName(a);

                        PropertyInfo property = properties.FirstOrDefault(p => p.Name == columnName);

                        if (property != null && sqlDataReader.GetFieldType(a) == property.PropertyType)
                            property.SetValue(t, sqlDataReader[a]);
                    }

                    list.Add(t);
                }
            }
            catch (Exception)
            {
            }

            return list;
        }
    }
    public enum IsConnectionUsable
    {
        None,
        Openable,
        Busy,
        NeedRestart
    }
}