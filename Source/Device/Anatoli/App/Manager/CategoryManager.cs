using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anatoli.Framework.Manager;
using Anatoli.Framework.DataAdapter;
using Anatoli.App.Model.Product;
using Anatoli.Framework.AnatoliBase;
using System.Net.Http;
namespace Anatoli.App.Manager
{
    public class CategoryManager : BaseManager<CategoryInfoModel>
    {
        public static async Task SyncDataBaseAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.GroupsTbl);
                List<ProductGroupModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductGroupModel>>(TokenType.AppToken, Configuration.WebService.Products.ProductGroups);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ProductGroupModel>>(TokenType.AppToken, Configuration.WebService.Products.ProductGroupsAfter,data);
                }
                Dictionary<string, CategoryInfoModel> items = new Dictionary<string, CategoryInfoModel>();
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    var currentList = connection.CreateCommand("SELECT * FROM categories").ExecuteQuery<CategoryInfoModel>();
                    foreach (var item in currentList)
                    {
                        items.Add(item.cat_id, item);
                    }

                    connection.BeginTransaction();
                    foreach (var item in list)
                    {
                        if (!item.UniqueId.Equals("169EBC7C-6112-4566-93B9-2869C3D3A112") && !item.UniqueId.Equals("E54AF059-5C22-4FAF-BC40-4169BF74C020"))
                        {
                            if (items.ContainsKey(item.UniqueId))
                            {
                                UpdateCommand command = new UpdateCommand("categories", new EqFilterParam("cat_id", item.UniqueId.ToUpper()),
                               new BasicParam("cat_name", item.GroupName.Trim()),
                               new BasicParam("cat_parent", item.ParentUniqueIdString.ToUpper()),
                               new BasicParam("cat_left", item.NLeft.ToString()),
                               new BasicParam("cat_right", item.NRight.ToString()),
                               new BasicParam("is_removed", (item.IsRemoved) ? "1" : "0"),
                               new BasicParam("cat_depth", item.NLevel.ToString()));
                                var query = connection.CreateCommand(command.GetCommand());
                                int t = query.ExecuteNonQuery();
                            }
                            else
                            {
                                InsertCommand command = new InsertCommand("categories", new BasicParam("cat_id", item.UniqueId.ToUpper()),
                               new BasicParam("cat_name", item.GroupName.Trim()),
                               new BasicParam("cat_parent", item.ParentUniqueIdString.ToUpper()),
                               new BasicParam("cat_left", item.NLeft.ToString()),
                               new BasicParam("is_removed", (item.IsRemoved) ? "1" : "0"),
                               new BasicParam("cat_right", item.NRight.ToString()),
                               new BasicParam("cat_depth", item.NLevel.ToString()));
                                string qq = command.GetCommand();
                                var query = connection.CreateCommand(qq);
                                int t = query.ExecuteNonQuery();
                            }
                        }
                    }
                    connection.Commit();
                    await SyncManager.AddLogAsync(SyncManager.GroupsTbl);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static GroupLeftRightModel GetLeftRight(string catId)
        {
            try
            {
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    SQLite.SQLiteCommand query;
                    if (catId == null)
                        query = connection.CreateCommand("SELECT min(cat_left) as left , max(cat_right) as right FROM categories");
                    else
                        query = connection.CreateCommand(String.Format("SELECT cat_left as left, cat_right as right FROM categories WHERE cat_id ='{0}'", catId));
                    var lr = query.ExecuteQuery<GroupLeftRightModel>();
                    return lr.First();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<List<CategoryInfoModel>> GetFirstLevelAsync()
        {
            try
            {
                var query = new StringQuery("SELECT * FROM categories WHERE cat_depth = 1 AND is_removed = 0");
                query.Unlimited = true;
                var list = await BaseDataAdapter<CategoryInfoModel>.GetListAsync(query);
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<List<CategoryInfoModel>> GetCategoriesAsync(string catId)
        {
            try
            {
                var query = new StringQuery(string.Format("SELECT * FROM categories WHERE cat_parent = '{0}' AND is_removed = 0", catId));
                var list = await BaseDataAdapter<CategoryInfoModel>.GetListAsync(query);
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static async Task<CategoryInfoModel> GetParentCategoryAsync(string catId)
        {
            try
            {
                var current = await BaseDataAdapter<CategoryInfoModel>.GetItemAsync(new StringQuery(string.Format("SELECT * FROM categories WHERE cat_id = '{0}'", catId)));
                var parent = await BaseDataAdapter<CategoryInfoModel>.GetItemAsync(new StringQuery(string.Format("SELECT * FROM categories WHERE cat_id = '{0}'", current.cat_parent)));
                return parent;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<CategoryInfoModel> GetCategoryInfoAsync(string catId)
        {
            try
            {
                var c = await BaseDataAdapter<CategoryInfoModel>.GetItemAsync(new StringQuery(string.Format("SELECT * FROM categories WHERE cat_id = '{0}'", catId)));
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

        public static async Task<string> GetFullNameAsync(string catId)
        {
            try
            {
                var current = await BaseDataAdapter<CategoryInfoModel>.GetItemAsync(new StringQuery(string.Format("SELECT * FROM categories WHERE cat_id = '{0}'", catId)));
                if (current != null)
                {
                    var parent = await BaseDataAdapter<CategoryInfoModel>.GetItemAsync(new StringQuery(string.Format("SELECT * FROM categories WHERE cat_id = '{0}'", current.cat_parent)));
                    if (parent != null)
                        return parent.cat_name + " / " + current.cat_name;
                    else
                        return current.cat_name;
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }


        public static async Task<List<CategoryInfoModel>> SearchAsync(string value)
        {
            var q2 = new StringQuery(string.Format("SELECT * FROM categories WHERE cat_name LIKE '%{0}%' AND is_removed = 0", value));
            var groups = await BaseDataAdapter<CategoryInfoModel>.GetListAsync(q2);
            return groups;
        }
    }
}
