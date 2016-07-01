using Anatoli.Framework.Model;
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
            //public static string PortalAddress = "http://192.168.201.127:8081";
            //public static string PortalAddress = "http://46.209.104.2:7000";
            public static string PortalAddress = "http://parastoo.varanegar.com:7000";
            //public static string PortalAddress = "http://46.32.2.234:8081";
            //public static string PortalAddress = "http://79.175.166.186/";
            //public static  string PortalAddress = "http://192.168.201.46/";
            //public static string PortalAddress = "http://192.168.0.160:8081/";
            public static readonly string OAuthTokenUrl = "/oauth/token";
            public static readonly string ParseInfo = "api/TestAuth/setParsInfo";
            public static readonly string BaseDatas = "api/gateway/basedata/basedatas/compress/";
            public struct ImageManager
            {
                public static readonly string ImagesAfter = "/api/imageManager/images/compress/after/";
                public static readonly string Images = "/api/imageManager/images/compress/";
                public static readonly string ImageSave = "/api/imageManager/save/";
            }
            public struct Products
            {
                public static readonly string ProductsListAfter = "/api/gateway/product/products/compress/after/";
                public static readonly string ProductsList = "/api/gateway/product/products/compress/";
                public static readonly string ProductGroupsAfter = "/api/gateway/product/productgroups/compress/after/";
                public static readonly string ProductGroups = "/api/gateway/product/productgroups/compress/";
                public static string ProductsTags = "/api/gateway/product/producttags/";
                public static string ProductsTagsAfter = "/api/gateway/product/producttags/after/";
                public static string ProductsTagValuesAfter = "/api/gateway/product/producttagvalues/after/";
            }
            public struct Stores
            {
                public static readonly string StoresViewAfter = "/api/gateway/store/stores/compress/after/";
                public static readonly string StoresView = "/api/gateway/store/stores/compress/";
                public static readonly string PricesViewAfter = "/api/gateway/store/storepricelist/compress/after/";
                public static readonly string PricesView = "/api/gateway/store/storepricelist/compress/";
                public static readonly string OnHandAfter = "/api/gateway/store/storeOnhand/compress/after/";
                public static readonly string OnHand = "/api/gateway/store/storeOnhand/compress/";
                public static readonly string StoreCalendar = "/api/gateway/store/storecalendar/";
            }
            public struct Purchase
            {
                public static readonly string OrdersList = "/api/gateway/purchaseorder/bycustomerid/";
                public static readonly string OrderItems = "/api/gateway/purchaseorder/lineitem/";
                public static readonly string OrderHistory = "/api/gateway/purchaseorder/statushistory/";
                public static readonly string CalcPromo = "/api/gateway/purchaseorder/calcpromo/";
                public static readonly string Create = "/api/gateway/purchaseorder/create/";
            }
            public struct Users
            {
                public static readonly string UserCreateUrl = "/api/accounts/create";
                public static readonly string UserAuthUrl = "/api/accounts/create";
                public static readonly string ConfirmMobile = "/api/accounts/confirmmobile/";
                public static readonly string ResendConfirmCode = "/api/accounts/resendpasscode/";
                public static readonly string ResetPassWord = "/api/accounts/resetpassword/";
                public static readonly string ResetPasswordByCode = "/api/accounts/resetpasswordbycode/";
                public static readonly string SendPassCode = "/api/accounts/sendpasscode/";
                public static readonly string ViewProfileUrl = "/api/gateway/customer/customers";
                public static readonly string SaveProfileUrl = "/api/gateway/customer/savesingle";
                public static readonly string ShoppingCardSave = "/api/gateway/incompletepurchaseorder/save";
                public static readonly string ShoppingCardView = "/api/gateway/incompletepurchaseorder";
                public static readonly string FavoritSaveItem = "/api/gateway/basket/basketitem/save";
                public static readonly string FavoritDeleteItem = "/api/gateway/basket/basketitem/delete";
                public static readonly string FavoritView = "/api/gateway/basket/customerbaskets/bybasket";
                public static readonly string BasketView = "/api/gateway/basket/customerbaskets/bycustomer";
                public static readonly string ChangePasswordUri = "/api/accounts/changepassword/";
            }
            public static readonly string CityRegionAfter = "/api/gateway/base/region/cityregions/compress/after/";
            public static readonly string CityRegion = "/api/gateway/base/region/cityregions/compress/";
        }

        public static Setting ReadSetting(string name)
        {
            return AnatoliClient.GetInstance().DbClient.GetItem<Setting>(new StringQuery(string.Format("SELECT * FROM Setting WHERE Name='{0}'", name)));
        }
        public static List<Setting> ReadSetting()
        {
            return AnatoliClient.GetInstance().DbClient.GetList<Setting>(new StringQuery("SELECT * FROM Setting"));
        }
        public static void SaveSetting(Setting setting)
        {
            if (ReadSetting(setting.Name) == null)
                AnatoliClient.GetInstance().DbClient.UpdateItem(new StringQuery(string.Format("INSERT INTO Setting (Name,Value) VALUES ('{0}','{1}')", setting.Name, setting.Value)));
            else
                AnatoliClient.GetInstance().DbClient.UpdateItem(new StringQuery(string.Format("UPDATE Setting SET Value='{1}' WHERE Name='{0}'", setting.Name, setting.Value)));
        }
        public static void SaveSetting(List<Setting> settings)
        {
            foreach (var setting in settings)
            {
                SaveSetting(setting);
            }
        }
        public class Setting : BaseModel
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public static string Camera = "Camera";
        }
    }
}
