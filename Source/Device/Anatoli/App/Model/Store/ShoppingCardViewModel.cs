using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class ShoppingCardViewModel : BaseViewModel
    {
        public int items_count { get; set; }
        public double total_price { get; set; }
    }
}
