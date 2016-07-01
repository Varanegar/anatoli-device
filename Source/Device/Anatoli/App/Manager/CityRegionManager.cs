using Anatoli.App.Model.Product;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Manager
{
    public class CityRegionManager : BaseManager<CityRegionModel>
    {
        public static async Task SyncDataBaseAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.CityRegionTbl);
                List<CityRegionModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<CityRegionModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.CityRegion, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<CityRegionModel>>(TokenType.AppToken, Configuration.WebService.CityRegionAfter, data, true);
                }

                Dictionary<Guid, CityRegionModel> items = new Dictionary<Guid, CityRegionModel>();
                var currentList = AnatoliClient.GetInstance().DbClient.GetList<CityRegionModel>(new StringQuery("SELECT * FROM cityregion"));
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
                        UpdateCommand command = new UpdateCommand("CityRegion", new BasicParam("GroupName", item.GroupName),
                        new EqFilterParam("UniqueId", item.UniqueId),
                        new BasicParam("ParentId", item.ParentUniqueIdString),
                        new BasicParam("NLevel", item.NLevel.ToString()),
                        new BasicParam("NLeft", item.NLeft.ToString()),
                        new BasicParam("NRight", item.NRight.ToString()));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                    else
                    {
                        InsertCommand command = new InsertCommand("cityregion", new BasicParam("GroupName", item.GroupName),
                            new BasicParam("UniqueId", item.UniqueId),
                            new BasicParam("ParentId", item.ParentUniqueIdString),
                            new BasicParam("NLevel", item.NLevel.ToString()),
                            new BasicParam("NLeft", item.NLeft.ToString()),
                            new BasicParam("NRight", item.NRight.ToString()));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.CityRegionTbl);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static GroupLeftRightModel GetLeftRight()
        {
            try
            {
                var connection = AnatoliClient.GetInstance().DbClient.GetConnection();
                SQLite.SQLiteCommand query;
                query = connection.CreateCommand("SELECT min(NLeft) as left , max(NRight) as right FROM CityRegion");
                var lr = query.ExecuteQuery<GroupLeftRightModel>();
                return lr.First();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static GroupLeftRightModel GetLeftRight(Guid groupId)
        {
            try
            {
                var connection = AnatoliClient.GetInstance().DbClient.GetConnection();
                SQLite.SQLiteCommand query;
                query = connection.CreateCommand(String.Format("SELECT NLeft as left , NRight as right FROM CityRegion WHERE UniqueId ='{0}'", groupId));
                var lr = query.ExecuteQuery<GroupLeftRightModel>();
                return lr.First();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static List<CityRegionModel> GetFirstLevel()
        {
            try
            {
                var query = new StringQuery("SELECT * FROM CityRegion WHERE NLevel = 1 ORDER BY GroupName ASC");
                var list = AnatoliClient.GetInstance().DbClient.GetList<CityRegionModel>(query);
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static List<CityRegionModel> GetGroups(Guid groupId)
        {
            try
            {
                var query = new StringQuery(string.Format("SELECT * FROM CityRegion WHERE ParentId = '{0}' ORDER BY GroupName ASC", groupId));
                query.Unlimited = true;
                var list = AnatoliClient.GetInstance().DbClient.GetList<CityRegionModel>(query);
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static CityRegionModel GetParentGroup(Guid groupId)
        {
            try
            {
                var current = AnatoliClient.GetInstance().DbClient.GetItem<CityRegionModel>(new StringQuery(string.Format("SELECT * FROM CityRegion WHERE UniqueId='{0}'", groupId)));
                var parent = AnatoliClient.GetInstance().DbClient.GetItem<CityRegionModel>(new StringQuery(string.Format("SELECT * FROM CityRegion WHERE UniqueId='{0}'", current.ParentId)));
                return parent;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static CityRegionModel GetGroupInfoAsync(Guid grouId)
        {
            try
            {
                var c = AnatoliClient.GetInstance().DbClient.GetItem<CityRegionModel>(new StringQuery(string.Format("SELECT * FROM CityRegion WHERE UniqueId='{0}'", grouId)));
                return c;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override int UpdateItem(CityRegionModel model)
        {
            throw new NotImplementedException();
        }
    }
}
