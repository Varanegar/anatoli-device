using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model;
namespace Anatoli.App.Manager
{
    public class SyncManager
    {
        public static string StoresTbl = "Store";
        public static string BaseTypesTbl = "BaseType";
        public static string GroupsTbl = "ProductGroup";
        public static string CityRegionTbl = "CityRegion";
        public static string ImagesTbl = "images";
        public static string PriceTbl = "ProductPrice";
        public static string ProductTbl = "Product";
        public static string BasketTbl = "BasketItem";
        public static string OnHand = "StoreOnhand";
        public static string StoreCalendarTbl = "StoreCalendar";
        public static string UpdateCompleted = "CompleteUpdate";
        public static bool AddLog(string tableName)
        {
            try
            {
                InsertCommand command = new InsertCommand("updates", new BasicParam("table_name", tableName),
                       new BasicParam("update_time", ConvertToUnixTimestamp(DateTime.Now).ToString()));
                int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                if (t > 0) return true; else return false;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public static bool RemoveLog(string tableName)
        {
            try
            {
                DeleteCommand command = new DeleteCommand("updates", new EqFilterParam("table_name", tableName));
                int t = AnatoliClient.GetInstance().DbClient.UpdateItem(command);
                if (t > 0) return true; else return false;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public static DateTime GetLog(string tableName)
        {
            try
            {
                var time = AnatoliClient.GetInstance().DbClient.GetItem<UpdateTimeModel>(new StringQuery(String.Format("SELECT * FROM updates WHERE table_name = '{0}' order by update_time DESC LIMIT 0,1", tableName)));
                if (time != null)
                {
                    var d = double.Parse(time.update_time);
                    return ConvertFromUnixTimestamp(d);
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
        static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }


        static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static async Task SyncDatabase()
        {
            try
            {
                OnProgressChanged("دریافت اطلاعلات ...", 1);
                await BaseTypeManager.SyncDataBaseAsync(null);
                OnProgressChanged("اطلاعات اولیه ذخیره شد", 1);
                OnProgressChanged("دریافت اطلاعات شهرها و نواحی", 2);
                await CityRegionManager.SyncDataBaseAsync(null);
                OnProgressChanged("شهر ها و نواحی ذخیره شدند", 2);
                OnProgressChanged("دریافت اطلاعات فروشگاه و شعب", 3);
                await StoreManager.SyncDataBase(null);
                OnProgressChanged("اطلاعات فروشگاه ها ذخیره شد", 3);
                OnProgressChanged("دریافت اطلاعات دسته بندی کالا", 4);
                await ProductGroupManager.SyncDataBaseAsync(null);
                OnProgressChanged("اطلاعات دسته بندی کالاها ذخیره شد", 4);
                OnProgressChanged("دریافت اطلاعات محصولات", 5);
                await ProductManager.SyncProductsAsync(null);
                OnProgressChanged("محصولات جدید ذخیره شدند", 5);
                OnProgressChanged("دریافت تصاویر محصولات", 6);
                await ItemImageManager.SyncDataBaseAsync(null);
                OnProgressChanged("تصاویر ذخیره شدند", 6);
                OnProgressChanged("دریافت قیمت محصولات", 7);
                await ProductManager.SyncPricesAsync(null);
                OnProgressChanged("قیمت کالاها بروز شد", 7);
                OnProgressChanged("دریافت وضعیت موجودی محصولات", 8);
                await ProductManager.SyncOnHandAsync(null);
                OnProgressChanged("آخرین وضعیت موجودی محصولات ذخیره شد", 8);
                SyncManager.AddLog(SyncManager.UpdateCompleted);
                OnProgressChanged("فرایند بروزرسانی با موفقیت انجام شد.", 8);
                OnSyncCompleted();
            }
            catch (Exception ex)
            {
                OnProgressChanged("Sync process finished. Error: " + ex.Message, 0);
                throw ex;
            }
        }

        static void OnProgressChanged(string s, int step)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged.Invoke(s, step);
            }
        }
        public static event ProgressChangedEventHandler ProgressChanged;
        public delegate void ProgressChangedEventHandler(string status, int step);

        static void OnSyncCompleted()
        {
            if (SyncCompleted != null)
            {
                SyncCompleted.Invoke();
            }
        }
        public delegate void SyncCompletedEventHandler();
        public static event SyncCompletedEventHandler SyncCompleted;


        public static void ClearDatabase()
        {
            try
            {
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("DeliveryType"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("PayType"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("CityRegion"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("ProductPrice"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("Product"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("Store"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("ProductGroup"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("updates"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("StoreOnhand"));
                AnatoliClient.GetInstance().DbClient.UpdateItem(new DeleteCommand("StoreCalendar"));
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
