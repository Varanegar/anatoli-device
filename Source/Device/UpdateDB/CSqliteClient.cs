using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateDB
{
    class CSqliteClient : Anatoli.Framework.AnatoliBase.AnatoliSQLite
    {
        public override SQLiteConnection GetConnection()
        {
#if (DEBUG)
            var currentPath = Environment.CurrentDirectory;
            DirectoryInfo info = new DirectoryInfo(currentPath);
            var folder = info.Parent;
            folder = folder.Parent;
            folder = folder.Parent;
            string path = Path.Combine(folder.FullName, "db", "paa.db");
#endif
#if(!DEBUG)
            string path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).ToString() , "db\\paa.db");
#endif
            var conn = new SQLiteConnection(path);
            return conn;
        }
    }
}
