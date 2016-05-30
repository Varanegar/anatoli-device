using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model.Store;
using Anatoli.Framework.DataAdapter;
using Anatoli.Framework.Manager;
using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Anatoli.App.Manager
{
    public class StoreManager : BaseManager<StoreDataModel>
    {
        public static async Task SyncDataBase(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.StoresTbl);
                List<StoreUpdateModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreUpdateModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Stores.StoresView, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreUpdateModel>>(TokenType.AppToken, Configuration.WebService.Stores.StoresViewAfter, data, true);
                }
                Dictionary<string, StoreDataModel> items = new Dictionary<string, StoreDataModel>();
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    var query = connection.CreateCommand("SELECT * FROM stores");
                    var currentList = query.ExecuteQuery<StoreDataModel>();
                    foreach (var item in currentList)
                    {
                        if (!items.ContainsKey(item.store_id))
                        {
                            items.Add(item.store_id, item);
                        }
                    }
                }
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    connection.BeginTransaction();
                    foreach (var item in list)
                    {
                        if (!item.UniqueId.Equals("680D21FE-5D68-4396-A99F-60814DF27D07"))
                        {
                            if (items.ContainsKey(item.UniqueId))
                            {
                                UpdateCommand command = new UpdateCommand("stores", new EqFilterParam("store_id", item.UniqueId.ToUpper()),
                                    new BasicParam("store_name", item.storeName.Trim()),
                                    new BasicParam("store_tel", item.Phone),
                                    new BasicParam("lat", item.lat.ToString()),
                                    new BasicParam("lng", item.lng.ToString()),
                                    new BasicParam("is_removed", (item.IsRemoved) ? "1" : "0"),
                                    new BasicParam("store_address", item.address));
                                var query = connection.CreateCommand(command.GetCommand());
                                int t = query.ExecuteNonQuery();
                            }
                            else
                            {
                                InsertCommand command = new InsertCommand("stores", new BasicParam("store_id", item.UniqueId.ToUpper()),
                                new BasicParam("store_name", item.storeName.Trim()),
                                new BasicParam("store_tel", item.Phone),
                                new BasicParam("lat", item.lat.ToString()),
                                new BasicParam("lng", item.lng.ToString()),
                                new BasicParam("is_removed", (item.IsRemoved) ? "1" : "0"),
                                new BasicParam("store_address", item.address));
                                var query = connection.CreateCommand(command.GetCommand());
                                int t = query.ExecuteNonQuery();
                            }
                        }
                    }
                    connection.Commit();
                }

                await SyncStoreCalendar();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task SyncStoreCalendar()
        {
            try
            {
                List<StoreCalendarViewModel> list2;
                list2 = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreCalendarViewModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Stores.StoreCalendar, false);

                Dictionary<string, StoreCalendarViewModel> timeItems = new Dictionary<string, StoreCalendarViewModel>();
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    var query = connection.CreateCommand("SELECT * FROM stores_calendar");
                    var currentList = query.ExecuteQuery<StoreCalendarViewModel>();
                    foreach (var item in currentList)
                    {
                        if (!timeItems.ContainsKey(item.UniqueId))
                        {
                            timeItems.Add(item.UniqueId, item);
                        }
                    }
                }
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    connection.BeginTransaction();
                    foreach (var item in list2)
                    {
                        if (timeItems.ContainsKey(item.UniqueId))
                        {
                            UpdateCommand command = new UpdateCommand(SyncManager.StoreCalendarTbl, new EqFilterParam("UniqueId", item.UniqueId),
                                new BasicParam("StoreId", item.StoreId),
                                new BasicParam("Date", item.Date),
                            new BasicParam("PDate", item.PDate),
                            new BasicParam("FromTimeString", item.FromTimeString),
                            new BasicParam("ToTimeString", item.ToTimeString),
                            new BasicParam("CalendarTypeValueId", item.CalendarTypeValueId.ToUpper()));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                        else
                        {
                            InsertCommand command = new InsertCommand(SyncManager.StoreCalendarTbl, new BasicParam("UniqueId", item.UniqueId),
                            new BasicParam("StoreId", item.StoreId),
                            new BasicParam("Date", item.Date),
                            new BasicParam("PDate", item.PDate),
                            new BasicParam("FromTimeString", item.FromTimeString),
                            new BasicParam("ToTimeString", item.ToTimeString),
                            new BasicParam("CalendarTypeValueId", item.CalendarTypeValueId.ToUpper()));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                    }
                    connection.Commit();
                }
                await SyncManager.AddLogAsync(SyncManager.StoreCalendarTbl);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static StringQuery Search(string value)
        {
            StringQuery query = new StringQuery(string.Format("SELECT * FROM stores WHERE store_name LIKE '%{0}%' AND is_removed != 1", value));
            return query;
        }
        public static StringQuery GetAll()
        {
            StringQuery query = new StringQuery(string.Format("SELECT * FROM stores WHERE is_removed != 1"));
            return query;
        }
        public static async Task<bool> SelectAsync(StoreDataModel store)
        {
            UpdateCommand command1 = new UpdateCommand("stores", new BasicParam("selected", "0"));
            UpdateCommand command2 = new UpdateCommand("stores", new BasicParam("selected", "1"), new EqFilterParam("store_id", store.store_id));
            try
            {
                int clear = await DataAdapter.UpdateItemAsync(command1);
                int result = await DataAdapter.UpdateItemAsync(command2);
                await ShoppingCardManager.ClearAsync();
                return (result > 0) ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static async Task<StoreDataModel> GetDefaultAsync()
        {
            SelectQuery query = new SelectQuery("stores", new EqFilterParam("selected", "1"));
            try
            {
                var store = await BaseDataAdapter<StoreDataModel>.GetItemAsync(query);
                if (store == null)
                    return null;
                return store;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<bool> UpdateDistanceAsync(string store_id, float dist)
        {
            UpdateCommand command = new UpdateCommand("stores", new BasicParam("distance", dist.ToString()), new EqFilterParam("store_id", store_id));
            try
            {
                int result = await DataAdapter.UpdateItemAsync(command);
                return (result > 0) ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
