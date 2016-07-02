using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Anatoli.Framework.AnatoliBase;
using System.IO;
using SQLite;
using AnatoliAndroid.Activities;
using AnatoliAndroid;

namespace AnatoliAndroid
{
    class SQLiteAndroid : AnatoliSQLiteClient
    {
        SQLiteConnection _connection;
        string _fileName = "paa.db";
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
            var path = FileAccessHelper.GetLocalFilePath(_fileName);
            FileAccessHelper.CopyDatabase(path, _fileName);
        }

        public override SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                string path = FileAccessHelper.GetLocalFilePath(_fileName);
                _connection = new SQLite.SQLiteConnection(path);
            }
            return _connection;
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
            Create();
        }

        public class FileAccessHelper
        {
            public static string GetLocalFilePath(string filename)
            {
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string dbPath = Path.Combine(path, filename);
                return dbPath;
            }

            public static void CopyDatabase(string dbPath, string fileName)
            {
                using (var br = new BinaryReader(Application.Context.Assets.Open(fileName)))
                {
                    using (var bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create)))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, length);
                        }
                    }
                }
            }
        }
    }
}