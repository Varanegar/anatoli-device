using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class IncompletePurchaseOrderLineItemViewModel : BaseModel
    {
        public decimal Qty { get; set; }
        public Guid ProductId { get; set; }
        public Guid IncompletePurchaseOrderId { get; set; }
    }
}
