using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anatoli.Framework.Manager;
using Anatoli.App.Model.Product;
using Anatoli.Framework.AnatoliBase;
using System.Net.Http;
namespace Anatoli.App.Manager
{
    public class ProductGroupManager : BaseManager<ProductGroupModel>
    {
        public static async Task SyncDataBaseAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.GroupsTbl);
                List<ProductGroupModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductGroupModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.Products.ProductGroups, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductGroupModel>>(TokenType.AppToken, Configuration.WebService.Products.ProductGroupsAfter, data, true);
                }
                Dictionary<Guid, ProductGroupModel> items = new Dictionary<Guid, ProductGroupModel>();
                var currentList = AnatoliClient.GetInstance().DbClient.GetList<ProductGroupModel>(new StringQuery("SELECT * FROM ProductGroup"));
                foreach (var item in currentList)
                {
                    items.Add(item.UniqueId, item);
                }
                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list)
                {
                    if (item.UniqueId != Guid.Parse("169EBC7C-6112-4566-93B9-2869C3D3A112") && item.UniqueId != Guid.Parse("E54AF059 -5C22-4FAF-BC40-4169BF74C020"))
                    {
                        if (items.ContainsKey(item.UniqueId))
                        {
                            UpdateCommand command = new UpdateCommand("ProductGroup", new EqFilterParam("UniqueId", item.UniqueId.ToString()),
                               new BasicParam("GroupName", item.GroupName.Trim()),
                               new BasicParam("ParentId", item.ParentUniqueIdString),
                               new BasicParam("NLeft", item.NLeft.ToString()),
                               new BasicParam("NRight", item.NRight.ToString()),
                               new BasicParam("IsRemoved", (item.IsRemoved) ? "1" : "0"),
                               new BasicParam("NLevel", item.NLevel.ToString()));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                        else
                        {
                            InsertCommand command = new InsertCommand("ProductGroup", new BasicParam("UniqueId", item.UniqueId.ToString()),
                           new BasicParam("GroupName", item.GroupName.Trim()),
                           new BasicParam("ParentId", item.ParentUniqueIdString.ToUpper()),
                           new BasicParam("NLeft", item.NLeft.ToString()),
                           new BasicParam("IsRemoved", (item.IsRemoved) ? "1" : "0"),
                           new BasicParam("NRight", item.NRight.ToString()),
                           new BasicParam("NLevel", item.NLevel.ToString()));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.GroupsTbl);

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
                query = connection.CreateCommand("SELECT min(NLeft) as left , max(NRight) as right FROM ProductGroup WHERE IsRemoved = 0");
                var lr = query.ExecuteQuery<GroupLeftRightModel>();
                return lr.First();

            }
            catch (Exception)
            {
                return null;
            }
        }
        public static GroupLeftRightModel GetLeftRight(Guid catId)
        {
            try
            {
                var connection = AnatoliClient.GetInstance().DbClient.GetConnection();
                SQLite.SQLiteCommand query;
                query = connection.CreateCommand(String.Format("SELECT NLeft as left, NRight as right FROM ProductGroup WHERE UniqueId ='{0}'", catId));
                var lr = query.ExecuteQuery<GroupLeftRightModel>();
                return lr.First();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static List<ProductGroupModel> GetFirstLevel()
        {
            try
            {
                var query = new StringQuery("SELECT * FROM ProductGroup WHERE NLevel = 1 AND IsRemoved = 0");
                query.Unlimited = true;
                var list = AnatoliClient.GetInstance().DbClient.GetList<ProductGroupModel>(query);
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static List<ProductGroupModel> GetProductGroup(string catId)
        {
            try
            {
                var query = new StringQuery(string.Format("SELECT * FROM ProductGroup WHERE ParentId = '{0}' AND IsRemoved = 0", catId));
                var list = AnatoliClient.GetInstance().DbClient.GetList<ProductGroupModel>(query);
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static ProductGroupModel GetParentCategory(string catId)
        {
            try
            {
                var current = AnatoliClient.GetInstance().DbClient.GetItem<ProductGroupModel>(new StringQuery(string.Format("SELECT * FROM ProductGroup WHERE UniqueId = '{0}'", catId)));
                var parent = AnatoliClient.GetInstance().DbClient.GetItem<ProductGroupModel>(new StringQuery(string.Format("SELECT * FROM ProductGroup WHERE UniqueId = '{0}'", current.ParentId)));
                return parent;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ProductGroupModel GetCategoryInfo(string catId)
        {
            try
            {
                var c = AnatoliClient.GetInstance().DbClient.GetItem<ProductGroupModel>(new StringQuery(string.Format("SELECT * FROM ProductGroup WHERE UniqueId = '{0}'", catId)));
                return c;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string GetImageAddress(string catId, string imageId)
        {
            if (string.IsNullOrEmpty(catId) || string.IsNullOrEmpty(imageId))
                return null;
            else
            {
                string imguri = String.Format("{2}/content/Images/149E61EF-C4DC-437D-8BC9-F6037C0A1ED1/320x320/{0}/{1}.png", catId, imageId, Configuration.WebService.PortalAddress);
                return imguri;
            }
        }

        public static string GetFullName(Guid catId)
        {
            try
            {
                var current = GetItem(catId);
                if (current != null)
                {
                    var parent = AnatoliClient.GetInstance().DbClient.GetItem<ProductGroupModel>(new StringQuery(string.Format("SELECT * FROM ProductGroup WHERE UniqueId = '{0}'", current.ParentId)));
                    if (parent != null)
                        return parent.GroupName + " / " + current.GroupName;
                    else
                        return current.GroupName;
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }


        public static List<ProductGroupModel> Search(string value)
        {
            var q2 = new StringQuery(string.Format("SELECT * FROM ProductGroup WHERE GroupName LIKE '%{0}%' AND IsRemoved = 0", value));
            var groups = AnatoliClient.GetInstance().DbClient.GetList<ProductGroupModel>(q2);
            return groups;
        }

        public static ProductGroupModel GetItem(Guid uniqueId)
        {
            return AnatoliClient.GetInstance().DbClient.GetItem<ProductGroupModel>(new StringQuery(string.Format("SELECT * FROM ProductGroup WHERE UniqueId = '{0}'", uniqueId)));
        }

        public override int UpdateItem(ProductGroupModel model)
        {
            throw new NotImplementedException();
        }
    }
}
