using System;
using Anatoli.App.Manager;
using Anatoli.App.Model.Store;
using AnatoliIOS.TableViewCells;
using AnatoliIOS.ViewControllers;
using UIKit;

namespace AnatoliIOS.TableViewSources
{
    public class StoresTableViewSource : BaseTableViewSource<StoreManager, StoreDataModel>
    {
        public StoresTableViewSource()
        {
        }
        public override UIKit.UITableViewCell GetCellView(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(StoreSummaryTableViewCell.Key) as StoreSummaryTableViewCell;
            cell.UpdateCell(Items[indexPath.Row]);
            return cell;
        }

        public async override void RowSelected(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            var item = Items[indexPath.Row];
            if (item != null)
            {
                if (!item.support_app_order)
                {
                    var alert = new UIAlertView("", "این فروشگاه امکان فروش اینترنتی را ندارد", null, "باشه");
                    alert.Show();
                    return;
                }
                var result = await StoreManager.SelectAsync(item);
                if (result)
                {
                    AnatoliApp.GetInstance().DefaultStore = item;
                    AnatoliApp.GetInstance().ReplaceViewController(new FirstPageViewController());
                }
            }

        }
        public override nfloat GetHeightForRow(UIKit.UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            return 70f;
        }
    }
}

