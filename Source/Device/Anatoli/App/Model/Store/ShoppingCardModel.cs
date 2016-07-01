using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class ShoppingCardModel : BaseModel
    {
        public int Qty { get; set; }
        public double TotalPrice { get; set; }
    }
}
