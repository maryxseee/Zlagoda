using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Zlagoda.Classes
{
    public static class DbHelper
    {
        private static string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=ZlagodaDB;Trusted_Connection=True;";
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    if (parameters != null) cmd.Parameters.AddRange(parameters);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка бази даних: " + ex.Message);
                    return null;
                }
            }
        }

        public static bool ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    if (parameters != null) cmd.Parameters.AddRange(parameters);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при виконанні операції: " + ex.Message);
                    return false;
                }
            }
        }
    }
}
