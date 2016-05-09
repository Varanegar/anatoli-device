using System;

using Foundation;
using UIKit;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using System.Threading.Tasks;
using Anatoli.App.Manager;

namespace AnatoliIOS.TableViewCells
{
    public partial class OrderItemTableViewCell : BaseTableViewCell
    {
        public static readonly NSString Key = new NSString("OrderItemTableViewCell");
        public static readonly UINib Nib;

        static OrderItemTableViewCell()
        {
            Nib = UINib.FromName("OrderItemTableViewCell", NSBundle.MainBundle);
        }

        public OrderItemTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public async void Update(PurchaseOrderLineItemViewModel purchaseOrderLineItemViewModel,PurchaseOrderViewModel order, UITableView tableView, NSIndexPath indexPath)
        {
            if (purchaseOrderLineItemViewModel != null)
            {
                itemCountLabel.Text = purchaseOrderLineItemViewModel.Qty.ToString("N0");
                itemPriceLabel.Text = purchaseOrderLineItemViewModel.NetAmount.ToCurrency();
                itemRowLabel.Text = (indexPath.Row + 1).ToString();
                itemNameLabel.Text = (await ProductManager.GetItemAsync(purchaseOrderLineItemViewModel.ProductId.ToString().ToUpper(),order.StoreGuid.ToString().ToUpper())).product_name;
            }
        }
    }
}
