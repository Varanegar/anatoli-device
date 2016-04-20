using Anatoli.Framework.Helper;
using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anatoli.App.Model.Product;
using Anatoli.Framework.AnatoliBase;
using System.Net.Http;

namespace Anatoli.Framework.DataAdapter
{
    public class BaseDataAdapter<DataModel>
        where DataModel : BaseViewModel, new()
    {
        public static async Task<List<DataModel>> GetListAsync(RemoteQuery query)
        {
            if (AnatoliClient.GetInstance().WebClient.IsOnline())
            {
                try
                {
                    if (query.Method == HttpMethod.Get)
                    {
                        var response = await AnatoliClient.GetInstance().WebClient.SendGetRequestAsync<List<DataModel>>(
                        TokenType.AppToken,
                    query.WebServiceEndpoint,
                    query.cancellationTokenSource,
                    query.Params.ToArray()
                    );
                        return response;
                    }
                    else if (query.Method == HttpMethod.Post)
                    {
                        var response = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<List<DataModel>>(
                        TokenType.AppToken,
                    query.WebServiceEndpoint,
                    query.cancellationTokenSource,
                    query.Params.ToArray()
                    );
                        return response;
                    }

                }
                catch (Exception)
                {
                    throw;
                }
            }
            {
                throw new NoInternetAccessException(query.WebServiceEndpoint);
            }
        }
        public static async Task<List<DataModel>> GetListAsync(DBQuery query)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                    {
                        var command = connection.CreateCommand(query.GetCommand());
                        var result = command.ExecuteQuery<DataModel>();
                        return result;
                    }
                });
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static async Task<DataModel> GetItemAsync(RemoteQuery query)
        {
            try
            {
                var response = await AnatoliClient.GetInstance().WebClient.SendGetRequestAsync<DataModel>(
                    TokenType.UserToken,
                query.WebServiceEndpoint,
                query.Params.ToArray()
                );
                if (response != null)
                {
                    return response;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<DataModel> GetItemAsync(DBQuery query)
        {
            try
            {
                return await Task.Run((Func<DataModel>)(() =>
                {
                    using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                    {
                        var command = connection.CreateCommand(query.GetCommand());
                        var qResult = command.ExecuteQuery<DataModel>();
                        if (qResult.Count > 0)
                        {
                            return qResult.First();
                        }
                        return null;
                    }
                }));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    public class DataAdapter
    {
        public static async Task<int> UpdateItemAsync(DBQuery query)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using (var connection = AnatoliClient.GetInstance().DbClient.GetConnection())
                    {
                        var command = connection.CreateCommand(query.GetCommand());
                        var qResult = command.ExecuteNonQuery();
                        return qResult;
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
