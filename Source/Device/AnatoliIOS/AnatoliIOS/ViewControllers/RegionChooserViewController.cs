using System;

using UIKit;
using AnatoliIOS.TableViewCells;
using System.Collections.Generic;
using Anatoli.App.Model.Store;
using Foundation;
using ObjCRuntime;

namespace AnatoliIOS.ViewControllers
{
	public partial class RegionChooserViewController : UIViewController
	{
		List<CityRegionModel> _items;
		Dictionary<string,int> ItemsDictionary;
		public CityRegionModel SelectedItem;
		RegionTableViewSource _dataSource;

		public RegionChooserViewController () : base ("RegionChooserViewController", null)
		{
		}

		public RegionChooserViewController (List<CityRegionModel> items) : this ()
		{
            if (items == null)
                items = new List<CityRegionModel>();
			SetItems (items);
			_dataSource = new RegionTableViewSource (_items);
		}

		public void Select (int row)
		{
			SelectedItem = _items [row];
			OnItemSelected (SelectedItem);
		}

		public void SelectByGroupId (string groupId)
		{            
			if (ItemsDictionary.ContainsKey (groupId)) {
				SelectedItem = _items [ItemsDictionary [groupId]];
				OnItemSelected (SelectedItem);
			}
		}

		public void SetItems (List<CityRegionModel> items)
		{
			_items = items;
			_items.Insert (0, null);
			ItemsDictionary = new Dictionary<string, int> ();
			for (int i = 0; i < _items.Count; i++) {
				if (_items [i] != null) {
					ItemsDictionary.Add (_items [i].group_id, i);
				}
			}
		}
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _dataSource = new RegionTableViewSource(_items);
            _dataSource.ItemSelected += (CityRegionModel item) =>
            {
                OnItemSelected(item);
            };
            table.Source = _dataSource;
            Foundation.NSIndexPath indexPath;
            if (SelectedItem != null)
            {
                indexPath = Foundation.NSIndexPath.FromRowSection((nint)ItemsDictionary[SelectedItem.group_id], 0);
            }
            else
                indexPath = Foundation.NSIndexPath.FromRowSection(0, 0);
            table.SelectRow(indexPath, true, UITableViewScrollPosition.None);
            backButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                DismissViewController(true, null);
            };
        }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
			EdgesForExtendedLayout = UIRectEdge.None;
			
		}

		void OnItemSelected (CityRegionModel item)
		{
			if (ItemSelected != null) {
				SelectedItem = item;
				ItemSelected.Invoke (item);
				DismissViewController (true, null);
			}
		}

		public event ItemSelectedEventHandler ItemSelected;

		public delegate void ItemSelectedEventHandler (CityRegionModel item);

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}

	class RegionTableViewSource : UITableViewSource
	{
		List<CityRegionModel> _items;

		public CityRegionModel SelectedItem { get; private set; }

		public RegionTableViewSource (List<CityRegionModel> items)
		{
			_items = items;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (RegionChooserTableViewCell.Key) as RegionChooserTableViewCell;
			if (cell == null) {
				var views = NSBundle.MainBundle.LoadNib (RegionChooserTableViewCell.Key, tableView, null);
				cell = Runtime.GetNSObject (views.ValueAt (0)) as RegionChooserTableViewCell;
			}

			if (_items.Count > indexPath.Row) {
				cell.Update (_items [indexPath.Row]);	
			}
			return cell;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return _items.Count;
		}

		public override void RowSelected (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			if (_items.Count > indexPath.Row) {
				SelectedItem = _items [indexPath.Row];
			}
			if (ItemSelected != null) {
				ItemSelected.Invoke (SelectedItem);
			}
		}

		public event ItemSelectedEventHandler ItemSelected;

		public delegate void ItemSelectedEventHandler (CityRegionModel item);
	}
}


