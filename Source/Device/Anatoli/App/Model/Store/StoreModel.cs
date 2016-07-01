using Anatoli.Framework.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class StoreModel : BaseModel
    {
        public string storeName { get; set; }
        public int selected { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string address { get; set; }
        public string Phone { get; set; }
        public bool supportAppOrder { get; set; }
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
