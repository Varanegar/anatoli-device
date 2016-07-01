using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class DeliveryTypeModel : BaseModel
    {
        public static string DeliveryType = "CE4AEE25-F8A7-404F-8DBA-80340F7339CC";
        public static string PickupType = "BE2919AB-5564-447A-BE49-65A81E6AF712";
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
