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
    public class PurchaseOrderManager : BaseManager<PurchaseOrderViewModel>
    {
        public static PurchaseOrderViewModel GetOrderByAppOrderNumber(long orderNumber)
        {
            try
            {
                SelectQuery query = new SelectQuery("OrderView", new EqFilterParam("AppOrderNo", orderNumber.ToString()));
                return AnatoliClient.GetInstance().DbClient.GetItem<PurchaseOrderViewModel>(query);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static PurchaseOrderViewModel GetOrderByUniqueId(Guid uniqueId)
        {
            try
            {
                SelectQuery query = new SelectQuery("OrderView", new EqFilterParam("UniqueId", uniqueId));
                return AnatoliClient.GetInstance().DbClient.GetItem<PurchaseOrderViewModel>(query);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<List<PurchaseOrderViewModel>> DownloadOrdersAsync(Guid customerId)
        {
            var data = new RequestModel.PurchaseOrderRequestModel();
            data.customerId = customerId;
            var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrdersList, data, false);
            return list;
        }

        public static async Task<List<PurchaseOrderStatusHistoryViewModel>> GetOrderHistoryAsync(Guid customerId, Guid poId)
        {
            var data = new RequestModel.PurchaseOrderRequestModel();
            data.customerId = customerId;
            data.poId = poId;
            var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderStatusHistoryViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrderHistory, data, false);
            return list;
        }

        //public static async Task SyncOrderHistoryAsync(string customerId, string poId)
        //{
        //    var data = new RequestModel.PurchaseOrderRequestModel();
        //    data.customerId = customerId;
        //    data.poId = poId;
        //    var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderStatusHistoryViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrderHistory, data);
        //}

        public static async Task<List<PurchaseOrderLineItemViewModel>> DownloadOrderItemsAsync(Guid customerId, Guid poId)
        {
            var data = new RequestModel.PurchaseOrderRequestModel();
            data.customerId = customerId;
            data.poId = poId;
            var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<PurchaseOrderLineItemViewModel>>(TokenType.AppToken, Configuration.WebService.Purchase.OrderItems, data, false);
            return list;
        }
        public static async Task SyncOrderItemsAsync(Guid customerId, PurchaseOrderViewModel order)
        {
            try
            {
                SelectQuery q = new SelectQuery("PurchaseOrderItem", new EqFilterParam("PurchaseOrderId", order.UniqueId ));
                var l = AnatoliClient.GetInstance().DbClient.GetList<PurchaseOrderLineItemViewModel>(q);
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
                        p.Add(new BasicParam("UniqueId", item.UniqueId));
                        p.Add(new BasicParam("PurchaseOrderId", order.UniqueId));
                        p.Add(new BasicParam("ProductId", item.ProductId));
                        p.Add(new BasicParam("Qty", item.Qty.ToString()));
                        p.Add(new BasicParam("FinalQty", item.FinalQty.ToString()));
                        p.Add(new BasicParam("FinalNetAmount", item.FinalNetAmount.ToString()));
                        p.Add(new BasicParam("NetAmount", item.NetAmount.ToString()));
                        parametres.Add(p);
                    }
                    InsertAllCommand command2 = new InsertAllCommand("PurchaseOrderItem", parametres);
                    var r = AnatoliClient.GetInstance().DbClient.UpdateItem(command2);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task SyncOrdersAsync(Guid customerId)
        {
            try
            {
                var orders = await PurchaseOrderManager.DownloadOrdersAsync(customerId);
                if (orders != null)
                {
                    SelectQuery query = new SelectQuery("PurchaseOrder");
                    var localOrders = AnatoliClient.GetInstance().DbClient.GetList<PurchaseOrderViewModel>(query);
                    Dictionary<long, PurchaseOrderViewModel> ordersDict = new Dictionary<long, PurchaseOrderViewModel>();
                    foreach (var item in localOrders)
                    {
                        if (!ordersDict.ContainsKey(item.AppOrderNo))
                        {
                            ordersDict.Add(item.AppOrderNo, item);
                        }
                    }

                    AnatoliClient.GetInstance().DbClient.BeginTransaction();
                    foreach (var item in orders)
                    {
                        if (ordersDict.ContainsKey(item.AppOrderNo))
                        {
                            UpdateCommand uComand = new UpdateCommand("PurchaseOrder", new EqFilterParam("AppOrderNo", item.AppOrderNo.ToString()),
                                new BasicParam("PurchaseOrderStatusValueId", item.PurchaseOrderStatusValueId));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(uComand);
                        }
                        else
                        {
                            InsertCommand insertCommand = new InsertCommand("PurchaseOrder",
                                new BasicParam("UniqueId", item.UniqueId),
                                new BasicParam("AppOrderNo", item.AppOrderNo.ToString()),
                                new BasicParam("StoreGuid", item.StoreGuid),
                                new BasicParam("PurchaseOrderStatusValueId", item.PurchaseOrderStatusValueId),
                                new BasicParam("PaymentTypeValueId", item.PaymentTypeValueId),
                                new BasicParam("OrderDate", item.OrderDate.ToString()),
                                new BasicParam("OrderTime", item.OrderTime.ToString()),
                                new BasicParam("OrderPDate", item.OrderPDate),
                                new BasicParam("FinalAmount", item.FinalAmount.ToString()),
                                new BasicParam("ActionSourceValueId", item.ActionSourceValueId),
                                new BasicParam("ShipAddress", item.ShipAddress),
                                new BasicParam("DeliveryTypeId", item.DeliveryTypeId),
                                new BasicParam("FinalNetAmount", item.FinalNetAmount.ToString()),
                            new BasicParam("OrderPDate", item.OrderPDate));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(insertCommand);
                        }
                    }
                    AnatoliClient.GetInstance().DbClient.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static StringQuery GetOrderQueryString()
        {
            StringQuery query = new StringQuery("SELECT * FROM OrdersView ORDER BY AppOrderNo DESC");
            return query;
        }


        public override int UpdateItem(PurchaseOrderViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
