using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model.Product;
using Anatoli.Framework.Manager;
using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anatoli.App.Model.Store;
using Anatoli.App.Model;

namespace Anatoli.App.Manager
{
    public class ShoppingCardManager : BaseManager<ProductModel>
    {
        public static bool AddProduct(Guid productId, Guid storeId, int count)
        {
            DBQuery query = null;
            try
            {
                var item = ProductManager.GetItem(productId, storeId);
                if (item == null || !item.IsAvailable)
                    return false;
                if (item.ShoppingBasketCount == 0)
                    query = new InsertCommand("shopping_card", new BasicParam("count", (count).ToString()), new BasicParam("product_id", productId));
                else
                    query = new UpdateCommand("shopping_card", new BasicParam("count", (item.ShoppingBasketCount + count).ToString()), new EqFilterParam("product_id", item.UniqueId.ToString()));
                var result = AnatoliClient.GetInstance().DbClient.UpdateItem(query) > 0 ? true : false;
                if (result)
                {
                    item.ShoppingBasketCount += count;
                    OnItemChanged(item);
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool AddProduct(ProductModel item)
        {
            try
            {
                DBQuery query = null;
                if (item.ShoppingBasketCount == 0)
                    query = new InsertCommand("shopping_card", new BasicParam("count", (item.ShoppingBasketCount + 1).ToString()), new BasicParam("product_id", item.UniqueId.ToString()));
                else
                    query = new UpdateCommand("shopping_card", new BasicParam("count", (item.ShoppingBasketCount + 1).ToString()), new EqFilterParam("product_id", item.UniqueId.ToString()));
                var result = AnatoliClient.GetInstance().DbClient.UpdateItem(query) > 0 ? true : false;
                if (result)
                {
                    item.ShoppingBasketCount++;
                    OnItemChanged(item);
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool RemoveProduct(ProductModel item, bool all = false)
        {
            try
            {
                DBQuery query = null;
                if (item.ShoppingBasketCount <= 1 || all)
                    query = new DeleteCommand("shopping_card", new SearchFilterParam("product_id", item.UniqueId.ToString()));
                else
                    query = new UpdateCommand("shopping_card", new BasicParam("count", (item.ShoppingBasketCount - 1).ToString()), new EqFilterParam("product_id", item.UniqueId.ToString()));
                var result = AnatoliClient.GetInstance().DbClient.UpdateItem(query) > 0 ? true : false;
                if (result)
                    if (all)
                    {
                        item.ShoppingBasketCount = 0;
                        OnItemChanged(item);
                    }
                    else
                    {
                        item.ShoppingBasketCount--;
                        OnItemChanged(item);
                    }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //public async static Task<double> GetTotalPriceAsync()
        //{
        //    try
        //    {
        //        SelectQuery query = new SelectQuery("shopping_card_view");
        //        query.Unlimited = true;
        //        var result = await BaseDataAdapter<ProductModel>.GetListAsync(query);
        //        double p = 0;
        //        foreach (var item in result)
        //        {
        //            p += (item.count * item.price);
        //        }
        //        return p;
        //    }
        //    catch (Exception)
        //    {
        //        return 0;
        //    }
        //}
        public static List<ProductModel> GetAllItems()
        {
            try
            {
                var defaultStore = StoreManager.GetDefault();
                StringQuery query = new StringQuery(string.Format("SELECT * FROM shopping_card_view LEFT JOIN store_onhand ON shopping_card_view.product_id = store_onhand.product_id WHERE store_onhand.store_id = '{0}'", defaultStore.UniqueId));
                query.Unlimited = true;
                var list = AnatoliClient.GetInstance().DbClient.GetList<ProductModel>(query);
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static StringQuery GetAll(string storeId)
        {
            StringQuery query = new StringQuery(string.Format("SELECT * FROM shopping_card_view LEFT JOIN store_onhand ON shopping_card_view.product_id = store_onhand.product_id WHERE store_onhand.store_id = '{0}'", storeId));
            query.Unlimited = false;
            return query;
        }
        public static bool Clear()
        {
            DeleteCommand command = new DeleteCommand("shopping_card");
            var result = (AnatoliClient.GetInstance().DbClient.UpdateItem(command) > 0) ? true : false;
            if (result)
                OnItemsCleared();
            return result;
        }

        public static ShoppingCardModel GetInfo()
        {
            try
            {
                var defaultStore = StoreManager.GetDefault();
                StringQuery query = new StringQuery(string.Format("SELECT SUM(count) as items_count, SUM(price*count) as total_price FROM shopping_card_view JOIN store_onhand ON shopping_card_view.product_id = store_onhand.product_id WHERE store_onhand.store_id = '{0}'", defaultStore.UniqueId));
                query.Unlimited = true;
                var card = AnatoliClient.GetInstance().DbClient.GetItem<ShoppingCardModel>(query);
                return card;
            }
            catch (Exception)
            {
                return new ShoppingCardModel();
            }
        }

        public static bool UpdateProductCount(ProductModel item)
        {
            DBQuery query = null;
            if (item.ShoppingBasketCount == 0)
                query = new DeleteCommand("shopping_card", new SearchFilterParam("product_id", item.UniqueId.ToString()));
            else
                query = new UpdateCommand("shopping_card", new BasicParam("count", (item.ShoppingBasketCount).ToString()), new EqFilterParam("product_id", item.UniqueId.ToString()));
            var result = AnatoliClient.GetInstance().DbClient.UpdateItem(query) > 0 ? true : false;
            if (result)
            {
                OnItemChanged(item);
            }
            return result;
        }

        public static async Task<PurchaseOrderViewModel> CalcPromo(CustomerViewModel customerModel, Guid userId, Guid storeId, Guid deliveryTypeId, DeliveryTimeModel deliveryTime)
        {
            try
            {
                var products = GetAllItems();
                PurchaseOrderViewModel order = new PurchaseOrderViewModel();
                order.Customer = customerModel;
                order.DeliveryTypeId = deliveryTypeId;
                order.PaymentTypeValueId = Guid.Parse("3a27504c-a9ba-46ce-9376-a63403bfe82a");
                order.StoreGuid = storeId;
                order.UserId = userId;
                order.OrderDate = DateTime.Now;
                order.OrderTime = DateTime.Now.TimeOfDay;
                order.DeliveryDate = DateTime.Now;
                order.ActionSourceValueId = "65DEC223-059E-48BA-8281-E4FAAFF6E32D";

                if (deliveryTime != null)
                {
                    order.DeliveryFromTime = deliveryTime.timespan;
                    order.DeliveryToTime = deliveryTime.timespan + TimeSpan.FromMinutes(30);
                }
                foreach (var item in products)
                {
                    PurchaseOrderLineItemViewModel line = new PurchaseOrderLineItemViewModel();
                    line.ProductId = Guid.Parse(item.UniqueId.ToString());
                    line.Qty = item.ShoppingBasketCount;
                    order.LineItems.Add(line);
                }
                var requestModel = new RequestModel.PurchaseOrderRequestModel();
                requestModel.orderEntity = order;
                var o = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<PurchaseOrderViewModel>(TokenType.AppToken, Configuration.WebService.Purchase.CalcPromo, requestModel, false);
                return o;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static async Task<PurchaseOrderViewModel> Checkout(CustomerViewModel customerModel, Guid userId, Guid storeId, Guid deliveryTypeId, DeliveryTimeModel deliveryTime)
        {
            try
            {
                var products = GetAllItems();
                PurchaseOrderViewModel order = new PurchaseOrderViewModel();
                order.Customer = customerModel;
                order.DeliveryTypeId = deliveryTypeId;
                order.PaymentTypeValueId = Guid.Parse("3a27504c-a9ba-46ce-9376-a63403bfe82a");
                order.StoreGuid = storeId;
                order.OrderDate = DateTime.Now;
                order.OrderTime = DateTime.Now.TimeOfDay;
                order.DeliveryDate = DateTime.Now;
                order.ShipAddress = customerModel.MainStreet + customerModel.OtherStreet + customerModel.Address;
                if (deliveryTime != null)
                {
                    order.DeliveryFromTime = deliveryTime.timespan;
                    order.DeliveryToTime = deliveryTime.timespan + TimeSpan.FromMinutes(30);
                }
                order.PurchaseOrderStatusValueId = Guid.Parse("A591658A-E46B-440D-9ADB-E3E5B01B7489");
                order.UserId = userId;
                order.ActionSourceValueId = "65DEC223-059E-48BA-8281-E4FAAFF6E32D";
                foreach (var item in products)
                {
                    PurchaseOrderLineItemViewModel line = new PurchaseOrderLineItemViewModel();
                    line.ProductId = Guid.Parse(item.UniqueId.ToString());
                    line.Qty = item.ShoppingBasketCount;
                    order.LineItems.Add(line);
                }
                var requestModel = new RequestModel.PurchaseOrderRequestModel();
                requestModel.orderEntity = order;
                var o = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<PurchaseOrderViewModel>(TokenType.AppToken, Configuration.WebService.Purchase.Create, requestModel, false);
                return o;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static async Task<bool> ValidateRequest(CustomerViewModel customer)
        {
            if (customer == null)
            {
                throw new ValidationException(ValidationErrorCode.NoLogin);
            }
            if (String.IsNullOrEmpty(customer.FirstName) || String.IsNullOrEmpty(customer.LastName) || String.IsNullOrEmpty(customer.MainStreet) || String.IsNullOrEmpty(customer.NationalCode))
            {
                throw new ValidationException(ValidationErrorCode.CustomerInfo);
            }
            var info = GetInfo();
            if (info != null)
            {
                if (info.Qty == 0)
                {
                    throw new ValidationException(ValidationErrorCode.EmptyBasket);
                }
            }

            return true;
        }

        static void OnItemsCleared()
        {
            if (ItemsCleared != null)
            {
                ItemsCleared.Invoke();
            }
        }

        public static event ItemsClearedEventHandler ItemsCleared;
        public delegate void ItemsClearedEventHandler();
        static void OnItemChanged(ProductModel item)
        {
            if (ItemChanged != null)
            {
                ItemChanged.Invoke(item);
            }
        }

        public static ProductModel GetItem(Guid uniqueId)
        {
            try
            {
                SelectQuery query = new SelectQuery("shopping_card_view", new EqFilterParam("product_id", uniqueId.ToString().ToUpper()));
                return AnatoliClient.GetInstance().DbClient.GetItem<ProductModel>(query);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override int UpdateItem(ProductModel model)
        {
            throw new NotImplementedException();
        }

        public static event ItemChangedEventHandler ItemChanged;
        public delegate void ItemChangedEventHandler(ProductModel item);
    }
    public class ValidationException : Exception
    {
        public ValidationErrorCode Code { get; set; }
        public ValidationException(ValidationErrorCode code)
        {
            Code = code;
        }
    }

    public enum ValidationErrorCode
    {
        NoLogin,
        CustomerInfo,
        EmptyBasket
    }

}
