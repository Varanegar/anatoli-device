using Anatoli.Framework.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.AnatoliBase
{
    public abstract class AnatoliSQLiteClient
    {
        public abstract void Upgrade(int currentVersion, int ollVersion);
        public abstract void Create();
        public abstract void BeginTransaction();
        public abstract void CommitTransaction();
        public abstract void RollbackTransactionTo(string savePoint);
        public abstract void RollbackTransaction();
        public abstract string SaveTransactionPoint();
        public abstract SQLiteConnection GetConnection();
        //public async Task<List<DataModel>> GetListAsync<DataModel>(DBQuery query)
        //    where DataModel : BaseModel
        //{
        //    return await Task.Run(() =>
        //    {
        //        try
        //        {
        //            var connection = GetConnection();
        //            var command = connection.CreateCommand(query.GetCommand());
        //            lock (connection)
        //            {
        //                var result = command.ExecuteQuery<DataModel>();
        //                return result;
        //            }
        //        }
        //        catch (Exception)
        //        {

        //            throw;
        //        }
        //    });

        //}
        public List<DataModel> GetList<DataModel>(DBQuery query)
            where DataModel : BaseModel
        {
            try
            {
                var connection = GetConnection();
                var command = connection.CreateCommand(query.GetCommand());
                lock (connection)
                {
                    var result = command.ExecuteQuery<DataModel>();
                    return result;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        //public async Task<DataModel> GetItemAsync<DataModel>(DBQuery query)
        //    where DataModel : BaseModel
        //{
        //    return await Task.Run((Func<DataModel>)(() =>
        //    {
        //        try
        //        {
        //            var connection = GetConnection();
        //            var command = connection.CreateCommand(query.GetCommand());
        //            List<DataModel> qResult;
        //            lock (connection)
        //            {
        //                qResult = command.ExecuteQuery<DataModel>();
        //            }
        //            if (qResult.Count > 0)
        //            {
        //                return qResult.First();
        //            }
        //            return null;
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }

        //    }));
        //}
        public DataModel GetItem<DataModel>(DBQuery query)
            where DataModel : BaseModel
        {
            try
            {
                var connection = GetConnection();
                var command = connection.CreateCommand(query.GetCommand());
                List<DataModel> qResult;
                lock (connection)
                {
                    qResult = command.ExecuteQuery<DataModel>();
                }
                if (qResult.Count > 0)
                {
                    return qResult.First();
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task<int> UpdateItemAsync(DBQuery query)
        //{
        //    return await Task.Run(() =>
        //    {
        //        try
        //        {
        //            var connection = GetConnection();
        //            var command = connection.CreateCommand(query.GetCommand());
        //            lock (connection)
        //            {
        //                var qResult = command.ExecuteNonQuery();
        //                return qResult;
        //            }

        //        }
        //        catch (Exception)
        //        {

        //            throw;
        //        }
        //    });
        //}
        public int UpdateItem(DBQuery query)
        {
            SQLiteCommand command;
            try
            {
                var connection = GetConnection();
                command = connection.CreateCommand(query.GetCommand());
                lock (connection)
                {
                    var qResult = command.ExecuteNonQuery();
                    return qResult;
                }

            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
