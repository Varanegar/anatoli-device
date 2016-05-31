using Anatoli.Framework.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class StoreDataModel : BaseViewModel
    {
        public string store_name { get; set; }
        public string store_address { get; set; }
        public string store_zone { get; set; }
        public string store_city { get; set; }
        public string store_province { get; set; }
        public string store_id { get; set; }
        public int selected { get; set; }
        public string store_tel { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public bool support_app_order { get; set; }
        public string location
        {
            get
            {
                return (lat != 0 && lng != 0) ? lat.ToString() + "," + lng.ToString() : null;
            }
        }
        public float distance { get; set; }
    }
}
