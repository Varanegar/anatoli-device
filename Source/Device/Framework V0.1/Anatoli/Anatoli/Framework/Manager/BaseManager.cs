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
                throw new ArgumentNullException("Both Local query and remote quesry are null. You have to call SetDBQuery() or SetRemoteQuery()");
            }
            _localP.Limit = _limit;
            _remoteP.Limit = _limit;
            var list = await Task.Run(() => { return dataAdapter.GetList(_localP, _remoteP); });
            _localP.Index += Math.Min(list.Count, _limit);
            _remoteP.Index += Math.Min(list.Count, _limit);
            return list;
        }
        public static async Task<DataModel> GetItemAsync(DBQuery query)
        {
            return await Task.Run(() => { return BaseDataAdapter<DataModel>.GetItem(query, null); });
        }
        public static DataModel GetItem(DBQuery query)
        {
            return BaseDataAdapter<DataModel>.GetItem(query, null);
        }
        public static async Task<bool> LocalUpdate(DBQuery command)
        {
            return await Task.Run(() => { return BaseDataAdapter<DataModel>.LocalUpdate(command); });
        }
    }
}