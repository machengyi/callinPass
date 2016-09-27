using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// projectDal 的摘要说明
/// </summary>
public class projectDal
{
    /// <summary>
    /// 获取数据库数据
    /// </summary>
    /// <param name="sql">sql语句</param>
    /// <param name="conn">数据库链接字符串</param>
    /// <returns>dt返回的数据表</returns>
    public static DataTable GetDataTable(string sql, string conn)
    {
        DataTable dt = new DataTable();
        SqlDataAdapter sda = new SqlDataAdapter();
        SqlCommand sc = new SqlCommand();
        SqlConnection connAcp = new SqlConnection(conn);

        sc.CommandText = sql;
        sc.Connection = connAcp;
        sda.SelectCommand = sc;
        sda.Fill(dt);
        return dt;
    }
}