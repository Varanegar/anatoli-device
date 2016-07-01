using Anatoli.App.Model.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.RequestModel
{
    public class PurchaseOrderRequestModel : BaseRequestModel
    {
        public Guid customerId { get; set; }
        public Guid centerId { get; set; }
        public Guid poId { get; set; }
        public PurchaseOrderViewModel orderEntity { get; set; }
        public bool getAllOrderTypes { get { return false; } }

    }
}
