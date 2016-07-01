using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.AnatoliBase
{
    public class AnatoliClient
    {

        AnatoliWebClient _webClient;
        public AnatoliWebClient WebClient
        {
            get { return _webClient; }
        }
        AnatoliSQLiteClient _sqlite;
        public AnatoliSQLiteClient DbClient
        {
            get { return _sqlite; }
        }
        IFileIO _fileIO;
        public IFileIO FileClient
        {
            get { return _fileIO; }
        }

        private static AnatoliClient _instance;
        public static AnatoliClient GetInstance()
        {
            if (_instance == null)
                throw new NullReferenceException("AnatoliClient is null. You should run GetInstance(AnatoliWebClient webClient, AnatoliSQLite sqlite, IFileIO fileIO) for the first time");
            return _instance;
        }
        public static AnatoliClient Initialize(AnatoliWebClient webClient, AnatoliSQLiteClient sqlite, IFileIO fileIO)
        {
            if (_instance == null)
                _instance = new AnatoliClient(webClient, sqlite, fileIO);

            return _instance;

        }
        private AnatoliClient(AnatoliWebClient webClient, AnatoliSQLiteClient sqlite, IFileIO fileIO)
        {
            _webClient = webClient;
            _sqlite = sqlite;
            _fileIO = fileIO;
        }
    }
}
