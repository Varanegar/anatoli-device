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
    public class OrderItemsManager : BaseManager<PurchaseOrderLineItemViewModel>
    {
        public static List<PurchaseOrderLineItemViewModel> GetItems(Guid orderId)
        {
            try
            {
                StringQuery query = new StringQuery(String.Format("SELECT * FROM OrderItemView WHERE PurchaseOrderId = '{0}'", orderId));
                return AnatoliClient.GetInstance().DbClient.GetList<PurchaseOrderLineItemViewModel>(query);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public override int UpdateItem(PurchaseOrderLineItemViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
