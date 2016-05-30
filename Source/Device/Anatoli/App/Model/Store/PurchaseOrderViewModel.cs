using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class PurchaseOrderViewModel : BaseViewModel
    {
        public Guid UserId { get; set; }
        public Guid StoreGuid { get; set; }
        public Guid PurchaseOrderStatusValueId { get; set; }
        public Guid PaymentTypeValueId { get; set; }
        public DateTime? OrderDate { get; set; }
        public TimeSpan? OrderTime { get; set; }
        public string OrderPDate { get; set; }
        public string DeviceIMEI { get; set; }
        public string ActionSourceValueId { get; set; }
        public String ShipAddress { get; set; }

        public String Comment { get; set; }
        public long AppOrderNo { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountAmount2 { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal FinalDiscountAmount { get; set; }
        public decimal FinalTaxAmount { get; set; }
        public decimal FinalChargeAmount { get; set; }
        public decimal FinalDiscountAmount2 { get; set; }
        public decimal FinalPayableAmount { get; set; }
        public decimal FinalFreightAmount { get; set; }
        public Guid DeliveryTypeId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryPDate { get; set; }
        public TimeSpan? DeliveryFromTime { get; set; }
        public TimeSpan? DeliveryToTime { get; set; }
        public bool IsCancelled { get; set; }
        public string CancelReason { get; set; }
        public decimal NetAmount { get; set; }
        public decimal FinalNetAmount { get; set; }
        public CustomerViewModel Customer { get; set; }
        public int? BackOfficeId { get; set; }

        public List<PurchaseOrderLineItemViewModel> LineItems = new List<PurchaseOrderLineItemViewModel>();
    }
}
