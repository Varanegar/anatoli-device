using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class IncompletePurchaseOrderViewModel : BaseModel
    {
        public Guid CustomerId { get; set; }
        public Guid StoreId { get; set; }
        public Guid CityRegionId { get; set; }
        public Guid DeliveryTypeId { get; set; }
        public string OrderShipAddress { get; set; }
        //تحویل گیرنده
        public string Transferee { get; set; }
        //تلفن تحویل گیرنده
        public string Phone { get; set; }
        public DateTime? DeliveryFromTime { get; set; }
        public DateTime? DeliveryToTime { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public Guid? PaymentTypeId { get; set; }

        public List<IncompletePurchaseOrderLineItemViewModel> LineItems = new List<IncompletePurchaseOrderLineItemViewModel>();
    }
}
