using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.DataAdapter;
using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.Manager
{
    public abstract class BaseManager<DataModel>
        where DataModel : BaseViewModel, new()
    {
        int _limit = 50;
        protected DBQuery _localP;
        protected RemoteQuery _remoteP;
        public int Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }
        public void Reset()
        {
            if (_localP != null)
                _localP.Index = 0;
            if (_remoteP != null)
                _remoteP.Index = 0;
        }
        public void SetQueries(DBQuery dbQuery, RemoteQuery remoteQuery)
        {
            _localP = dbQuery;
            _remoteP = remoteQuery;
        }
        public static async Task<DataModel> GetItemAsync(DBQuery query)
        {
            return await Anatoli.Framework.DataAdapter.BaseDataAdapter<DataModel>.GetItemAsync(query);
        }
        public static async Task<DataModel> GetItemAsync(RemoteQuery query)
        {
            return await Anatoli.Framework.DataAdapter.BaseDataAdapter<DataModel>.GetItemAsync(query);
        }

        public virtual async Task<List<DataModel>> GetNextAsync()
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
            var list = await BaseDataAdapter<DataModel>.GetListAsync(_localP);
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
    }
}
