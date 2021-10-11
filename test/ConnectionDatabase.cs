using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OracleClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Auto_Query_Version1
{
    class ConnectionDatabase
    {
        OracleConnection conn = null;
        string chuoiketnoi = @"Data Source= (DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.159.131.91)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = PDBSXKD)));User Id=sxkd;Password=sxkd@2021;";
        public ConnectionDatabase()
        {
            if (conn == null)
            {
                conn = new OracleConnection();
                conn.ConnectionString = chuoiketnoi;
                conn.Open();
            }
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

        }

        public DataTable Get_Table(string str)
        { 
            OracleCommand command = new OracleCommand();
            command.CommandText = str;
            command.Connection = conn;
            OracleDataReader rd = command.ExecuteReader();
            DataTable tb = new DataTable();
            tb.Load(rd);
            rd.Close();
            return tb;
        }
        public DataTable Get_Table()
        {
            OracleCommand command = new OracleCommand();
            command.CommandText = @"select distinct table_name from all_tab_cols where UPPER(column_name) like '%PHANVUNG_ID%'";
            command.Connection = conn;
            OracleDataReader rd = command.ExecuteReader();
            DataTable tb = new DataTable();
            tb.Load(rd);
            rd.Close();
            return tb;
        }

    
       private DataTable ThucThiSql(string sql)
        {
            OracleCommand command = new OracleCommand();
            command.CommandText = sql + " FETCH NEXT 5 ROWS ONLY";
            command.Connection = conn;
            OracleDataReader rd = command.ExecuteReader();
            DataTable tb = new DataTable();
            tb.Load(rd);
            rd.Close();
            return tb;
        }
        private string XuLyCacBienSql(string bien , int kieu)
        {
            if(kieu == 2)
            {
                // Tách mấy cái biến có dấu :vloaitb_id ra
                int k1 = bien.IndexOf(':');
                bien = bien.ToString().Remove(k1, bien.Length - k1);
                bien = bien.Split('=')[0].ToString().Trim().ToUpper();
                if (bien.IndexOf('.') >= 0)
                {
                    bien = bien.Split('.')[1].ToString().Trim().ToUpper();
                }
                return bien;
            }    
            if(kieu == 1)
            {
                // Tách mấy cái biến có dấu :vloaitb_id ra              
                bien = bien.Split('=')[0].ToString().Trim().ToUpper();         
                return bien;
            }
            return "";
        }
        public string XuLychuoi(string sql)
        {
            try
            {
                DataTable tb = new DataTable();
                string[] mangchuakq = new string[100];

                #region Xử lý selct, where , form đưa vào mảng.
                ////////////////////////////////////////////////////////////////////////////
                string chuoi1 = "";
                string[] select = new string[10];
                string[] from = new string[20];

                sql = sql.Replace('\r', ' ');
                sql = sql.Replace('\n', ' ');
                sql = sql.Replace('\t', ' ');
                sql = sql.Replace(';', ' ');
                sql = sql.Replace('"', ' ');
                string[] chuoi = sql.Split(' ');
                for (int i = 0; i < chuoi.Length; i++)
                {
                    if (chuoi[i].Trim() != "")
                    {
                        chuoi1 = chuoi1 + chuoi[i] + " ";
                    }
                }
                string[] catchuoi = chuoi1.ToLower().Split("from");
                select[0] = catchuoi[0].Trim();
                catchuoi = catchuoi[1].Split("where");
                from[0] = catchuoi[0].Replace("{?db2}", "CSS").Trim();
              //  from[0] = catchuoi[0].Replace("{?db1}", "ADMIN").Trim();
                string[] where = catchuoi[1].Trim().Split("and");
                /////////////////////////////////////////////////////////
                #endregion

                string sqlhoanchinh = "select * from " + from[0] + " where ";
                int k = 0;
                string[] mangchuabien = new string[100];
                // Lấy ra sql hoàn chỉnh chưa chứa các biến truyền vào.
                for (int j = 0; j < where.Length && where[j] != ""; j++)
                {
                    if (where[j].IndexOf(':') < 0)
                    {
                        sqlhoanchinh = sqlhoanchinh + where[j] + " and ";
                    }
                    else
                    {
                        mangchuabien[k] = where[j];
                        sqlhoanchinh = sqlhoanchinh + XuLyCacBienSql(mangchuabien[k], 1) + " is not null " + " and ";
                        k++;
                    }
                }

                // Thực thi thử câu sql, xử lý cái "and ở cuối caai lệnh sql"
                sqlhoanchinh = sqlhoanchinh.Remove(sqlhoanchinh.Length - 4, 4);
                //  Nếu lấy được dữ liệu thì ngon 
                if (ThucThiSql(sqlhoanchinh).Rows.Count <= 0)
                    return "";
                else
                {
                    for (int l = 0; l < mangchuabien.Length; l++)
                    {
                        if (mangchuabien[l] != null)
                        {
                            string l2 = XuLyCacBienSql(mangchuabien[l], 1);
                            string l1 = XuLyCacBienSql(mangchuabien[l], 2);
                            tb = ThucThiSql(sqlhoanchinh);
                        firstPoint:
                            for (int m = 0; m < tb.Rows.Count; m++)
                            {
                                sqlhoanchinh = sqlhoanchinh + " and " + l2.ToLower() + " = '" + tb.Rows[m][l1.ToUpper()] + "'";
                                tb = ThucThiSql(sqlhoanchinh);
                                if (tb.Rows.Count > 0)
                                {
                                    mangchuakq[l] = tb.Rows[m][l1].ToString();
                                    break;
                                }
                                else
                                {
                                    return "";
                                    goto firstPoint;
                                }

                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                string kq = "";
                int loop = 0;
                while (mangchuakq[loop] != null)
                {
                    kq = kq + XuLyCacBienSql(mangchuabien[loop], 1) + " = " + mangchuakq[loop] + Environment.NewLine;
                    loop++;
                }
                return kq + Environment.NewLine + sqlhoanchinh;
            }
            catch(Exception ex)
            {
               
            }                     
                return "";            
        }   
    }
}
