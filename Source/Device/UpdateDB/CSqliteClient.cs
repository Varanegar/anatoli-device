using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateDB
{
    class CSqliteClient : Anatoli.Framework.AnatoliBase.AnatoliSQLiteClient
    {
        SQLite.SQLiteConnection _connection;
        public override void BeginTransaction()
        {
            _connection.BeginTransaction();
        }

        public override void CommitTransaction()
        {
            _connection.Commit();
        }

        public override void Create()
        {
            
        }

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

        public override void RollbackTransaction()
        {
            _connection.Rollback();
        }

        public override void RollbackTransactionTo(string savePoint)
        {
            _connection.RollbackTo(savePoint);
        }

        public override string SaveTransactionPoint()
        {
            return _connection.SaveTransactionPoint();
        }

        public override void Upgrade(int currentVersion, int ollVersion)
        {
            
        }
    }
}
