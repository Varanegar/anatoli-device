using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anatoli.Framework.Manager;
using Anatoli.App.Model.AnatoliUser;
using Anatoli.Framework.AnatoliBase;
using PCLCrypto;
using Anatoli.App.Model;
using Anatoli.Framework.Model;
namespace Anatoli.App.Manager
{
    public class AnatoliUserManager
    {
        public static async Task<AnatoliUserModel> LoginAsync(string userName, string passWord)
        {
            if (!String.IsNullOrEmpty(userName))
            {
                userName = userName.Trim();
            }
            if (!String.IsNullOrEmpty(passWord))
            {
                passWord = passWord.Trim();
            }
            await AnatoliClient.GetInstance().WebClient.RefreshTokenAsync(new TokenRefreshParameters(userName, passWord, Configuration.AppMobileAppInfo.Scope));
            var userModel = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<AnatoliUserModel>(Configuration.WebService.PortalAddress, TokenType.AppToken, "/api/accounts/user/" + userName, false);
            if (userModel.IsValid)
            {
                await AnatoliUserManager.SaveUserInfoAsync(userModel);
                try
                {
                    var customer = await CustomerManager.DownloadCustomerAsync(userModel, null);
                    if (customer.IsValid)
                    {
                        await CustomerManager.SaveCustomerAsync(customer);
                        //await OrderManager.SyncOrdersAsync(customer.UniqueId);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
            return userModel;
        }
        public static async Task<BaseWebClientResult> RegisterAsync(string passWord, string confirmPassword, string tel, string email)
        {
            AnatoliUserModel user = new AnatoliUserModel();
            if (!String.IsNullOrEmpty(email))
            {
                email = email.Trim();
            }
            if (!String.IsNullOrEmpty(tel))
            {
                tel = tel.Trim();
            }
            if (!String.IsNullOrEmpty(passWord))
            {
                passWord = passWord.Trim();
            }

            user.Email = email;
            if (user.Email == "") user.Email = null;

            user.Username = tel;
            user.Password = passWord;
            user.ConfirmPassword = passWord;
            user.Mobile = tel;
            try
            {
                var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<BaseWebClientResult>(
                    TokenType.AppToken,
                Configuration.WebService.Users.UserCreateUrl,
                user,
                false
                );
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void CancelRegisterTask()
        {
            throw new NotImplementedException();
        }
        public static async Task SaveUserInfoAsync(AnatoliUserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("Could not save null user!");
            }
            string content = user.Email +
                Environment.NewLine + user.Username +
                Environment.NewLine + user.UniqueId +
                Environment.NewLine + user.Id;
            bool wResult = await Task.Run(() =>
                {
                    var cipherText = Crypto.EncryptAES(content);
                    bool result = AnatoliClient.GetInstance().FileClient.WriteAllBytes(cipherText, AnatoliClient.GetInstance().FileClient.GetDataLoction(), Configuration.userInfoFile);
                    return result;
                });
        }

        public static async Task<AnatoliUserModel> ReadUserInfoAsync()
        {
            try
            {
                byte[] cipherText = await Task.Run(() =>
                {
                    byte[] result = AnatoliClient.GetInstance().FileClient.ReadAllBytes(AnatoliClient.GetInstance().FileClient.GetDataLoction(), Configuration.userInfoFile);
                    return result;
                });
                byte[] plainText = Crypto.DecryptAES(cipherText);
                string userInfo = Encoding.Unicode.GetString(plainText, 0, plainText.Length);
                string[] userInfoFields = userInfo.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                AnatoliUserModel user = new AnatoliUserModel();
                user.Email = userInfoFields[0];
                user.Username = userInfoFields[1];
                user.UniqueId = Guid.Parse(userInfoFields[2]);
                user.Id = userInfoFields[3];
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Logout()
        {
            var fileIO = AnatoliClient.GetInstance().FileClient;
            try
            {
                fileIO.DeleteFile(fileIO.GetDataLoction(), Configuration.userInfoFile);
                fileIO.DeleteFile(fileIO.GetDataLoction(), Configuration.tokenInfoFile);
                fileIO.DeleteFile(fileIO.GetDataLoction(), Configuration.customerInfoFile);
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("Message"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("OrderItem"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("Order"));
                ShoppingCardManager.Clear();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static async Task<ChangePasswordBindingModel> ChangePassword(string p1, string p2)
        {
            var obj = new ChangePasswordBindingModel();
            obj.ConfirmPassword = p2;
            obj.NewPassword = p2;
            obj.OldPassword = p1;
            var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<ChangePasswordBindingModel>(TokenType.UserToken, Configuration.WebService.Users.ChangePasswordUri, obj, false);
            return result;
        }

        public static async Task<BaseWebClientResult> SendConfirmCode(string userName, string code)
        {
            var userRequestModel = new RequestModel.UserRequestModel();
            userRequestModel.username = userName;
            userRequestModel.code = code;
            var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<BaseWebClientResult>(TokenType.AppToken, Configuration.WebService.Users.ConfirmMobile, userRequestModel, false);
            return result;
        }

        public static async Task<BaseWebClientResult> RequestConfirmCode(string userName)
        {
            var userRequestModel = new RequestModel.UserRequestModel();
            userRequestModel.username = userName;
            var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<BaseWebClientResult>(TokenType.AppToken, Configuration.WebService.Users.ResendConfirmCode, userRequestModel, false);
            return result;
        }

        public static async Task<BaseWebClientResult> ResetPassword(string userName, string passWord)
        {
            var userRequestModel = new RequestModel.UserRequestModel();
            userRequestModel.username = userName;
            userRequestModel.password = passWord;
            var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<BaseWebClientResult>(TokenType.AppToken, Configuration.WebService.Users.ResetPassWord, userRequestModel, false);
            return result;
        }
        public static async Task<BaseWebClientResult> SendPassCode(string userName)
        {
            var userRequestModel = new RequestModel.UserRequestModel();
            userRequestModel.username = userName;
            var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<BaseWebClientResult>(TokenType.AppToken, Configuration.WebService.Users.SendPassCode, userRequestModel, false);
            return result;
        }
        public static async Task<BaseWebClientResult> ResetPasswordByCode(string userName, string passWord, string code)
        {
            var userRequestModel = new RequestModel.UserRequestModel();
            userRequestModel.username = userName;
            userRequestModel.password = passWord;
            userRequestModel.code = code;
            var result = await AnatoliClient.GetInstance().WebClient.SendPostRequestAsync<BaseWebClientResult>(TokenType.AppToken, Configuration.WebService.Users.ResetPasswordByCode, userRequestModel, false);
            return result;
        }
    }
}
