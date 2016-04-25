using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateDB
{
    class CFileIO : IFileIO
    {
        public string GetDataLoction()
        {
            return Directory.GetCurrentDirectory();
        }

        public string GetInternetCacheLoction()
        {
            return Directory.GetCurrentDirectory();
        }

        public bool WriteAllText(string content, string path, string fileName)
        {
            throw new NotImplementedException();
        }

        public bool WriteAllBytes(byte[] content, string path, string fileName)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path, string fileName)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadAllBytes(string path, string fileName)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadAllBytes(string filePath)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
