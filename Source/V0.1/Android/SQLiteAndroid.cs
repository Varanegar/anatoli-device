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
using AnatoliaLibrary.anatoliaclient;
using System.IO;

namespace AnatoliaAndroid
{
    class SQLiteAndroid : AnatoliaSQLite
    {
        public override SQLite.SQLiteConnection GetConnection()
        {
            var sqliteFileName = "Test.db3";
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, sqliteFileName);
            var conn = new SQLite.SQLiteConnection(path);
            return conn;
        }
    }
}