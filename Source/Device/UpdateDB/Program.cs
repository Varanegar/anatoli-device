using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Syncronising Anatoli database started...");
            try
            {
                AnatoliClient.Initialize(new CWebClient(), new CSqliteClient(), new CFileIO());
                Console.WriteLine("Data base connection established successfully.");
                SyncManager.ProgressChanged += (status, step) => { Console.WriteLine(status); };
                SyncManager.SyncDatabase().Wait();
                Console.WriteLine("Syncronizing anatoli database finished successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Syncdronization Anatoli database failed.  error: " + ex.Message);
            }
            finally
            {
                Console.Read();
            }
        }
    }
}
