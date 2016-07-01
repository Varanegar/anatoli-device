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
    public class OrderItemsManager : BaseManager<OrderItemModel>
    {
        public static List<OrderItemModel> GetItems(string orderId)
        {
            try
            {
                StringQuery query = new StringQuery(String.Format(@"SELECT Order.UniqueId as order_id,
order_items.item_price as item_price,
products.product_name as product_name,
products.image as image,
products.favorit as favorit,
order_items.product_count as item_count,
order_items.product_id as product_id
FROM
orders JOIN order_items ON orders.order_id = order_items.order_id
JOIN stores ON orders.store_id = stores.store_id
JOIN products ON order_items.product_id = products.product_id 
WHERE orders.order_id = {0}", orderId));
                return AnatoliClient.GetInstance().DbClient.GetList<OrderItemModel>(query);
            }
            catch (Exception)
            {

                return null;
            }
        }


        public override int UpdateItem(OrderItemModel model)
        {
            throw new NotImplementedException();
        }
    }
}
