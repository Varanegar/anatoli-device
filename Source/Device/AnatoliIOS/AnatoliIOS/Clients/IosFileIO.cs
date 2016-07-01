using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnatoliIOS.Clients
{
    class IosFileIO : IFileIO
    {
        public string GetDataLoction()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public string GetInternetCacheLoction()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
        }

        public bool WriteAllText(string content, string path, string fileName)
        {
            string filePath = System.IO.Path.Combine(path, fileName);
            try
            {
                System.IO.File.WriteAllText(filePath, content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool WriteAllBytes(byte[] content, string path, string fileName)
        {
            string filePath = System.IO.Path.Combine(path, fileName);
            try
            {
                System.IO.File.WriteAllBytes(filePath, content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string ReadAllText(string path, string fileName)
        {
            string filePath = System.IO.Path.Combine(path, fileName);
            return System.IO.File.ReadAllText(filePath);
        }

        public byte[] ReadAllBytes(string path, string fileName)
        {
            try
            {
                string filePath = System.IO.Path.Combine(path, fileName);
                var bytes = System.IO.File.ReadAllBytes(filePath);
                return bytes;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public byte[] ReadAllBytes(string filePath)
        {
            return System.IO.File.ReadAllBytes(filePath);
        }

        public void DeleteFile(string path, string fileName)
        {
            System.IO.File.Delete(System.IO.Path.Combine(path, fileName));
        }
    }
}
