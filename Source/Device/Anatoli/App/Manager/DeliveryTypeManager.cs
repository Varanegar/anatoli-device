using Anatoli.App.Model;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
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
        public static List<DeliveryTypeModel> GetDeliveryTypes()
        {
            var query = new StringQuery("SELECT * FROM delivery_types");
            query.Unlimited = true;
            return AnatoliClient.GetInstance().DbClient.GetList<DeliveryTypeModel>(query);
        }
        public override int UpdateItem(DeliveryTypeModel model)
        {
            throw new NotImplementedException();
        }
    }
}
