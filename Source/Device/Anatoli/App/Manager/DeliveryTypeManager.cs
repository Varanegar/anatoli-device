using Anatoli.App.Model;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.DataAdapter;
using Anatoli.Framework.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Manager
{
    public class DeliveryTypeManager : BaseManager<DeliveryTypeModel>
    {
        public static async Task<List<DeliveryTypeModel>> GetDeliveryTypesAsync()
        {
            var query = new StringQuery("SELECT * FROM delivery_types");
            query.Unlimited = true;
            return await BaseDataAdapter<DeliveryTypeModel>.GetListAsync(query);
        }
    }
}
