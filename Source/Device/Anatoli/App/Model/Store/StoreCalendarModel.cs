using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class StoreCalendarModel : BaseModel
    {
        public static string StoreOpenTime = "E4A73D47-8AC7-41D1-8EEA-21EDFBA90424";
        public static string StoreActivedeliveryTime = "D5C5E5BF-9235-48D8-B026-B7EB8DB14100";
        public string Date { get; set; }
        public string PDate { get; set; }
        public string FromTimeString { get; set; }
        public string ToTimeString { get; set; }
        public TimeSpan FromTime
        {
            get
            {
                var ts = TimeSpan.Parse(FromTimeString, CultureInfo.InvariantCulture);
                return ts;
            }
        }
        public TimeSpan ToTime
        {
            get
            {
                var ts = TimeSpan.Parse(ToTimeString, CultureInfo.InvariantCulture);
                return ts;
            }
        }
        public Guid StoreId { get; set; }
        public Guid CalendarTypeValueId { get; set; }
        public string Description { get; set; }
    }
}
