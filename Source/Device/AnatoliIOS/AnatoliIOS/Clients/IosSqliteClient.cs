using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AnatoliIOS.Clients
{
    class IosSqliteClient : AnatoliSQLite
    {
        public override SQLite.SQLiteConnection GetConnection()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbPath = Path.Combine(documents, "paa.db");
            if (!File.Exists(dbPath))
            {
                if (System.IO.File.Exists("paa.db"))
                {
                    File.Copy("paa.db", dbPath);
                }
                else
                {
                    throw new FileNotFoundException("Database file not found");
                }
            }
            var conn = new SQLite.SQLiteConnection(dbPath);
            return conn;
        }

    }
}
