using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.AnatoliBase
{
    public class Configuration
    {
        public static readonly string parseAppId = "wUAgTsRuLdin0EvsBhPniG40O24i2nEGVFl8R5OI";
        public static readonly string parseDotNetKey = "G7guVuyx35bb4fBOwo7BVhlG2L2E2qKLQI0sLAe0";
        public static readonly string userInfoFile = "userInfo";
        public static readonly string customerInfoFile = "customerInfo";
        public static readonly string tokenInfoFile = "tk.info";
        public struct AppMobileAppInfo
        {
            public static readonly string UserName = "AnatoliMobileApp";
            public static readonly string Password = "Anatoli@App@Vn";
            //public static readonly string UserName = "petropay";
            //public static readonly string Password = "petropay";
            public static readonly string Scope = "79A0D598-0BD2-45B1-BAAA-0A9CF9EFF240,3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
        }
        public struct WebService
        {
            public static string PortalAddress = "http://192.168.201.205:8081";
            //public static string PortalAddress = "http://46.209.104.2:7000";
            //public static string PortalAddress = "http://217.218.53.71:7000";
            //public static string PortalAddress = "http://46.32.2.234:8081";
            //public static string PortalAddress = "http://79.175.166.186/";
            //public static  string PortalAddress = "http://192.168.201.46/";
            //public static string PortalAddress = "http://192.168.0.160:8081/";
            public static readonly string OAuthTokenUrl = "/oauth/token";
            public static readonly string ParseInfo = "api/TestAuth/setParsInfo";
            public static readonly string BaseDatas = "api/gateway/basedata/basedatas/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
            public struct ImageManager
            {
                public static readonly string ImagesAfter = "/api/imageManager/images/after/";
                public static readonly string Images = "/api/imageManager/images/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string ImageSave = "/api/imageManager/save/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
            }
            public struct Products
            {
                public static readonly string ProductsListAfter = "/api/gateway/product/products/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string ProductsList = "/api/gateway/product/products/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string ProductGroupsAfter = "/api/gateway/product/productgroups/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string ProductGroups = "/api/gateway/product/productgroups/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static string ProductsTags = "/api/gateway/product/producttags/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static string ProductsTagsAfter = "/api/gateway/product/producttags/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static string ProductsTagValuesAfter = "/api/gateway/product/producttagvalues/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
            }
            public struct Stores
            {
                public static readonly string StoresViewAfter = "/api/gateway/store/stores/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string StoresView = "/api/gateway/store/stores/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string PricesViewAfter = "/api/gateway/store/storepricelist/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string PricesView = "/api/gateway/store/storepricelist/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string OnHandAfter = "/api/gateway/store/storeOnhand/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string OnHand = "/api/gateway/store/storeOnhand/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string StoreCalendar = "/api/gateway/store/storecalendar/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
            }
            public struct Purchase
            {
                public static readonly string OrdersList = "/api/gateway/purchaseorder/bycustomerid/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string OrderHistory = "/api/gateway/purchaseorder/statushistory/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string CalcPromo = "/api/gateway/purchaseorder/calcpromo/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string Create = "/api/gateway/purchaseorder/create/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
            }
            public struct Users
            {
                public static readonly string UserCreateUrl = "/api/accounts/create";
                public static readonly string UserAuthUrl = "/api/accounts/create";
                public static readonly string ConfirmMobile = "/api/accounts/confirmmobile/";
                public static readonly string ResendConfirmCode = "/api/accounts/resendpasscode/";
                public static readonly string ResetPassWord = "/api/accounts/resetpassword/";
                public static readonly string ViewProfileUrl = "/api/gateway/customer/customers";
                public static readonly string SaveProfileUrl = "/api/gateway/customer/savesingle";
                public static readonly string ShoppingCardSave = "/api/gateway/incompletepurchaseorder/save?privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string ShoppingCardView = "/api/gateway/incompletepurchaseorder?privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C&customerId=E42F8E97-BFD2-4B18-8C06-6F3BCFF9A42D";
                public static readonly string FavoritSaveItem = "/api/gateway/basket/basketitem/save?privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string FavoritDeleteItem = "/api/gateway/basket/basketitem/delete?privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string FavoritView = "/api/gateway/basket/customerbaskets/bybasket?privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string BasketView = "/api/gateway/basket/customerbaskets/bycustomer?customerId=4ED993BB-B746-4AC7-B455-EBE91834364F&privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
                public static readonly string ChangePasswordUri = "/api/accounts/changepassword/?privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
            }
            public static readonly string CityRegionAfter = "/api/gateway/base/region/cityregions/after/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
            public static readonly string CityRegion = "/api/gateway/base/region/cityregions/?PrivateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C";
        }

        public static async Task ReadConfigFromFile()
        {
            try
            {
                var content = await Task.Run(() =>
                {
                    return AnatoliClient.GetInstance().FileIO.ReadAllText(AnatoliClient.GetInstance().FileIO.GetDataLoction(), "config");
                });
                if (!String.IsNullOrEmpty(content))
                {
                    WebService.PortalAddress = content;
                }
            }
            catch (Exception)
            {

            }

        }

        public static async Task SaveConfigToFile()
        {
            try
            {
                await Task.Run(() =>
                {
                    AnatoliClient.GetInstance().FileIO.WriteAllText(WebService.PortalAddress, AnatoliClient.GetInstance().FileIO.GetDataLoction(), "config");
                });
            }
            catch (Exception)
            {

            }

        }
    }
}
