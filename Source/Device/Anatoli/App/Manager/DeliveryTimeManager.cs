using Anatoli.App.Model;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework.Manager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Manager
{
    public class DeliveryTimeManager : BaseManager<DeliveryTimeModel>
    {
        public static List<DeliveryTimeModel> GetAvailableDeliveryTimes(string storeId, DateTime now, string deliveryType)
        {
            List<DeliveryTimeModel> times = new List<DeliveryTimeModel>();
            SelectQuery query;
            if (deliveryType.Equals(DeliveryTypeModel.DeliveryType.Equals(deliveryType)))
                query = new SelectQuery("StoreCalendar", new EqFilterParam("StoreId", storeId), new EqFilterParam("CalendarTypeValueId", StoreCalendarModel.StoreActivedeliveryTime));
            else
                query = new SelectQuery("StoreCalendar", new EqFilterParam("StoreId", storeId), new EqFilterParam("CalendarTypeValueId", StoreCalendarModel.StoreOpenTime));
            query.Unlimited = true;
            var result = AnatoliClient.GetInstance().DbClient.GetList<StoreCalendarModel>(query);
            var time = new TimeSpan(DateTime.Now.Hour, 30, 0);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var d = DateTime.Parse(item.Date, CultureInfo.CurrentCulture);
                    if (d.DayOfYear == DateTime.Now.DayOfYear)
                    {
                        for (TimeSpan i = (time > item.FromTime ? time : item.FromTime); i < item.ToTime; i += TimeSpan.FromMinutes(30))
                        {
                            var t = new DeliveryTimeModel();
                            t.timespan = i;
                            t.UniqueId = Guid.NewGuid();
                            times.Add(t);
                        }
                    }
                }
            }
            return times;
        }
        public override int UpdateItem(DeliveryTimeModel model)
        {
            throw new NotImplementedException();
        }
    }
}
