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
    public class BaseTypeManager : BaseManager<BaseTypeViewModel>
    {
        public static async Task SyncDataBaseAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = SyncManager.GetLog(SyncManager.BaseTypesTbl);
                List<BaseTypeViewModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BaseTypeViewModel>>(TokenType.AppToken, Configuration.WebService.BaseDatas, cancellationTokenSource, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BaseTypeViewModel>>(TokenType.AppToken, Configuration.WebService.BaseDatas, data, cancellationTokenSource, true);
                }

                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("DeliveryType"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("PayType"));
                AnatoliClient.GetInstance().DbClient.BeginTransaction();
                foreach (var item in list)
                {
                    if (item.UniqueId.Equals(BaseTypeViewModel.DeliveryType))
                    {
                        foreach (var value in item.BaseValues)
                        {
                            InsertCommand command = new InsertCommand("DeliveryType", new BasicParam("Name", value.BaseValueName),
                             new BasicParam("UniqueId", value.UniqueId));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                    }
                    else if (item.UniqueId.Equals(BaseTypeViewModel.PayType))
                    {
                        foreach (var value in item.BaseValues)
                        {
                            InsertCommand command = new InsertCommand("PayType", new BasicParam("Name", value.BaseValueName),
                             new BasicParam("UniqueId", value.UniqueId.ToString().ToUpper()));
                            int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                        }
                    }
                }
                AnatoliClient.GetInstance().DbClient.CommitTransaction();
                SyncManager.AddLog(SyncManager.BaseTypesTbl);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override int UpdateItem(BaseTypeViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
