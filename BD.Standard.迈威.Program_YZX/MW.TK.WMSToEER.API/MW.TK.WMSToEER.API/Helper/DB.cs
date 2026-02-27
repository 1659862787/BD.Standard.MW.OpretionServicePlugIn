using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.TK.WMSToERP.API
{
    public class DB
    {
        SqlConnection con;

        public DB(string conString)
        {
            //string conString = ConfigurationManager.AppSettings["con"];

            con = new SqlConnection(conString);
            con.Open();
        }

        public DataSet getDataSet(string sql)
        {


            SqlDataAdapter sda = new SqlDataAdapter(sql, con);
            sda.SelectCommand.CommandTimeout = 300;

            DataSet ds = new DataSet();
            sda.Fill(ds);
            return ds;


        }

        public void exeSql(string sql)
        {
            //WEB.writestr(sql, "exeSql");

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();

        }

        public void exeSql(string sql, SqlParameter[] s)
        {
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 300;
            cmd.CommandType = CommandType.StoredProcedure;
            if (s != null)
                cmd.Parameters.AddRange(s);
            cmd.ExecuteNonQuery();


        }

        public void Dispose()
        {
            con.Close();
            con.Dispose();
        }

        public void InsertDBdata(DataTable setds, string tableName)
        {
            string sql = "";
            try
            {


                foreach (DataRow dr in setds.Rows)
                {
                    sql = "insert into " + tableName + "(";
                    for (int q = 0; q < setds.Columns.Count; q++)
                    {
                        sql += setds.Columns[q].ColumnName;
                        sql += ",";
                        //sbc.ColumnMappings.Add(setds.Columns[q].ColumnName, setds.Columns[q].ColumnName);

                    }
                    sql = sql.Remove(sql.Length - 1, 1);
                    sql += ") select ";

                    for (int q = 0; q < setds.Columns.Count; q++)
                    {
                        sql += "'" + dr[setds.Columns[q].ColumnName].ToString().Replace("'", "") + "'";
                        sql += ",";
                        //sbc.ColumnMappings.Add(setds.Columns[q].ColumnName, setds.Columns[q].ColumnName);

                    }

                    sql = sql.Remove(sql.Length - 1, 1);

                    exeSql(sql);

                }
                //sbc.WriteToServer(setds);


            }
            catch (Exception ex)
            {
                //WEB.writestr(ex.Message, "InsertDBdata");
            }

        }


        public int IsExists(string sql)
        {

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.CommandType = CommandType.Text;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
