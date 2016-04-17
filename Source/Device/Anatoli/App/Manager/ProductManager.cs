using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model.Product;
using Anatoli.Framework.DataAdapter;
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
        public static async Task SyncProductTagsAsync()
        {
            RemoteQuery q = new RemoteQuery(TokenType.AppToken, Configuration.WebService.Products.ProductsTags, HttpMethod.Get);
            var list = await BaseDataAdapter<ProductTagViewModel>.GetListAsync(q);
        }
        public static async Task<ProductModel> GetItemAsync(string id)
        {
            var query = new StringQuery(string.Format("SELECT * FROM products_price_view WHERE product_id='{0}'", id));
            return await GetItemAsync(query);
        }
        public static async Task SyncProductsAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            string queryString = "";
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.ProductTbl);
                List<ProductUpdateModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductUpdateModel>>(TokenType.AppToken, Configuration.WebService.Products.ProductsList);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductUpdateModel>>(TokenType.AppToken, Configuration.WebService.Products.ProductsListAfter, data);
                }
                Dictionary<string, ProductModel> items = new Dictionary<string, ProductModel>();
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    var query = connection.CreateCommand("SELECT * FROM products");
                    var currentList = query.ExecuteQuery<ProductModel>();
                    foreach (var item in currentList)
                    {
                        items.Add(item.product_id, item);
                    }
                }
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    connection.BeginTransaction();
                    foreach (var item in list)
                    {
                        if (items.ContainsKey(item.UniqueId))
                        {
                            UpdateCommand command = new UpdateCommand("products", new EqFilterParam("product_id", item.UniqueId.ToUpper()),
                            new BasicParam("product_name", item.StoreProductName),
                            new BasicParam("is_removed", (item.IsRemoved == true) ? "1" : "0"),
                            new BasicParam("cat_id", (item.ProductGroupIdString != null) ? item.ProductGroupIdString.ToUpper() : item.ProductGroupIdString));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                        else
                        {
                            InsertCommand command = new InsertCommand("products", new BasicParam("product_id", item.UniqueId.ToUpper()),
                            new BasicParam("product_name", item.StoreProductName),
                            new BasicParam("is_removed", (item.IsRemoved == true) ? "1" : "0"),
                            new BasicParam("cat_id", (item.ProductGroupIdString != null) ? item.ProductGroupIdString.ToUpper() : item.ProductGroupIdString));
                            var query = connection.CreateCommand(command.GetCommand());
                            queryString = query.ToString();
                            int t = query.ExecuteNonQuery();
                        }
                    }
                    connection.Commit();
                }
                await SyncManager.AddLogAsync(SyncManager.ProductTbl);
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
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.PriceTbl);
                List<ProductPriceUpdateModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductPriceUpdateModel>>(TokenType.AppToken, Configuration.WebService.Stores.PricesView);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductPriceUpdateModel>>(TokenType.AppToken, Configuration.WebService.Stores.PricesViewAfter, data);
                }
                Dictionary<string, ProductPriceModel> items = new Dictionary<string, ProductPriceModel>();
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    var query = connection.CreateCommand("SELECT * FROM products_price");
                    var currentList = query.ExecuteQuery<ProductPriceModel>();
                    foreach (var item in currentList)
                    {
                        items.Add(item.product_id.ToUpper() + item.store_id.ToUpper(), item);
                    }
                }
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    connection.BeginTransaction();
                    foreach (var item in list)
                    {
                        if (items.ContainsKey(item.ProductGuid.ToUpper() + item.StoreGuid.ToString().ToUpper()))
                        {
                            UpdateCommand command = new UpdateCommand("products_price", new BasicParam("price", item.Price.ToString()),
                            new EqFilterParam("product_id", item.ProductGuid.ToUpper()),
                            new EqFilterParam("store_id", item.StoreGuid.ToString().ToUpper()));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                        else
                        {
                            InsertCommand command = new InsertCommand("products_price", new BasicParam("price", item.Price.ToString()),
                            new BasicParam("product_id", item.ProductGuid.ToUpper()),
                            new BasicParam("store_id", item.StoreGuid.ToString().ToUpper()));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                    }
                    connection.Commit();
                }
                await SyncManager.AddLogAsync(SyncManager.PriceTbl);
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
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.OnHand);
                List<StoreActiveOnhandViewModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreActiveOnhandViewModel>>(TokenType.AppToken, Configuration.WebService.Stores.OnHand);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreActiveOnhandViewModel>>(TokenType.AppToken, Configuration.WebService.Stores.OnHandAfter, data);
                }
                Dictionary<string, StoreActiveOnhandViewModel> currentOnHand = new Dictionary<string, StoreActiveOnhandViewModel>();
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    var query = connection.CreateCommand("SELECT * FROM store_onhand");
                    var onhandList = query.ExecuteQuery<StoreActiveOnhandViewModel>();
                    foreach (var item in onhandList)
                    {
                        currentOnHand.Add(item.ProductGuid + item.StoreGuid, item);
                    }
                }
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    connection.BeginTransaction();
                    foreach (var item in list)
                    {
                        if (currentOnHand.ContainsKey(item.ProductGuid + item.StoreGuid))
                        {
                            UpdateCommand command = new UpdateCommand("store_onhand", new BasicParam("qty", item.Qty.ToString()),
                            new EqFilterParam("product_id", item.ProductGuid.ToUpper()),
                            new EqFilterParam("store_id", item.StoreGuid.ToString().ToUpper()));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                        else
                        {
                            InsertCommand command = new InsertCommand("store_onhand", new BasicParam("qty", item.Qty.ToString()),
                            new BasicParam("product_id", item.ProductGuid),
                            new BasicParam("store_id", item.StoreGuid));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                    }
                    connection.Commit();
                }
                await SyncManager.AddLogAsync(SyncManager.OnHand);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task SyncFavoritsAsync()
        {
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.BasketTbl);
                var data = new RequestModel.BaseRequestModel();
                data.dateAfter = lastUpdateTime.ToString();
                var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketViewModel>>(TokenType.UserToken, Configuration.WebService.Users.BasketView, data);
                await DataAdapter.UpdateItemAsync(new UpdateCommand("products", new BasicParam("favorit", "0")));
                foreach (var basket in list)
                {
                    using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                    {
                        connection.BeginTransaction();
                        if (basket.BasketTypeValueId == BasketViewModel.FavoriteBasketTypeId)
                        {
                            foreach (var item in basket.BasketItems)
                            {
                                UpdateCommand command = new UpdateCommand("products", new EqFilterParam("product_id", item.ProductId.ToString().ToUpper()),
                              new BasicParam("favorit", "1"));
                                var query = connection.CreateCommand(command.GetCommand());
                                int t = query.ExecuteNonQuery();
                            }
                        }
                        connection.Commit();
                        await SyncManager.AddLogAsync(SyncManager.BasketTbl);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }


        public static async Task<bool> RemoveFavoritAsync(ProductModel product)
        {
            var dbQuery = new UpdateCommand("products", new EqFilterParam("product_id", product.product_id), new BasicParam("favorit", "0"));
            var r = await DataAdapter.UpdateItemAsync(dbQuery) > 0 ? true : false;
            if (r)
            {
                product.favorit = 0;
                RemoveFavoritFromCloud(product.product_id);
            }
            return r;
        }

        public static async Task<bool> AddToFavoritsAsync(ProductModel product)
        {
            var dbQuery = new UpdateCommand("products", new EqFilterParam("product_id", product.product_id), new BasicParam("favorit", "1"));
            var r = await DataAdapter.UpdateItemAsync(dbQuery) > 0 ? true : false;
            if (r)
            {
                product.favorit = 1;
                AddFavoritToCloud();
            }
            return r;
        }

        public static async Task AddFavoritToCloud()
        {
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.BasketTbl);
                var data = new RequestModel.BaseRequestModel();
                data.dateAfter = lastUpdateTime.ToString();
                var list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketViewModel>>(TokenType.UserToken, Configuration.WebService.Users.BasketView, data);
                Guid basketId = default(Guid);
                foreach (var basket in list)
                {
                    using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                    {
                        connection.BeginTransaction();
                        if (basket.BasketTypeValueId == BasketViewModel.FavoriteBasketTypeId)
                        {
                            basketId = Guid.Parse(basket.UniqueId);
                        }

                    }
                }

                var f = await ProductManager.GetFavorits();
                List<BasketItemViewModel> items = new List<BasketItemViewModel>();
                foreach (var item in f)
                {
                    var i = new BasketItemViewModel();
                    i.Qty = 1;
                    i.ProductId = Guid.Parse(item.product_id);
                    i.BasketId = basketId;
                    items.Add(i);
                }
                var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketItemViewModel>>(TokenType.UserToken, Configuration.WebService.Users.FavoritSaveItem, items);

            }
            catch (Exception e)
            {


            }
        }

        public static async Task RemoveFavoritFromCloud(string productId)
        {
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.BasketTbl);
                var q = new RemoteQuery(TokenType.UserToken, Configuration.WebService.Users.BasketView, HttpMethod.Get, new BasicParam("after", lastUpdateTime.ToString()));
                var list = await BaseDataAdapter<BasketViewModel>.GetListAsync(q);
                Guid basketId = default(Guid);
                foreach (var basket in list)
                {
                    using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                    {
                        connection.BeginTransaction();
                        if (basket.BasketTypeValueId == BasketViewModel.FavoriteBasketTypeId)
                        {
                            basketId = Guid.Parse(basket.UniqueId);
                        }

                    }
                }

                List<BasketItemViewModel> items = new List<BasketItemViewModel>();
                BasketItemViewModel favoritItem = new BasketItemViewModel();
                favoritItem.Qty = 1;
                favoritItem.ProductId = Guid.Parse(productId);
                favoritItem.BasketId = basketId;
                items.Add(favoritItem);
                var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BasketItemViewModel>>(TokenType.UserToken, Configuration.WebService.Users.FavoritDeleteItem, items);

            }
            catch (Exception e)
            {


            }
        }


        public static async Task<List<string>> GetSuggests(string key, int no)
        {
            try
            {
                var dbQuery = new SelectQuery("products", new SearchFilterParam("product_name", key));
                dbQuery.Limit = 10000;
                var listModel = await BaseDataAdapter<ProductModel>.GetListAsync(dbQuery);
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
                var pname = item.product_name;
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


        public static async Task<bool> RemoveFavoritsAll()
        {
            UpdateCommand command = new UpdateCommand("products", new BasicParam("favorit", "0"));
            try
            {
                var result = await DataAdapter.UpdateItemAsync(command);
                return (result > 0) ? true : false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static StringQuery Search(string value, string storeId)
        {
            //StringQuery query = new StringQuery(string.Format("SELECT * FROM products_price_view WHERE (product_name LIKE '{0}%' OR cat_name LIKE '{0}%') OR (product_name LIKE '% {0} %' OR cat_name LIKE '% {0} %') OR (product_name LIKE '% {0}' OR cat_name LIKE '% {0}') AND (store_id = '{1}') ORDER BY cat_id", value, storeId).PersianToArabic());
            StringQuery query = new StringQuery(string.Format("SELECT *,store_onhand.qty as qty FROM products_price_view JOIN store_onhand ON store_onhand.product_id = products_price_view.product_id WHERE (product_name LIKE '%{0}%' OR cat_name LIKE '%{0}%') AND (products_price_view.store_id = '{1}') AND (store_onhand.store_id = '{1}') AND products_price_view.is_removed='0' ORDER BY cat_id , product_name", value, storeId).PersianToArabic());
            return query;
        }

        public static string GetImageAddress(string productId, string imageId)
        {
            if (String.IsNullOrEmpty(productId) || string.IsNullOrEmpty(imageId))
                return null;
            else
            {
                string imguri = String.Format("{2}/content/Images/635126C3-D648-4575-A27C-F96C595CDAC5/100x100/{0}/{1}.png", productId, imageId, Configuration.WebService.PortalAddress);
                return imguri;
            }
        }
        public static async Task<List<ProductModel>> GetFavorits()
        {
            try
            {
                var dbQuery = new SelectQuery("products", new EqFilterParam("favorit", "1"));
                return await BaseDataAdapter<ProductModel>.GetListAsync(dbQuery);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static StringQuery GetFavoritsQueryString(string storeId)
        {
            var query = new StringQuery(string.Format("SELECT *,store_onhand.qty as qty FROM products_price_view JOIN store_onhand  ON store_onhand.product_id = products_price_view.product_id WHERE favorit = 1 AND products_price_view.store_id = '{0}' AND store_onhand.store_id='{0}' AND products_price_view.is_removed='0' ORDER BY product_name", storeId));
            return query;
        }

        public bool ShowGroups = false;
        string lastGroupId = Guid.NewGuid().ToString();
        public override async Task<List<ProductModel>> GetNextAsync()
        {
            var list = await base.GetNextAsync();
            if (!ShowGroups)
                return list;
            List<ProductModel> list2 = new List<ProductModel>();
            foreach (var item in list)
            {
                if (!item.cat_id.Equals(lastGroupId))
                {
                    lastGroupId = item.cat_id;
                    ProductModel g = new ProductModel();
                    g.is_group = 1;
                    g.cat_id = lastGroupId;
                    g.cat_name = item.cat_name;
                    g.product_name = await CategoryManager.GetFullNameAsync(item.cat_id);
                    list2.Add(g);
                    list2.Add(item);
                }
                else
                    list2.Add(item);
            }
            return list2;
        }


        public static StringQuery SetCatId(string catId, string storeId)
        {
            var leftRight = CategoryManager.GetLeftRight(catId);
            StringQuery query;
            if (leftRight != null)
                query = new StringQuery(string.Format("SELECT *,store_onhand.qty as qty FROM products_price_view JOIN store_onhand ON store_onhand.product_id = products_price_view.product_id WHERE cat_left >= {0} AND cat_right <= {1} AND products_price_view.store_id = '{2}' AND store_onhand.store_id = '{2}' AND products_price_view.is_removed='0'  ORDER BY product_name", leftRight.left, leftRight.right, storeId).PersianToArabic());
            else
                query = new StringQuery(string.Format("SELECT *,store_onhand.qty as qty FROM products_price_view JOIN store_onhand ON store_onhand.product_id = products_price_view.product_id AND products_price_view.store_id='{0}' AND store_onhand.store_id='{0}' AND products_price_view.is_removed='0' ORDER BY product_name", storeId).PersianToArabic());
            return query;
        }

        public static StringQuery GetAll(string storeId)
        {
            StringQuery query = new StringQuery(string.Format("SELECT *,store_onhand.qty as qty FROM products_price_view JOIN store_onhand ON store_onhand.product_id = products_price_view.product_id WHERE products_price_view.store_id = '{0}' AND store_onhand.store_id = '{0}' AND products_price_view.is_removed='0' ORDER BY product_name", storeId).PersianToArabic());
            return query;
        }
    }
}
