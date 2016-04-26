using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Anatoli.App.Model.Store;
using Anatoli.Framework.Model;
using Anatoli.App.Manager;
using AnatoliAndroid.ListAdapters;
using Anatoli.Framework.AnatoliBase;
using Android.Locations;
using AnatoliAndroid.Activities;
using System.Threading.Tasks;

namespace AnatoliAndroid.Fragments
{
    [FragmentTitle("انتخاب فروشگاه")]
    class StoresListFragment : BaseListFragment<StoreManager, StoresListAdapter, StoreDataModel>
    {
        Location _currentLocation;
        public StoresListFragment()
        {
            StringQuery query = new StringQuery("SELECT * FROM stores");
            _dataManager.SetQueries(query, null);
            _listAdapter.StoreSelected += async (store) =>
                {
                    foreach (var item in _listAdapter.List)
                    {
                        if (item != store)
                            item.selected = 0;
                        else
                            item.selected = 1;
                    }
                    AnatoliApp.GetInstance().SetDefaultStore(store);
                    AnatoliApp.GetInstance().RefreshMenuItems();
                    if (await ShoppingCardManager.ClearAsync())
                    {
                        AnatoliApp.GetInstance().ShoppingCardItemCount.Text = "0";
                        AnatoliApp.GetInstance().SetTotalPrice(0);
                    }
                    _listAdapter.NotifyDataSetChanged();
                };
        }

        private async void UpdateDistances()
        {
            if (_currentLocation == null)
                return;
            for (int i = _listView.FirstVisiblePosition; i < _listView.LastVisiblePosition; i++)
            {
                var item = _listView.Adapter.GetItem(i);
                var store = item.Cast<StoreDataModel>();
                if (store != null && !String.IsNullOrEmpty(store.location))
                {
                    Location loc = new Location("destination");
                    double latitude = store.lat;
                    double langitude = store.lng;
                    loc.Latitude = latitude;
                    loc.Longitude = langitude;
                    var dist = CalcDistance(_currentLocation, loc);
                    store.distance = dist;
                    await StoreManager.UpdateDistanceAsync(store.store_id.ToString(), dist);
                    _listAdapter.NotifyDataSetChanged();
                }
            }
        }

        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideSearchIcon();
            AnatoliApp.GetInstance().StartLocationUpdates();
            AnatoliApp.GetInstance().LocationChanged += StoresListFragment_LocationChanged;
            _listView.Scroll += _listView_Scroll;
            UpdateDistances();
        }

        private void _listView_Scroll(object sender, AbsListView.ScrollEventArgs e)
        {
            UpdateDistances();
        }

        void StoresListFragment_LocationChanged(Location location)
        {
            if (_currentLocation == null)
            {
                _currentLocation = location;
                UpdateDistances();
            }
            else
                _currentLocation = location;
        }

        public float CalcDistance(Location location1, Location location2)
        {
            var dist = location1.DistanceTo(location2);
            return dist;
        }

        public override void OnPause()
        {
            base.OnPause();
            AnatoliApp.GetInstance().StopLocationUpdates();
            AnatoliApp.GetInstance().LocationChanged -= StoresListFragment_LocationChanged;
        }
    }
}