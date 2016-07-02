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
using System.Net.Http;

namespace Anatoli.App.Manager
{
    public class ProductManager : BaseManager<ProductModel>
    {
        public static ProductModel GetItem(Guid productId, Guid storeId)
        {
            var query = new StringQuery(string.Format(@"
                   SELECT 
a.UniqueId as UniqueId,
a.StoreProductName as StoreProductName,
a.ProductName as ProductName,
a.ProductGroupId as ProductGroupId,
a.Price as Price,
a.StoreGuid as StoreGuid,
a.ImageAddress as ImageAddress,
a.GroupName as GroupName,
a.Qty as Qty,
a.FavoritBasketCount as FavoritBasketCount,
b.ShoppingBasketCount as ShoppingBasketCount
FROM 
(SELECT
Product.UniqueId as UniqueId,
Product.StoreProductName as StoreProductName,
Product.ProductName as ProductName,
Product.ProductGroupId as ProductGroupId,
ProductPrice.Price as Price,
ProductPrice.StoreGuid as StoreGuid,
Product.Image as ImageAddress,
ProductGroup.GroupName as GroupName,
StoreOnhand.Qty as Qty,
BasketItem.Qty as FavoritBasketCount
FROM Product LEFT JOIN ProductPrice ON Product.UniqueId = ProductPrice.ProductGuid AND ProductPrice.StoreGuid = '{0}'
LEFT JOIN BasketItem ON Product.UniqueId = BasketItem.ProductId AND BasketItem.BasketTypeValueId = 'AE5DE00D-3391-49FE-985B-9DA7045CDB13'
LEFT JOIN ProductGroup ON Product.ProductGroupId = ProductGroup.UniqueId
LEFT JOIN StoreOnhand ON Product.UniqueId = StoreOnhand.ProductGuid AND StoreOnhand.StoreGuid = '{0}'
WHERE ProductPrice.Price != 0 ) as a
INNER JOIN 
(SELECT
Product.UniqueId as UniqueId,
BasketItem.Qty as ShoppingBasketCount
FROM Product LEFT JOIN ProductPrice ON Product.UniqueId = ProductPrice.ProductGuid AND ProductPrice.StoreGuid = '{0}'
LEFT JOIN BasketItem ON Product.UniqueId = BasketItem.ProductId AND BasketItem.BasketTypeValueId = 'F6CE03E2-8A2A-4996-8739-DA9C21EAD787'
LEFT JOIN ProductGroup ON Product.ProductGroupId = ProductGroup.UniqueId
LEFT JOIN StoreOnhand ON Product.UniqueId = StoreOnhand.ProductGuid AND StoreOnhand.StoreGuid = '{0}'
WHERE ProductPrice.Price != 0 ) as b
ON a.UniqueId = b.UniqueId AND a.UniqueId = '{1}'", storeId.ToString(), productId.ToString()));
            return AnatoliClient.GetInstance().DbClient.GetItem<ProductModel>(query);
        }
        public static async Task SyncProductsAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.ProductTbl);
                List<ProductModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Products.ProductsList, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductModel>>(TokenType.AppToken, Configuration.WebService.Products.ProductsListAfter, data, true);
                }
                Dictionary<Guid, ProductModel> items = new Dictionary<Guid, ProductModel>();
                var currentList = AnatoliClient.GetInstance().DbClient.GetList<ProductModel>(new StringQuery("SELECT * FROM Product"));
                foreach (var item in currentList)
                {
                    items.Add(item.UniqueId, item);
                }
                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list)
                {
                    if (items.ContainsKey(item.UniqueId))
                    {
                        var currentValue = items[item.UniqueId];

                        UpdateCommand command = new UpdateCommand("Product", new EqFilterParam("UniqueId", item.UniqueId),
                            new BasicParam("ProductName", item.ProductName),
                            new BasicParam("StoreProductName", item.StoreProductName),
                            new BasicParam("IsRemoved", (item.IsRemoved == true) ? "1" : "0"),
                            new BasicParam("ProductGroupId", item.ProductGroupId.ToString().ToUpper()));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);

                    }
                    else
                    {
                        InsertCommand command = new InsertCommand("Product", new BasicParam("UniqueId", item.UniqueId),
                         new BasicParam("ProductName", item.ProductName),
                            new BasicParam("StoreProductName", item.StoreProductName),
                            new BasicParam("IsRemoved", (item.IsRemoved == true) ? "1" : "0"),
                            new BasicParam("ProductGroupId", item.ProductGroupId.ToString().ToUpper()));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.ProductTbl);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task SyncPricesAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.PriceTbl);
                List<ProductPriceModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductPriceModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Stores.PricesView, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductPriceModel>>(TokenType.AppToken, Configuration.WebService.Stores.PricesViewAfter, data, true);
                }
                Dictionary<string, ProductPriceModel> items = new Dictionary<string, ProductPriceModel>();
                var currentList = AnatoliClient.GetInstance().DbClient.GetList<ProductPriceModel>(new StringQuery("SELECT * FROM ProductPrice"));
                foreach (var item in currentList)
                {
                    items.Add(item.ProductGuid.ToString().ToUpper() + item.StoreGuid.ToString().ToUpper(), item);
                }
                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list)
                {
                    if (items.ContainsKey(item.ProductGuid.ToString().ToUpper() + item.StoreGuid.ToString().ToUpper()))
                    {
                        var currentValue = items[item.ProductGuid.ToString().ToUpper() + item.StoreGuid.ToString().ToUpper()];
                        if (items[item.ProductGuid.ToString().ToUpper() + item.StoreGuid.ToString().ToUpper()].Price != item.Price)
                        {
                            UpdateCommand command = new UpdateCommand("ProductPrice", new BasicParam("Price", item.Price.ToString()),
                                new EqFilterParam("ProductGuid", item.ProductGuid),
                                new EqFilterParam("StoreGuid", item.StoreGuid));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                    }
                    else
                    {
                        InsertCommand command = new InsertCommand("ProductPrice",
                            new BasicParam("UniqueId", item.UniqueId),
                            new BasicParam("Price", item.Price.ToString()),
                            new BasicParam("ProductGuid", item.ProductGuid),
                            new BasicParam("StoreGuid", item.StoreGuid));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.PriceTbl);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task SyncOnHandAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.OnHand);
                List<StoreActiveOnhandViewModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreActiveOnhandViewModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Stores.OnHand, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreActiveOnhandViewModel>>(TokenType.AppToken, Configuration.WebService.Stores.OnHandAfter, data, true);
                }
                Dictionary<string, StoreActiveOnhandViewModel> currentOnHand = new Dictionary<string, StoreActiveOnhandViewModel>();
                var onhandList = AnatoliClient.GetInstance().DbClient.GetList<StoreActiveOnhandViewModel>(new StringQuery("SELECT * FROM StoreOnhand"));
                foreach (var item in onhandList)
                {
                    currentOnHand.Add(item.ProductGuid.ToString().ToUpper() + item.StoreGuid.ToString().ToUpper(), item);
                }

                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list)
                {
                    if (currentOnHand.ContainsKey(item.ProductGuid.ToString().ToUpper() + item.StoreGuid.ToString().ToUpper()))
                    {
                        var currentValue = currentOnHand[item.ProductGuid.ToString().ToUpper() + item.StoreGuid.ToString().ToUpper()];
                        if (currentValue.Qty != item.Qty)
                        {
                            UpdateCommand command = new UpdateCommand("StoreOnhand", new BasicParam("Qty", item.Qty.ToString()),
                                new EqFilterParam("ProductGuid", item.ProductGuid),
                                new EqFilterParam("StoreGuid", item.StoreGuid));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                    }
                    else
                    {
                        InsertCommand command = new InsertCommand("StoreOnhand",
                            new BasicParam("UniqueId", item.UniqueId),
                            new BasicParam("Qty", item.Qty.ToString()),
                            new BasicParam("ProductGuid", item.ProductGuid),
                            new BasicParam("StoreGuid", item.StoreGuid));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.OnHand);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task SyncFavoritsAsync()
        {
            // TODO: implement 

            //try
            //{
            //    var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.BasketTbl);
            //    var data = new RequestModel.BaseRequestModel();
            //    data.dateAfter = lastUpdateTime.ToString();
            //    var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketModel>>(TokenType.UserToken, Configuration.WebService.Users.BasketView, data, false);
            //    await DataAdapter.UpdateItemAsync(new UpdateCommand("products", new BasicParam("favorit", "0")));
            //    foreach (var basket in list)
            //    {
            //        using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
            //        {
            //            connection.BeginTransaction();
            //            if (basket.BasketTypeValueId == BasketModel.FavoriteBasketTypeId)
            //            {
            //                foreach (var item in basket.BasketItems)
            //                {
            //                    UpdateCommand command = new UpdateCommand("products", new EqFilterParam("product_id", item.ProductId.ToString().ToUpper()),
            //                  new BasicParam("favorit", "1"));
            //                    var query = connection.CreateCommand(command.GetCommand());
            //                    int t = query.ExecuteNonQuery();
            //                }
            //            }
            //            connection.Commit();
            //            await SyncManager.AddLogAsync(SyncManager.BasketTbl);
            //        }
            //    }
            //}
            //catch (Exception e)
            //{

            //}
        }


        public static bool RemoveFavorit(ProductModel product)
        {
            var dbQuery = new DeleteCommand("BasketItem",
                new EqFilterParam("ProductId", product.UniqueId),
                new EqFilterParam("BasketTypeValueId", BasketModel.FavoriteBasketTypeId));
            var r = AnatoliClient.GetInstance().DbClient.UpdateItem(dbQuery) > 0 ? true : false;
            if (r)
            {
                product.FavoritBasketCount = 0;
                RemoveFavoritFromCloud(product.UniqueId);
            }
            return r;
        }

        public static bool AddToFavorits(ProductModel product)
        {
            var dbQuery = new InsertCommand("BasketItem",
                new BasicParam("ProductId", product.UniqueId),
                new BasicParam("BasketTypeValueId", BasketModel.FavoriteBasketTypeId),
                new BasicParam("Qty", "1"));
            var r = AnatoliClient.GetInstance().DbClient.UpdateItem(dbQuery) > 0 ? true : false;
            if (r)
            {
                product.FavoritBasketCount = 1;
                AddFavoritToCloud();
            }
            return r;
        }

        public static async Task AddFavoritToCloud()
        {
            // TODO : implement

            //try
            //{
            //    var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.BasketTbl);
            //    var data = new RequestModel.BaseRequestModel();
            //    data.dateAfter = lastUpdateTime.ToString();
            //    var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketModel>>(TokenType.UserToken, Configuration.WebService.Users.BasketView, data, false);
            //    Guid basketId = default(Guid);
            //    foreach (var basket in list)
            //    {
            //        using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
            //        {
            //            connection.BeginTransaction();
            //            if (basket.BasketTypeValueId == BasketModel.FavoriteBasketTypeId)
            //            {
            //                basketId = Guid.Parse(basket.UniqueId);
            //            }

            //        }
            //    }

            //    var f = await ProductManager.GetFavorits();
            //    List<BasketItemModel> items = new List<BasketItemModel>();
            //    foreach (var item in f)
            //    {
            //        var i = new BasketItemModel();
            //        i.Qty = 1;
            //        i.ProductId = Guid.Parse(item.product_id);
            //        i.BasketId = basketId;
            //        items.Add(i);
            //    }
            //    var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketItemModel>>(TokenType.UserToken, Configuration.WebService.Users.FavoritSaveItem, items, false);

            //}
            //catch (Exception e)
            //{


            //}
        }

        public static async Task RemoveFavoritFromCloud(Guid productId)
        {
            // TODO : implement

            //try
            //{
            //    var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.BasketTbl);
            //    var q = new RemoteQuery(TokenType.UserToken, Configuration.WebService.Users.BasketView, HttpMethod.Get, new BasicParam("after", lastUpdateTime.ToString()));
            //    var list = await BaseDataAdapter<BasketModel>.GetListAsync(q);
            //    Guid basketId = default(Guid);
            //    foreach (var basket in list)
            //    {
            //        using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
            //        {
            //            connection.BeginTransaction();
            //            if (basket.BasketTypeValueId == BasketModel.FavoriteBasketTypeId)
            //            {
            //                basketId = Guid.Parse(basket.UniqueId);
            //            }

            //        }
            //    }

            //    List<BasketItemModel> items = new List<BasketItemModel>();
            //    BasketItemModel favoritItem = new BasketItemModel();
            //    favoritItem.Qty = 1;
            //    favoritItem.ProductId = Guid.Parse(productId);
            //    favoritItem.BasketId = basketId;
            //    items.Add(favoritItem);
            //    var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketItemModel>>(TokenType.UserToken, Configuration.WebService.Users.FavoritDeleteItem, items, false);

            //}
            //catch (Exception e)
            //{


            //}
        }


        public static List<string> GetSuggests(string key, int no)
        {
            try
            {
                var dbQuery = new SelectQuery("Product", new SearchFilterParam("StoreProductName", key));
                dbQuery.Limit = 10000;
                var listModel = AnatoliClient.GetInstance().DbClient.GetList<ProductModel>(dbQuery);
                if (listModel.Count > 0)
                    return ShowSuggests(listModel, no);
                else
                    return null;
            }
            catch (Exception)
            {

                return null;
            }
        }

        static List<string> ShowSuggests(List<ProductModel> list, int no)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            List<string> suggestions = new List<string>();
            foreach (var item in list)
            {
                var pname = item.StoreProductName;
                var splits = pname.Split(new char[] { ' ' });
                string word = splits[0];
                if (!dict.ContainsKey(word))
                    dict.Add(word, 1);
                else
                    dict[word]++;

                if (splits.Length > 1)
                {
                    word = splits[0] + " " + splits[1];
                    if (!dict.ContainsKey(word))
                        dict.Add(word, 1);
                    else
                        dict[word]++;
                }

                if (splits.Length > 2)
                {
                    word = splits[0] + " " + splits[1] + " " + splits[2];
                    if (!dict.ContainsKey(word))
                        dict.Add(word, 1);
                    else
                        dict[word]++;
                }
            }

            foreach (KeyValuePair<string, int> item in dict.OrderByDescending(k => k.Value))
            {
                suggestions.Add(item.Key);
            }
            List<string> output = new List<string>();
            for (int i = 0; i < Math.Min(no, suggestions.Count); i++)
            {
                output.Add(suggestions[i]);
            }
            return output;
        }

        public static StringQuery Search(string value, string storeId)
        {
            StringQuery query = new StringQuery(string.Format("SELECT * FROM ProductStoreView WHERE (StoreProductName LIKE '%{0}%' OR GroupName LIKE '%{0}%') AND (StoreGuid = '{1}') AND IsRemoved='0' ORDER BY ProductGroupId , StoreProductName", value, storeId).PersianToArabic());
            return query;
        }

        public static string GetImageAddress(Guid productId, string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
                return null;
            else
            {
                string imguri = String.Format("{2}/content/Images/635126C3-D648-4575-A27C-F96C595CDAC5/100x100/{0}/{1}.png", productId, imageId, Configuration.WebService.PortalAddress);
                return imguri;
            }
        }
        public static List<ProductModel> GetFavorits()
        {
            try
            {
                var dbQuery = new SelectQuery("BasketItem", new EqFilterParam("BasketTypeValueId", BasketModel.FavoriteBasketTypeId));
                return AnatoliClient.GetInstance().DbClient.GetList<ProductModel>(dbQuery);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static StringQuery GetFavoritsQueryString(Guid storeId)
        {
            var query = new StringQuery(string.Format("SELECT * FROM ProductStoreView WHERE FavoritBasketCount > 0 AND ProductStoreView.StoreGuid = '{0}' AND ProductStoreView.IsRemoved='0' ORDER BY StoreProductName", storeId));
            return query;
        }

        public bool ShowGroups = false;
        Guid lastGroupId = Guid.NewGuid();
        //public override List<ProductModel> GetNext()
        //{
        //    var list = base.GetNext();
        //    if (!ShowGroups)
        //        return list;
        //    List<ProductModel> list2 = new List<ProductModel>();
        //    foreach (var item in list)
        //    {
        //        if (!item.ProductGroupId.Equals(lastGroupId))
        //        {
        //            lastGroupId = item.ProductGroupId;
        //            ProductModel g = new ProductModel();
        //            g.is_group = 1;
        //            g.cat_id = lastGroupId;
        //            g.cat_name = item.cat_name;
        //            g.product_name = ProductGroupManager.GetFullNameAsync(item.cat_id);
        //            list2.Add(g);
        //            list2.Add(item);
        //        }
        //        else
        //            list2.Add(item);
        //    }
        //    return list2;
        //}


        public static StringQuery GetGroupQueryString(Guid catId, Guid storeId)
        {
            if (catId == null)
            {
                var q = new StringQuery(string.Format("SELECT * FROM ProductStoreView WHERE StoreGuid='{0}' AND IsRemoved='0' ORDER BY StoreProductName", storeId).PersianToArabic());
                return q;
            }
            var leftRight = ProductGroupManager.GetLeftRight(catId);
            StringQuery query;
            if (leftRight != null)
                query = new StringQuery(string.Format("SELECT * FROM ProductStoreView StoreGuid = '{2}' AND IsRemoved='0' WHERE NLeft >= {0} AND NRight <= {1} ORDER BY StoreProductName", leftRight.left, leftRight.right, storeId).PersianToArabic());
            else
                query = new StringQuery(string.Format("SELECT * FROM ProductStoreView StoreGuid = '{0}'  AND IsRemoved='0' ORDER BY StoreProductName", storeId).PersianToArabic());
            return query;
        }

        public static StringQuery GetAll(Guid storeId)
        {
            StringQuery query = new StringQuery(string.Format("SELECT * FROM ProductStoreView StoreGuid = '{0}' AND IsRemoved='0' ORDER BY StoreProductName", storeId).PersianToArabic());
            return query;
        }

        public override int UpdateItem(ProductModel model)
        {
            throw new NotImplementedException();
        }
    }
}
