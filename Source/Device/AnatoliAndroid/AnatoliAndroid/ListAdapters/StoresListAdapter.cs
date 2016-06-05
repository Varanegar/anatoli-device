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
using Anatoli.App.Model.Product;
using Anatoli.App.Model.Store;
using Anatoli.Framework.Model;
using Anatoli.App.Manager;
using AnatoliAndroid.Fragments;
using AnatoliAndroid.Activities;
using Android.Locations;
using System.Threading.Tasks;

namespace AnatoliAndroid.ListAdapters
{
    class StoresListAdapter : BaseListAdapter<StoreManager, StoreDataModel>
    {
        public override View GetItemView(int position, View convertView, ViewGroup parent)
        {
            convertView = _context.LayoutInflater.Inflate(Resource.Layout.StoreSummaryLayout, null);

            StoreDataModel item = null;
            if (List != null)
                item = List[position];
            else
                return convertView;

            if (item.selected == 1)
            {
                convertView.SetBackgroundResource(Resource.Color.lightgray);
            }
            TextView storeNameTextView = convertView.FindViewById<TextView>(Resource.Id.storeNameTextView);
            TextView storeAddressTextView = convertView.FindViewById<TextView>(Resource.Id.storeAddressTextView);
            TextView storeStatusTextView = convertView.FindViewById<TextView>(Resource.Id.storeStatusTextView);
            TextView _storeDistance = convertView.FindViewById<TextView>(Resource.Id.distanceTextView);
            ImageView _mapIconImageView = convertView.FindViewById<ImageView>(Resource.Id.mapIconImageView);
            LinearLayout storeSummaryInfoLinearLayout = convertView.FindViewById<LinearLayout>(Resource.Id.storeSummaryInfoLinearLayout);
            ImageView storeImageView = convertView.FindViewById<ImageView>(Resource.Id.storeImageImageView);

            storeNameTextView.Text = item.store_name;
            storeAddressTextView.Text = item.store_address;

            storeStatusTextView.Click += async (s, e) =>
            {
                await Select(item);
            };
            storeImageView.Click += async (s, e) =>
            {
                await Select(item);
            };
            storeSummaryInfoLinearLayout.Click += async (s, e) =>
            {
                await Select(item);
            };
            if (!item.support_app_order)
                convertView.FindViewById<ImageView>(Resource.Id.onlineStamp).Visibility = ViewStates.Invisible;
            if (item.distance < 0.0005)
                _storeDistance.Text = "";
            else if (item.distance < 1500)
                _storeDistance.Text = AnatoliApp.GetResources().GetText(Resource.String.DistanceFromYou) + " " + Math.Round(item.distance, 0).ToString() + " " + AnatoliApp.GetResources().GetText(Resource.String.Meter);
            else
                _storeDistance.Text = AnatoliApp.GetResources().GetText(Resource.String.DistanceFromYou) + " " + Math.Round((item.distance / 1000), 1).ToString() + " " + AnatoliApp.GetResources().GetText(Resource.String.KMeter);

            if (!String.IsNullOrEmpty(item.location))
            {
                _mapIconImageView.Click += (s, e) =>
                {
                    OpenMap(item.location, item.store_address);
                };
            }

            // todo : add store close open 

            storeStatusTextView.Text = AnatoliApp.GetResources().GetText(Resource.String.Open);
            storeStatusTextView.SetTextColor(AnatoliApp.GetResources().GetColor(Resource.Color.green));

            //storeStatusTextView.Text = AnatoliApp.GetResources().GetText(Resource.String.Close);
            //storeStatusTextView.SetTextColor(Android.Graphics.Color.Red);


            // productIimageView.SetUrlDrawable(MadanerClient.Configuration.UsersImageBaseUri + "/" + item.User.image, null, 600000);
            return convertView;
        }
        async Task Select(StoreDataModel item)
        {
            if (!item.support_app_order)
            {
                Toast.MakeText(AnatoliApp.GetInstance().Activity, "این فروشگاه فروش اینترنتی ندارد", ToastLength.Short).Show();
                return;
            }
            if ((await ShoppingCardManager.GetInfoAsync()).items_count > 0)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                alert.SetTitle(Resource.String.Error);
                alert.SetMessage("پس از تغییر فروشگاه سبد خرید شما خالی میشود، ادامه می دهید؟");
                alert.SetPositiveButton(Resource.String.Ok, async delegate
                {
                    if (await StoreManager.SelectAsync(item) == true)
                    {
                        AnatoliApp.GetInstance().SetFragment<FirstFragment>(new FirstFragment(), "first_fragment");
                        OnStoreSelected(item);
                    }
                });
                alert.SetNegativeButton(Resource.String.Cancel, delegate { });
                alert.Show();
                return;
            }
            else if (await StoreManager.SelectAsync(item) == true)
            {
                AnatoliApp.GetInstance().SetFragment<FirstFragment>(new FirstFragment(), "first_fragment");
                OnStoreSelected(item);
            }
        }
        void OpenMap(string location, string label)
        {
            try
            {
                string a = "geo:" + location + "?q=" + location + "(" + label + ")";
                Console.WriteLine(a);
                var geoUri = Android.Net.Uri.Parse(a);
                var mapIntent = new Intent(Intent.ActionView, geoUri);
                AnatoliApp.GetInstance().Activity.StartActivity(mapIntent);
            }
            catch (Exception)
            {

            }
        }
        void OnStoreSelected(StoreDataModel store)
        {
            if (StoreSelected != null)
            {
                StoreSelected.Invoke(store);
            }
        }
        public event StoreSelectedHandler StoreSelected;
        public delegate void StoreSelectedHandler(StoreDataModel item);
    }
}