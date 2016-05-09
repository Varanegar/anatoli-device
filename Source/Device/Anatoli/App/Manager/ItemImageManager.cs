using Anatoli.App.Model;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.DataAdapter;
using Anatoli.Framework.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Manager
{
    public class ItemImageManager : BaseManager<ItemImageViewModel>
    {
        public static async Task SyncDataBaseAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.ImagesTbl);
                List<ItemImageViewModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ItemImageViewModel>>(Configuration.WebService.PortalAddress,TokenType.AppToken, Configuration.WebService.ImageManager.Images);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ItemImageViewModel>>(TokenType.AppToken, Configuration.WebService.ImageManager.ImagesAfter + "?dateafter=" + lastUpdateTime.ToString(), data);
                }
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    connection.BeginTransaction();
                    foreach (var item in list)
                    {
                        if (item.ImageType == ItemImageViewModel.ProductImageType)
                        {
                            UpdateCommand command = new UpdateCommand("products", new EqFilterParam("product_id", item.BaseDataId.ToUpper()), new BasicParam("image", item.ImageName.ToUpper()));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();
                        }
                        if (item.ImageType == ItemImageViewModel.ProductSiteGroupImageType)
                        {
                            UpdateCommand command = new UpdateCommand("categories", new EqFilterParam("cat_id", item.BaseDataId.ToUpper()), new BasicParam("cat_image", item.ImageName.ToUpper()));
                            var query = connection.CreateCommand(command.GetCommand());
                            int t = query.ExecuteNonQuery();

                            command = new UpdateCommand("products", new EqFilterParam("product_id", item.BaseDataId.ToUpper()), new BasicParam("image", item.ImageName.ToUpper()));
                            query = connection.CreateCommand(command.GetCommand());
                            t = query.ExecuteNonQuery();
                        }
                    }
                    connection.Commit();
                    await SyncManager.AddLogAsync(SyncManager.ImagesTbl);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
