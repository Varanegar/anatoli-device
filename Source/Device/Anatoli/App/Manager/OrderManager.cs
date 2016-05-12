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
    public class OrderManager : BaseManager<OrderModel>
    {
        public static async Task<OrderModel> GetOrderByIdAsync(string orderId)
        {
            try
            {
                SelectQuery query = new SelectQuery("orders_view", new EqFilterParam("order_id", orderId));
                return await BaseDataAdapter<OrderModel>.GetItemAsync(query);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<OrderModel> GetOrderByUniqueIdAsync(string uniqueId)
        {
            try
            {
                SelectQuery query = new SelectQuery("orders_view", new EqFilterParam("UniqueId", uniqueId));
                return await BaseDataAdapter<OrderModel>.GetItemAsync(query);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<List<PurchaseOrderViewModel>> DownloadOrdersAsync(string customerId)
        {
            var data = new RequestModel.PurchaseOrderRequestModel();
            data.customerId = customerId;
            var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrdersList, data);
            return list;
        }

        public static async Task<List<PurchaseOrderStatusHistoryViewModel>> GetOrderHistoryAsync(string customerId, string poId)
        {
            var data = new RequestModel.PurchaseOrderRequestModel();
            data.customerId = customerId;
            data.poId = poId;
            var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderStatusHistoryViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrderHistory, data);
            return list;
        }

        //public static async Task SyncOrderHistoryAsync(string customerId, string poId)
        //{
        //    var data = new RequestModel.PurchaseOrderRequestModel();
        //    data.customerId = customerId;
        //    data.poId = poId;
        //    var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderStatusHistoryViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrderHistory, data);
        //}

        public static async Task<List<PurchaseOrderLineItemViewModel>> DownloadOrderItemsAsync(string customerId, string poId)
        {
            var data = new RequestModel.PurchaseOrderRequestModel();
            data.customerId = customerId;
            data.poId = poId;
            var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderLineItemViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrderItems, data);
            return list;
        }
        public static async Task SyncOrderItemsAsync(string customerId, OrderModel order)
        {
            try
            {
                SelectQuery q = new SelectQuery("order_items", new EqFilterParam("order_id", order.order_id.ToString()));
                var l = await BaseDataAdapter<OrderItemModel>.GetListAsync(q);
                if (l.Count > 0)
                {
                    return;
                }

                var items = await DownloadOrderItemsAsync(customerId, order.UniqueId);
                if (items.Count > 0)
                {

                    List<List<BasicParam>> parametres = new List<List<BasicParam>>();
                    foreach (var item in items)
                    {
                        var p = new List<BasicParam>();
                        p.Add(new BasicParam("order_id", order.order_id.ToString()));
                        p.Add(new BasicParam("product_id", item.ProductId.ToString().ToUpper()));
                        p.Add(new BasicParam("product_count", item.FinalQty.ToString()));
                        p.Add(new BasicParam("item_price", item.FinalNetAmount.ToString()));
                        parametres.Add(p);
                    }
                    InsertAllCommand command2 = new InsertAllCommand("order_items", parametres);
                    var r = await DataAdapter.UpdateItemAsync(command2);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task SyncOrdersAsync(string customerId)
        {
            try
            {
                var orders = await OrderManager.DownloadOrdersAsync(customerId);
                if (orders != null)
                {
                    SelectQuery query = new SelectQuery("orders");
                    var localOrders = await BaseDataAdapter<OrderModel>.GetListAsync(query);
                    Dictionary<long, OrderModel> ordersDict = new Dictionary<long, OrderModel>();
                    foreach (var item in localOrders)
                    {
                        if (!ordersDict.ContainsKey(item.order_id))
                        {
                            ordersDict.Add(item.order_id, item);
                        }
                    }
                    using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                    {
                        connection.BeginTransaction();
                        foreach (var item in orders)
                        {
                            if (ordersDict.ContainsKey(item.AppOrderNo))
                            {
                                UpdateCommand uComand = new UpdateCommand("orders", new EqFilterParam("order_id", item.AppOrderNo.ToString()),
                                    new BasicParam("order_status", item.PurchaseOrderStatusValueId.ToString().ToUpper()));
                                var dbquery = connection.CreateCommand(uComand.GetCommand());
                                int t = dbquery.ExecuteNonQuery();
                            }
                            else
                            {
                                InsertCommand insertCommand = new InsertCommand("orders",
                                    new BasicParam("UniqueId", item.UniqueId.ToUpper()),
                                    new BasicParam("order_price", item.FinalNetAmount.ToString()),
                                    new BasicParam("order_id", item.AppOrderNo.ToString()),
                                    new BasicParam("store_id", item.StoreGuid.ToString().ToUpper()),
                                    new BasicParam("order_status", item.PurchaseOrderStatusValueId.ToString().ToUpper()),
                                new BasicParam("order_date", item.OrderPDate));
                                var dbquery = connection.CreateCommand(insertCommand.GetCommand());
                                int t = dbquery.ExecuteNonQuery();
                            }
                        }
                        connection.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static StringQuery GetOrderQueryString()
        {
            StringQuery query = new StringQuery("SELECT * FROM orders_view");
            return query;
        }
    }
}
