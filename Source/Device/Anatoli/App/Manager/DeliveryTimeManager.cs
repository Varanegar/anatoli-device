using Anatoli.App.Model;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.DataAdapter;
using Anatoli.Framework.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Manager
{
    public class DeliveryTimeManager : BaseManager<DeliveryTimeModel>
    {
        public static async Task<List<DeliveryTimeModel>> GetAvailableDeliveryTimes(string storeId, DateTime now, string deliveryType)
        {
            List<DeliveryTimeModel> times = new List<DeliveryTimeModel>();
            SelectQuery query;
            if (deliveryType.Equals(DeliveryTypeModel.DeliveryType.Equals(deliveryType)))
                query = new SelectQuery("stores_calendar", new EqFilterParam("StoreId", storeId), new EqFilterParam("CalendarTypeValueId", StoreCalendarViewModel.StoreActivedeliveryTime));
            else
                query = new SelectQuery("stores_calendar", new EqFilterParam("StoreId", storeId), new EqFilterParam("CalendarTypeValueId", StoreCalendarViewModel.StoreOpenTime));
            query.Unlimited = true;
            var result = await BaseDataAdapter<StoreCalendarViewModel>.GetListAsync(query);
            var time = new TimeSpan(DateTime.Now.Hour, 30, 0);
            if (result.Count > 0)
            {
                if (time > result.First().FromTime)
                {
                    for (TimeSpan i = time; i < result.First().ToTime; i += TimeSpan.FromMinutes(30))
                    {
                        var t = new DeliveryTimeModel();
                        t.timespan = i;
                        t.UniqueId = Guid.NewGuid().ToString().ToUpper();
                        times.Add(t);
                    }
                }
            }
            return times;
        }
    }
}
