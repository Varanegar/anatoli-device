using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model.Store;
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
    public class StoreManager : BaseManager<StoreModel>
    {
        public static async Task SyncDataBase(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.StoresTbl);
                List<StoreModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Stores.StoresView, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreModel>>(TokenType.AppToken, Configuration.WebService.Stores.StoresViewAfter, data, true);
                }
                Dictionary<Guid, StoreModel> items = new Dictionary<Guid, StoreModel>();
                var currentList = AnatoliClient.GetInstance().DbClient.GetList<StoreModel>(new StringQuery("SELECT * FROM Store"));
                foreach (var item in currentList)
                {
                    if (!items.ContainsKey(item.UniqueId))
                    {
                        items.Add(item.UniqueId, item);
                    }
                }
                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list)
                {
                    // ستاد مرکزی را اضافه نمیکنیم
                    if (!item.UniqueId.Equals(Guid.Parse("680D21FE-5D68-4396-A99F-60814DF27D07")))
                    {
                        if (items.ContainsKey(item.UniqueId))
                        {
                            UpdateCommand command = new UpdateCommand("Store", new EqFilterParam("UniqueId", item.UniqueId.ToString()),
                                new BasicParam("storeName", item.storeName.Trim()),
                                new BasicParam("Phone", item.Phone),
                                new BasicParam("lat", item.lat.ToString()),
                                new BasicParam("lng", item.lng.ToString()),
                                new BasicParam("IsRemoved", (item.IsRemoved) ? "1" : "0"),
                                new BasicParam("supportAppOrder", (item.supportAppOrder) ? "1" : "0"),
                                new BasicParam("address", item.address));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                        else
                        {
                            InsertCommand command = new InsertCommand("Store", new BasicParam("UniqueId", item.UniqueId.ToString()),
                            new BasicParam("storeName", item.storeName.Trim()),
                            new BasicParam("Phone", item.Phone),
                            new BasicParam("lat", item.lat.ToString()),
                            new BasicParam("lng", item.lng.ToString()),
                            new BasicParam("IsRemoved", (item.IsRemoved) ? "1" : "0"),
                            new BasicParam("supportAppOrder", (item.supportAppOrder) ? "1" : "0"),
                            new BasicParam("address", item.address));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
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
                List<StoreCalendarModel> list2;
                list2 = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<StoreCalendarModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Stores.StoreCalendar, false);
                Dictionary<Guid, StoreCalendarModel> timeItems = new Dictionary<Guid, StoreCalendarModel>();
                var currentList = AnatoliClient.GetInstance().DbClient.GetList<StoreCalendarModel>(new StringQuery("SELECT * FROM StoreCalendar"));
                foreach (var item in currentList)
                {
                    if (!timeItems.ContainsKey(item.UniqueId))
                    {
                        timeItems.Add(item.UniqueId, item);
                    }
                }

                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list2)
                {
                    if (timeItems.ContainsKey(item.UniqueId))
                    {
                        UpdateCommand command = new UpdateCommand(SyncManager.StoreCalendarTbl, new EqFilterParam("UniqueId", item.UniqueId.ToString()),
                            new BasicParam("StoreId", item.StoreId.ToString()),
                            new BasicParam("Date", item.Date),
                        new BasicParam("PDate", item.PDate),
                        new BasicParam("FromTimeString", item.FromTimeString),
                        new BasicParam("ToTimeString", item.ToTimeString),
                        new BasicParam("CalendarTypeValueId", item.CalendarTypeValueId.ToString()));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                    else
                    {
                        InsertCommand command = new InsertCommand(SyncManager.StoreCalendarTbl, new BasicParam("UniqueId", item.UniqueId.ToString()),
                        new BasicParam("StoreId", item.StoreId.ToString()),
                        new BasicParam("Date", item.Date),
                        new BasicParam("PDate", item.PDate),
                        new BasicParam("FromTimeString", item.FromTimeString),
                        new BasicParam("ToTimeString", item.ToTimeString),
                        new BasicParam("CalendarTypeValueId", item.CalendarTypeValueId.ToString()));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.StoreCalendarTbl);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static StringQuery SearchQuerString(string value)
        {
            StringQuery query = new StringQuery(string.Format("SELECT * FROM Store WHERE storeName LIKE '%{0}%' AND IsRemoved == 0", value));
            return query;
        }
        public static StringQuery GetAllQueryString()
        {
            StringQuery query = new StringQuery(string.Format("SELECT * FROM Store WHERE IsRemoved != 1"));
            return query;
        }
        public static bool Select(StoreModel store)
        {
            UpdateCommand command1 = new UpdateCommand("Store", new BasicParam("selected", "0"));
            UpdateCommand command2 = new UpdateCommand("Store", new BasicParam("selected", "1"), new EqFilterParam("UniqueId", store.UniqueId.ToString()));
            try
            {
                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                int clear = AnatoliClient.GetInstance().DbClient.UpdateItem(command1);
                int result = AnatoliClient.GetInstance().DbClient.UpdateItem(command2);
                ShoppingCardManager.Clear();
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                return (result > 0) ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static StoreModel GetDefault()
        {
            SelectQuery query = new SelectQuery("Store", new EqFilterParam("selected", "1"));
            try
            {
                var store = AnatoliClient.GetInstance().DbClient.GetItem<StoreModel>(query);
                if (store == null)
                    return null;
                return store;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool UpdateDistance(Guid store_id, float dist)
        {
            UpdateCommand command = new UpdateCommand("Store", new BasicParam("distance", dist.ToString()), new EqFilterParam("UniqueId", store_id));
            try
            {
                int result = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                return (result > 0) ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override int UpdateItem(StoreModel model)
        {
            throw new NotImplementedException();
        }
    }
}
