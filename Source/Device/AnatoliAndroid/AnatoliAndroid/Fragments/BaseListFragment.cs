using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Anatoli.Framework.Manager;
using Anatoli.Framework.Model;
using Anatoli.Framework.DataAdapter;
using AnatoliAndroid.ListAdapters;
using Anatoli.Framework.AnatoliBase;
using System.Threading.Tasks;
using AnatoliAndroid.Activities;
using AnatoliAndroid.Components;
using Anatoli.Framework;

namespace AnatoliAndroid.Fragments
{
    abstract class BaseListFragment<BaseDataManager, DataListAdapter, ListTools, DataModel> : Fragment
        where BaseDataManager : BaseManager<DataModel>, new()
        where DataListAdapter : BaseListAdapter<BaseDataManager, DataModel>, new()
        where ListTools : ListToolsDialog, new()
        where DataModel : BaseViewModel, new()
    {
        protected View _view;
        protected ListView _listView;
        TextView _resultTextView;
        protected DataListAdapter _listAdapter;
        protected BaseDataManager _dataManager;
        protected ListTools _toolsDialogFragment;
        private bool _refresh = true;
        public BaseListFragment()
            : base()
        {
            _listAdapter = new DataListAdapter();
            _dataManager = new BaseDataManager();
            _toolsDialogFragment = new ListTools();

        }
        public virtual async Task Search(DBQuery query, string value)
        {
            _dataManager.SetQueries(query, null);
            try
            {
                _listAdapter.List = await _dataManager.GetNextAsync();
                AnatoliApp.GetInstance().SetToolbarTitle(string.Format("جستجو  \"{0}\"", value.Trim()));
            }
            catch (Exception ex)
            {
                ex.SendTrace();
            }
        }
        protected virtual View InflateLayout(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(
                Resource.Layout.ItemsListLayout, container, false);
            return view;
        }
        public async override void OnStart()
        {
            base.OnStart();
            if (_refresh)
            {
                await RefreshAsync();
            }
        }
        public override void OnDetach()
        {
            _refresh = true;
            base.OnDetach();
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = InflateLayout(inflater, container, savedInstanceState);
            _listView = _view.FindViewById<ListView>(Resource.Id.itemsListView);
            _resultTextView = _view.FindViewById<TextView>(Resource.Id.resultTextView);
            _listView.ScrollStateChanged += _listView_ScrollStateChanged;
            _listView.Adapter = _listAdapter;

            if (_toolsDialogFragment.GetType() == typeof(NoListToolsDialog))
            {
                AnatoliApp.GetInstance().HideMenuIcon();
            }
            else
            {
                AnatoliApp.GetInstance().ShowMenuIcon();
                AnatoliApp.GetInstance().MenuClicked = () =>
                {
                    _toolsDialogFragment.Show(AnatoliApp.GetInstance().Activity.FragmentManager, "sss");
                };
            }
            EmptyList += (s, e) =>
            {
                _resultTextView.Visibility = ViewStates.Visible;
                _resultTextView.Text = "هیچ موردی یافت نشد";
            };
            FullList += (s, e) =>
            {
                _resultTextView.Visibility = ViewStates.Gone;
            };
            return _view;
        }
        internal async Task RefreshAsync()
        {
            try
            {
                _refresh = false;
               
                _dataManager.ResetQueryLimits();
                _listAdapter.List = await _dataManager.GetNextAsync();
                if (_listAdapter.Count == 0)
                    OnEmptyList();
                else
                    OnFullList();
                _listView.SetSelection(0);
                _listAdapter.NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                ex.SendTrace();
            }
        }

        async void _listView_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            if (e.ScrollState == ScrollState.Idle)
            {
                if ((_listView.Adapter.Count - 1) <= _listView.LastVisiblePosition)
                {
                    try
                    {
                        var list = await _dataManager.GetNextAsync();
                        if (list.Count > 0)
                        {
                            _listAdapter.List.AddRange(list);
                            _listAdapter.NotifyDataSetChanged();
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.SendTrace();
                    }
                }
            }
        }

        public void HideTools()
        {
            _toolsDialogFragment.Dismiss();
        }
        public void ShowTools()
        {
            var transaction = FragmentManager.BeginTransaction();
            _toolsDialogFragment.Show(transaction, "tools_dialog");
        }
        protected void OnEmptyList()
        {
            if (EmptyList != null)
            {
                EmptyList.Invoke(this, new EventArgs());
            }
        }
        public event EventHandler EmptyList;

        protected void OnFullList()
        {
            if (FullList != null)
            {
                FullList.Invoke(this, new EventArgs());
            }
        }
        public event EventHandler FullList;
    }
}