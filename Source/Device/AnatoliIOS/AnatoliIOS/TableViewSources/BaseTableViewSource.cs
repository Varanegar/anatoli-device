using System;
using UIKit;
using Anatoli.Framework.Model;
using Anatoli.Framework.Manager;
using System.Collections.Generic;
using Anatoli.App.Manager;
using System.Threading.Tasks;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.TableViewCells;
using Foundation;

namespace AnatoliIOS.TableViewSources
{
    public abstract class BaseTableViewSource<BaseDataManager, DataModel> : UITableViewSource
        where DataModel : BaseModel, new()
        where BaseDataManager : BaseManager<DataModel>, new()
    {
        public int ItemsCount { get { return Items.Count; } }
        DateTime _lastUpdate;
        protected List<DataModel> Items { get; set; }

        protected BaseDataManager DataManager;

        public BaseTableViewSource()
        {
            Items = new List<DataModel>();
            DataManager = new BaseDataManager();
            _lastUpdate = DateTime.Now;
        }

        public void SetDataQuery(DBQuery query)
        {
            DataManager.SetQueries(query, null);
        }

        public async Task RefreshAsync()
        {
            Items = await DataManager.GetNextAsync();
        }

        public async Task GetNextAsync()
        {
            if ((DateTime.Now - _lastUpdate) > TimeSpan.FromSeconds(5))
            {
                _lastUpdate = DateTime.Now;
                var list = await DataManager.GetNextAsync();
                if (list != null)
                {
                    Items.AddRange(list);
                    OnUpdated();
                }
            }

        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Items.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            if (indexPath.Row + 1 == Items.Count)
            {
                GetNextAsync();
            }
            var cell = GetCellView(tableView, indexPath);
            return cell;
        }

        public abstract UITableViewCell GetCellView(UITableView tableView, Foundation.NSIndexPath indexPath);

        void OnUpdated()
        {
            if (Updated != null)
            {
                Updated.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler Updated;

        protected void OnItemRemoved(UITableView tableView, NSIndexPath index)
        {
            if (ItemRemoved != null)
                ItemRemoved.Invoke(tableView, index);
        }

        public event ItemRemovedEventHandler ItemRemoved;

        public delegate void ItemRemovedEventHandler(UITableView tableView, NSIndexPath indexPath);

    }


}

