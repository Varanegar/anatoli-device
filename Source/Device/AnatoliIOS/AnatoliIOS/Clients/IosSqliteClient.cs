using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AnatoliIOS.Clients
{
    class IosSqliteClient : AnatoliSQLite
    {
        double _latestVersion;
        double _currentVersion;
        public IosSqliteClient(double latestVersion, double currentVersion)
        {
            _latestVersion = latestVersion;
            _currentVersion = currentVersion;
        }
        public override SQLite.SQLiteConnection GetConnection()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbPath = Path.Combine(documents, "paa.db");
            if (!File.Exists(dbPath) || _latestVersion < _currentVersion)
            {
                if (System.IO.File.Exists("paa.db"))
                {
                    File.Copy("paa.db", dbPath, true);
                }
                else
                {
                    throw new FileNotFoundException("Database file not found");
                }
            }
            _latestVersion = _currentVersion;
            var conn = new SQLite.SQLiteConnection(dbPath);
            return conn;
        }

    }
}
