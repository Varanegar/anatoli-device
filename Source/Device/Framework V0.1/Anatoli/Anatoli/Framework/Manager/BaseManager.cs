﻿using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.DataAdapter;
using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.Manager
{
    public abstract class BaseManager<DataAdapter, DataModel>
        where DataAdapter : BaseDataAdapter<DataModel>, new()
        where DataModel : BaseDataModel, new()
    {
        protected DataAdapter dataAdapter = null;
        int _limit = 10;
        protected DBQuery _localP;
        protected RemoteQuery _remoteP;
        public int Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }
        protected BaseManager()
        {
            dataAdapter = new DataAdapter();
        }

        public bool IsIdValid(string id)
        {
            return true;
        }
        public void SetQueries(DBQuery dbQuery, RemoteQuery remoteQuery)
        {
            _localP = dbQuery;
            _remoteP = remoteQuery;
        }

        public async Task<List<DataModel>> GetNextAsync()
        {
            if (_localP == null && _remoteP == null)
            {
                throw new ArgumentNullException();
            }
            if (_localP != null)
            {
                _localP.Limit = _limit;
            }
            if (_remoteP != null)
            {
                _remoteP.Limit = _limit;
            }
            var list = await Task.Run(() => { return dataAdapter.GetListAsync(_localP, _remoteP); });
            if (_localP != null)
            {
                _localP.Index += Math.Min(list.Count, _limit);
            }
            if (_remoteP != null)
            {
                _remoteP.Index += Math.Min(list.Count, _limit);
            }
            return list;
        }
        //public static List<DataModel> GetList(DBQuery dbQuery, RemoteQuery remoteQuery)
        //{
        //    if (dbQuery == null && remoteQuery == null)
        //    {
        //        throw new ArgumentNullException();
        //    }
        //    var list = await BaseDataAdapter<DataModel>.GetListStaticAsync(dbQuery, remoteQuery);
        //    return list;
        //}
        public static async Task<List<DataModel>> GetListAsync(DBQuery dbQuery, RemoteQuery remoteQuery)
        {
            if (dbQuery == null && remoteQuery == null)
            {
                throw new ArgumentNullException();
            }
            var list = await BaseDataAdapter<DataModel>.GetListStaticAsync(dbQuery, remoteQuery);
            return list;
        }
        public static async Task<DataModel> GetItemAsync(DBQuery query)
        {
            return await BaseDataAdapter<DataModel>.GetItemAsync(query, null);
        }
        //public static DataModel GetItem(DBQuery query)
        //{
        //    return await BaseDataAdapter<DataModel>.GetItemAsync(query, null);
        //}
        /// <summary>
        /// Runs a query and saves data to the locl sqlite database
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static async Task<int> LocalUpdateAsync(DBQuery command)
        {
            return await Task.Run(() => { return BaseDataAdapter<DataModel>.UpdateItemStatic(command, null); });
        }
    }
}
