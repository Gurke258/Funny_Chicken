using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Funny_Chicken.Database
{
    public class SQLiteCon
    {
        public void SQLLiteConnection(string query)
        {
            string connectionString = "Data Source=highscore.db;Version=3;";
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();
            connection.Close();

        }
    }
}
