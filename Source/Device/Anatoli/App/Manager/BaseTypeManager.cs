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
    public class BaseTypeManager : BaseManager<BaseTypeViewModel>
    {
        public static async Task SyncDataBaseAsync(System.Threading.CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                var lastUpdateTime = await SyncManager.GetLogAsync(SyncManager.BaseTypesTbl);
                List<BaseTypeViewModel> list;
                if (lastUpdateTime == DateTime.MinValue)
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BaseTypeViewModel>>(TokenType.AppToken, Configuration.WebService.BaseDatas, cancellationTokenSource, true);
                else
                {
                    var data = new RequestModel.BaseRequestModel();
                    data.dateAfter = lastUpdateTime.ToString();
                    list = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<BaseTypeViewModel>>(TokenType.AppToken, Configuration.WebService.BaseDatas, data, cancellationTokenSource, true);
                }

                await DataAdapter.UpdateItemAsync(new DeleteCommand("delivery_types"));
                await DataAdapter.UpdateItemAsync(new DeleteCommand("pay_types"));
                using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                {
                    connection.BeginTransaction();
                    foreach (var item in list)
                    {
                        if (item.UniqueId.ToUpper() == BaseTypeViewModel.DeliveryType)
                        {
                            foreach (var value in item.BaseValues)
                            {
                                InsertCommand command = new InsertCommand("delivery_types", new BasicParam("name", value.BaseValueName),
                                 new BasicParam("id", value.UniqueId.ToString().ToUpper()));
                                var query = connection.CreateCommand(command.GetCommand());
                                int t = query.ExecuteNonQuery();
                            }
                        }
                        else if (item.UniqueId.ToUpper() == BaseTypeViewModel.PayType)
                        {
                            foreach (var value in item.BaseValues)
                            {
                                InsertCommand command = new InsertCommand("pay_types", new BasicParam("name", value.BaseValueName),
                                 new BasicParam("id", value.UniqueId.ToString().ToUpper()));
                                var query = connection.CreateCommand(command.GetCommand());
                                int t = query.ExecuteNonQuery();
                            }
                        }
                    }

                    connection.Commit();
                }
                await SyncManager.AddLogAsync(SyncManager.BaseTypesTbl);
            }
            catch (Exception e)
            {
                throw e;
            }
        }



    }
}
