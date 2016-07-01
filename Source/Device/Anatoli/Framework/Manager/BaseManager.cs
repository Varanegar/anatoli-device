using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.Manager
{
    public abstract class BaseManager<DataModel>
        where DataModel : BaseModel, new()
    {
        int _limit = 10;
        DBQuery _query;
        public DBQuery Query
        {
            get
            { return _query; }
            set
            {
                _query = value;
            }
        }
        public virtual List<DataModel> GetNext()
        {
            var list = AnatoliClient.GetInstance().DbClient.GetList<DataModel>(_query);
            _query.Index = list.Count + _query.Index;
            return list;
        }
        public abstract int UpdateItem(DataModel model);
        public int UpdateItems(List<DataModel> models)
        {
            var db = AnatoliClient.GetInstance().DbClient;
            db.BeginTransaction();
            int rows = 0;
            foreach (var model in models)
            {
                var r = UpdateItem(model);
                rows += r;
                if (r == 0)
                {
                    db.RollbackTransaction();
                    return 0;
                }
            }
            db.CommitTransaction();
            return rows;
        }
    }
}
