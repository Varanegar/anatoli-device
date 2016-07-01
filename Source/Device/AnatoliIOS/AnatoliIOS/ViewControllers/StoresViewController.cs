using System;

using UIKit;
using Anatoli.App.Manager;
using AnatoliIOS.TableViewSources;
using AnatoliIOS.TableViewCells;

namespace AnatoliIOS.ViewControllers
{
    public partial class StoresViewController : BaseController
    {
        public StoresViewController()
            : base("StoresViewController", null)
        {
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.SetToolbarItems(AnatoliApp.GetInstance().CreateToolbarItems(), true);
        }
        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
			Title = "انتخاب فروشگاه";
            EdgesForExtendedLayout = UIRectEdge.None;

            storesTableView.RegisterNibForCellReuse(UINib.FromName("StoreSummaryTableViewCell", null), StoreSummaryTableViewCell.Key);
            StoresTableViewSource storesTableViewSource = new StoresTableViewSource();
            storesTableViewSource.SetDataQuery(StoreManager.GetAllQueryString());
            await storesTableViewSource.RefreshAsync();
            storesTableView.Source = storesTableViewSource;
            storesTableView.ReloadData();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}


