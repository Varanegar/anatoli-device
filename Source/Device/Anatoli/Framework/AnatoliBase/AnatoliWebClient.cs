using Anatoli.Framework.Model;
using RestSharp.Portable;
using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;

namespace Anatoli.Framework.AnatoliBase
{
    public abstract class AnatoliWebClient
    {
        public abstract bool IsOnline();
        AnatoliTokenInfo _userTokenInfo;
        AnatoliTokenInfo _appTokenInfo;
        public async Task<bool> LoadTokenFileAsync()
        {
            try
            {
                byte[] cipherText = await Task.Run(() =>
                {
                    byte[] result = AnatoliClient.GetInstance().FileIO.ReadAllBytes(AnatoliClient.GetInstance().FileIO.GetDataLoction(), Configuration.tokenInfoFile);
                    return result;
                });
                byte[] plainText = Crypto.DecryptAES(cipherText);
                string tokenString = Encoding.Unicode.GetString(plainText, 0, plainText.Length);
                string[] tokenStringFields = tokenString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if (tokenStringFields.Length == 4)
                {
                    if (tokenStringFields[0] != "null")
                    {
                        _userTokenInfo = new AnatoliTokenInfo();
                        _userTokenInfo.AccessToken = tokenStringFields[0];
                        _userTokenInfo.ExpiresIn = long.Parse(tokenStringFields[1]);
                    }
                    if (tokenStringFields[2] != "null")
                    {
                        _appTokenInfo = new AnatoliTokenInfo();
                        _appTokenInfo.AccessToken = tokenStringFields[2];
                        _appTokenInfo.ExpiresIn = long.Parse(tokenStringFields[3]);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }


        public async Task<bool> SaveTokenFileAsync()
        {
            string content = "";
            if (_userTokenInfo != null)
                if (_userTokenInfo.AccessToken != null)
                    content += _userTokenInfo.AccessToken + Environment.NewLine + _userTokenInfo.ExpiresIn.ToString() + Environment.NewLine;
                else
                    content += "null" + Environment.NewLine + "null" + Environment.NewLine;
            else
                content += "null" + Environment.NewLine + "null" + Environment.NewLine;
            if (_appTokenInfo != null)
                if (_appTokenInfo.AccessToken != null)
                    content += _appTokenInfo.AccessToken + Environment.NewLine + _appTokenInfo.ExpiresIn;
                else
                    content += "null" + Environment.NewLine + "null" + Environment.NewLine;
            else
                content += "null" + Environment.NewLine + "null" + Environment.NewLine;

            try
            {
                bool wResult = await Task.Run(() =>
                {
                    var cipherText = Crypto.EncryptAES(content);
                    bool result = AnatoliClient.GetInstance().FileIO.WriteAllBytes(cipherText, AnatoliClient.GetInstance().FileIO.GetDataLoction(), Configuration.tokenInfoFile);
                    return true;
                });
                return wResult;
            }
            catch (Exception)
            {
                return false;
            }
        }

        async Task<AnatoliTokenInfo> GetTokenAsync(TokenType tokenType)
        {
            if (tokenType == TokenType.UserToken)
            {
                if (_userTokenInfo == null)
                {
                    await LoadTokenFileAsync();
                    if (_userTokenInfo == null)
                    {
                        await RefreshTokenAsync();
                        if (_userTokenInfo == null)
                        {
                            throw new ServerUnreachableException();
                        }
                    }
                }
                return _userTokenInfo;
            }
            else
            {
                if (_appTokenInfo == null)
                {
                    await LoadTokenFileAsync();
                    if (_appTokenInfo == null)
                    {
                        await RefreshTokenAsync();
                        if (_appTokenInfo == null)
                        {
                            throw new ServerUnreachableException();
                        }
                    }
                }
                return _appTokenInfo;
            }
        }

        RestRequest CreateRequest(AnatoliTokenInfo tokenInfo, string requestUrl, HttpMethod method, params Tuple<string, string>[] parameters)
        {
            var request = new RestRequest(requestUrl, method);
            request.AddParameter("Authorization", string.Format("Bearer {0}", tokenInfo.AccessToken), ParameterType.HttpHeader);
            request.AddHeader("Accept", "application/json");
            //request.AddHeader("OwnerKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");
            request.AddHeader("OwnerKey", "79A0D598-0BD2-45B1-BAAA-0A9CF9EFF240");
            request.AddHeader("DataOwnerKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");
            request.AddHeader("DataOwnerCenterKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    Parameter p = new Parameter();
                    p.Name = item.Item1;
                    p.Value = item.Item2;
                    request.AddParameter(p);
                }
            }
            return request;
        }
        RestRequest CreateRequest(AnatoliTokenInfo tokenInfo, string requestUrl, HttpMethod method, Object obj, params Tuple<string, string>[] parameters)
        {
            var request = new RestRequest(requestUrl, method);
            request.AddParameter("Authorization", string.Format("Bearer {0}", tokenInfo.AccessToken), ParameterType.HttpHeader);
            request.AddHeader("Accept", "application/json");
            //request.AddHeader("OwnerKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");
            request.AddHeader("OwnerKey", "79A0D598-0BD2-45B1-BAAA-0A9CF9EFF240");
            request.AddHeader("DataOwnerKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");
            request.AddHeader("DataOwnerCenterKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    Parameter p = new Parameter();
                    p.Name = item.Item1;
                    p.Value = item.Item2;
                    request.AddParameter(p);
                }
            }
            request.AddJsonBody(obj);
            return request;
        }

        RestRequest CreateRequest(AnatoliTokenInfo tokenInfo, string requestUrl, HttpMethod method, object obj)
        {

            var request = new RestRequest(requestUrl, method);
            request.AddParameter("Authorization", string.Format("Bearer {0}", tokenInfo.AccessToken), ParameterType.HttpHeader);
            //request.AddHeader("OwnerKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");
            request.AddHeader("OwnerKey", "79A0D598-0BD2-45B1-BAAA-0A9CF9EFF240");
            request.AddHeader("DataOwnerKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");
            request.AddHeader("DataOwnerCenterKey", "3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C");

            request.AddJsonBody(obj);
            return request;
        }
        public async Task RefreshTokenAsync(TokenRefreshParameters parameters = null)
        {
            var tclient = new Thinktecture.IdentityModel.Client.OAuth2Client(new Uri(Configuration.WebService.PortalAddress + Configuration.WebService.OAuthTokenUrl));
            try
            {
                tclient.Timeout = new TimeSpan(0, 0, 30);
                var oauthresult = await tclient.RequestResourceOwnerPasswordAsync(Configuration.AppMobileAppInfo.UserName, Configuration.AppMobileAppInfo.Password, Configuration.AppMobileAppInfo.Scope);
                if (oauthresult.AccessToken == null)
                    throw new TokenException();
                _appTokenInfo = new AnatoliTokenInfo();
                _appTokenInfo.AccessToken = oauthresult.AccessToken;
                _appTokenInfo.ExpiresIn = oauthresult.ExpiresIn;

                if (parameters != null)
                {
                    oauthresult = await tclient.RequestResourceOwnerPasswordAsync(parameters.UserName, parameters.Password, parameters.Scope);
                    if (oauthresult.AccessToken == null)
                        if (oauthresult.Raw.Equals("{\"error\":\"خطا در ورود به سیستم\",\"error_description\":\"تایید نام کاربری دریافت نشده است\"}"))
                            throw new UnConfirmedUserException();
                        else
                            throw new TokenException();
                    _userTokenInfo = new AnatoliTokenInfo();
                    _userTokenInfo.AccessToken = oauthresult.AccessToken;
                    _userTokenInfo.ExpiresIn = oauthresult.ExpiresIn;
                }

                await SaveTokenFileAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<Result> SendPostRequestAsync<Result>(TokenType tokenType, string requestUri, params Tuple<string, string>[] parameters)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post, parameters);
            return await ExecRequestAsync<Result>(client, request);
        }
        public async Task<Result> SendPostRequestAsync<Result>(TokenType tokenType, string requestUri, System.Threading.CancellationTokenSource cancelToken, params Tuple<string, string>[] parameters)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post, parameters);
            return await ExecRequestAsync<Result>(client, request, cancelToken);
        }
        public async Task SendPostRequestAsync(TokenType tokenType, string requestUri, params Tuple<string, string>[] parameters)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post, parameters);
            await ExecRequestAsync(client, request);
        }
        public async Task<Result> SendPostRequestAsync<Result>(TokenType tokenType, string requestUri, object obj)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post, obj);
            return await ExecRequestAsync<Result>(client, request);
        }
        public async Task<Result> SendPostRequestAsync<Result>(TokenType tokenType, string requestUri, object obj, System.Threading.CancellationTokenSource cancelToken)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post, obj);
            return await ExecRequestAsync<Result>(client, request, cancelToken);
        }
        public async Task<Result> SendPostRequestAsync<Result>(TokenType tokenType, string requestUri, object obj, params Tuple<string, string>[] parameters)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post, obj, parameters);
            return await ExecRequestAsync<Result>(client, request);
        }
        public async Task<Result> SendPostRequestAsync<Result>(TokenType tokenType, string requestUri, object obj, System.Threading.CancellationTokenSource cancelToken, params Tuple<string, string>[] parameters)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post, obj, parameters);
            return await ExecRequestAsync<Result>(client, request, cancelToken);
        }
        public async Task<Result> SendFileAsync<Result>(TokenType tokenType, string requestUri, Byte[] bytes, string fileName, System.Threading.CancellationTokenSource cancelToken)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Post);
            request.AddFile(fileName, bytes, "ttt");
            return await ExecRequestAsync<Result>(client, request, cancelToken);
        }
        public async Task<Result> SendGetRequestAsync<Result>(TokenType tokenType, string requestUri, System.Threading.CancellationTokenSource cancelToken, params Tuple<string, string>[] parameters)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Get, parameters);
            return await ExecRequestAsync<Result>(client, request, cancelToken);
        }
        public async Task<Result> SendGetRequestAsync<Result>(TokenType tokenType, string requestUri, params Tuple<string, string>[] parameters)
        {
            var client = new RestClient(Configuration.WebService.PortalAddress);
            RestRequest request;
            var token = await GetTokenAsync(tokenType);
            request = CreateRequest(token, requestUri, HttpMethod.Get, parameters);
            return await ExecRequestAsync<Result>(client, request);
        }

        async Task<Result> ExecRequestAsync<Result>(RestClient client, RestRequest request, System.Threading.CancellationTokenSource cancelToken)
        {
            client.IgnoreResponseStatusCode = true;
            RestSharp.Portable.IRestResponse respone;
            if (cancelToken != null)
                respone = await client.Execute(request, cancelToken.Token);
            else
                respone = await client.Execute(request);
            CheckResponseStatus(respone);
            JsonDeserializer deserializer = new JsonDeserializer();
            try
            {
                var result = deserializer.Deserialize<Result>(respone);
                return result;
            }
            catch (Exception e)
            {
                throw new AnatoliWebClientException(respone.ResponseUri.ToString(), "Deserializer got inproper jason model at " + respone.ResponseUri.ToString() + ": " + Encoding.UTF8.GetString(respone.RawBytes, 0, respone.RawBytes.Length), e);
            }
        }
        async Task<Result> ExecRequestAsync<Result>(RestClient client, RestRequest request)
        {
            client.IgnoreResponseStatusCode = true;
            RestSharp.Portable.IRestResponse respone = await client.Execute(request);
            CheckResponseStatus(respone);
            JsonDeserializer deserializer = new JsonDeserializer();
            try
            {
                var result = deserializer.Deserialize<Result>(respone);
                return result;
            }
            catch (Exception e)
            {
                throw new AnatoliWebClientException(respone.ResponseUri.ToString(), "Deserializer got inproper jason model at " + respone.ResponseUri.ToString() + ": " + Encoding.UTF8.GetString(respone.RawBytes, 0, respone.RawBytes.Length), e);
            }
        }
        async Task ExecRequestAsync(RestClient client, RestRequest request)
        {
            client.IgnoreResponseStatusCode = true;
            RestSharp.Portable.IRestResponse respone = await client.Execute(request);
            CheckResponseStatus(respone);
        }
        void CheckResponseStatus(IRestResponse response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadGateway:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.BadRequest:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.Conflict:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.Continue:
                    break;
                case HttpStatusCode.Created:
                    break;
                case HttpStatusCode.ExpectationFailed:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.Forbidden:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.Found:
                    break;
                case HttpStatusCode.GatewayTimeout:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.Gone:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.HttpVersionNotSupported:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.InternalServerError:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.LengthRequired:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.MethodNotAllowed:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.Moved:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.MultipleChoices:
                    break;
                case HttpStatusCode.NoContent:
                    break;
                case HttpStatusCode.NonAuthoritativeInformation:
                    break;
                case HttpStatusCode.NotAcceptable:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.NotFound:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.NotImplemented:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.NotModified:
                    break;
                case HttpStatusCode.OK:
                    break;
                case HttpStatusCode.PartialContent:
                    break;
                case HttpStatusCode.PaymentRequired:
                    break;
                case HttpStatusCode.PreconditionFailed:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.ProxyAuthenticationRequired:
                    throw new AnatoliWebClientException(response);
                //case HttpStatusCode.Redirect:
                //    break;
                case HttpStatusCode.RedirectKeepVerb:
                    break;
                case HttpStatusCode.RedirectMethod:
                    break;
                case HttpStatusCode.RequestEntityTooLarge:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.RequestTimeout:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.RequestUriTooLong:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.RequestedRangeNotSatisfiable:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.ResetContent:
                    throw new AnatoliWebClientException(response);
                //case HttpStatusCode.SeeOther:
                //    break;
                case HttpStatusCode.ServiceUnavailable:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.SwitchingProtocols:
                    throw new AnatoliWebClientException(response);
                //case HttpStatusCode.TemporaryRedirect:
                //    break;
                case HttpStatusCode.Unauthorized:
                    OnTokenExpire();
                    throw new TokenException();
                case HttpStatusCode.UnsupportedMediaType:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.Unused:
                    throw new AnatoliWebClientException(response);
                case HttpStatusCode.UpgradeRequired:
                    break;
                case HttpStatusCode.UseProxy:
                    break;
                default:
                    break;
            }
        }

        public void OnTokenExpire()
        {
            if (TokenExpire != null)
            {
                TokenExpire.Invoke(this, new EventArgs());
            }
        }

        public EventHandler TokenExpire;
    }
    public class AnatoliTokenInfo
    {
        public string AccessToken { get; set; }
        public long ExpiresIn { get; set; }
    }
    public enum TokenType
    {
        AppToken = 1,
        UserToken
    }
    public class TokenRefreshParameters
    {
        public TokenRefreshParameters(string userName, string passWord, string scope)
        {
            UserName = userName;
            Password = passWord;
            Scope = scope;
        }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Scope { get; set; }
    }
    public class ServerUnreachableException : Exception
    {

    }
    public class UnConfirmedUserException : Exception
    {

    }
    public class TokenException : Exception
    {

    }
    public class AnatoliWebClientException : Exception
    {
        public string RequestUri { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public AnatoliMetaInfo MetaInfo { get; private set; }
        public AnatoliWebClientException(string uri, string message = "Web Exception", Exception ex = null)
            : base(message, ex)
        {
            RequestUri = uri;
            StatusCode = 0;
            MetaInfo = new AnatoliMetaInfo();
        }
        public AnatoliWebClientException(IRestResponse response)
            : base("Web Exception, Http status: " + response.StatusCode.ToString() + " at :" + response.ResponseUri.ToString() + "" +
            Encoding.UTF8.GetString(response.RawBytes, 0, response.RawBytes.Length))
        {
            RequestUri = response.ResponseUri.ToString();
            StatusCode = response.StatusCode;
            JsonDeserializer deserializer = new JsonDeserializer();
            MetaInfo = deserializer.Deserialize<AnatoliMetaInfo>(response);
            if (MetaInfo != null)
            {
                MetaInfo.ModelState = MetaInfo.modelState;
            }
        }
    }
    public class ConnectionFailedException : AnatoliWebClientException
    {
        public ConnectionFailedException(string uri, string message = "Connection Failed", Exception ex = null) : base(uri, message, ex) { }
    }
    public class NoInternetAccessException : AnatoliWebClientException
    {
        public NoInternetAccessException(string uri, string message = "No Internet Access", Exception ex = null) : base(uri, message, ex) { }
    }

    public class BaseWebClientResult : BaseViewModel
    {

    }

    public class AnatoliMetaInfo : BaseViewModel
    {
        public string message { get; set; }
        public Dictionary<string, string[]> modelState { get; set; }
    }
}
