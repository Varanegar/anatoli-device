using Anatoli.App.Model;
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
    public class ItemImageManager : BaseManager<ItemImageViewModel>
    {
        public static async Task SyncDataBaseAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.ImagesTbl);
                List<ItemImageViewModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ItemImageViewModel>>(Configuration.WebService.PortalAddress, TokenType.AppToken, Configuration.WebService.ImageManager.Images, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<ItemImageViewModel>>(TokenType.AppToken, Configuration.WebService.ImageManager.ImagesAfter + "?dateafter=" + lastUpdateTime.ToString(), data, true);
                }

                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list)
                {
                    if (item.ImageType.Equals(ItemImageViewModel.ProductImageType))
                    {
                        UpdateCommand command = new UpdateCommand("Product", new EqFilterParam("UniqueId", item.BaseDataId), new BasicParam("Image", item.ImageName));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                    if (item.ImageType.Equals(ItemImageViewModel.ProductSiteGroupImageType))
                    {
                        UpdateCommand command = new UpdateCommand("ProductGroup", new EqFilterParam("UniqueId", item.BaseDataId), new BasicParam("Image", item.ImageName));
                        int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);

                        //command = new UpdateCommand("products", new EqFilterParam("product_id", item.BaseDataId), new BasicParam("image", item.ImageName));
                        //int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.ImagesTbl);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override int UpdateItem(ItemImageViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
