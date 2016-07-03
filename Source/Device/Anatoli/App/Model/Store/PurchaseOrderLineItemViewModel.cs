using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class PurchaseOrderLineItemViewModel : BaseModel
    {
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public Guid? FinalProductId { get; set; }
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal Qty { get; set; }
        public bool IsPrize { get; set; }
        public string Comment { get; set; }
        public bool AllowReplace { get; set; }
        public decimal FinalUnitPrice { get; set; }
        public decimal FinalDiscountAmount { get; set; }
        public decimal FinalTaxAmount { get; set; }
        public decimal FinalChargeAmount { get; set; }
        public decimal FinalNetAmount { get; set; }
        public decimal FinalQty { get; set; }
        public bool FinalIsPrize { get; set; }
        public Guid PurchaseOrderId { get; set; }
    }
}
