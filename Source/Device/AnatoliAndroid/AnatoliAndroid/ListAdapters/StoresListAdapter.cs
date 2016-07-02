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
    class StoresListAdapter : BaseListAdapter<StoreManager, StoreModel>
    {
        public override View GetItemView(int position, View convertView, ViewGroup parent)
        {
            convertView = _context.LayoutInflater.Inflate(Resource.Layout.StoreSummaryLayout, null);

            StoreModel item = null;
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

            storeNameTextView.Text = item.storeName;
            storeAddressTextView.Text = item.address;

            storeStatusTextView.Click += (s, e) =>
            {
                Select(item);
            };
            storeImageView.Click += (s, e) =>
            {
                Select(item);
            };
            storeSummaryInfoLinearLayout.Click += (s, e) =>
            {
                Select(item);
            };
            if (!item.supportAppOrder)
                convertView.FindViewById<ImageView>(Resource.Id.onlineStamp).Visibility = ViewStates.Invisible;
            if (item.distance < 0.0005)
                _storeDistance.Text = "";
            else if (item.distance < 1500)
                _storeDistance.Text = _context.Resources.GetText(Resource.String.DistanceFromYou) + " " + Math.Round(item.distance, 0).ToString() + " " + AnatoliApp.GetResources().GetText(Resource.String.Meter);
            else
                _storeDistance.Text = _context.Resources.GetText(Resource.String.DistanceFromYou) + " " + Math.Round((item.distance / 1000), 1).ToString() + " " + AnatoliApp.GetResources().GetText(Resource.String.KMeter);

            if (!String.IsNullOrEmpty(item.location))
            {
                _mapIconImageView.Click += (s, e) =>
                {
                    OpenMap(item.location, item.address);
                };
            }

            // todo : add store close open 

            storeStatusTextView.Text = _context.Resources.GetText(Resource.String.Open);
            storeStatusTextView.SetTextColor(_context.Resources.GetColor(Resource.Color.green));

            //storeStatusTextView.Text = AnatoliApp.GetResources().GetText(Resource.String.Close);
            //storeStatusTextView.SetTextColor(Android.Graphics.Color.Red);


            // productIimageView.SetUrlDrawable(MadanerClient.Configuration.UsersImageBaseUri + "/" + item.User.image, null, 600000);
            return convertView;
        }
        void Select(StoreModel item)
        {
            if (!item.supportAppOrder)
            {
                Toast.MakeText(AnatoliApp.GetInstance().Activity, "این فروشگاه فروش اینترنتی ندارد", ToastLength.Short).Show();
                return;
            }
            if (ShoppingCardManager.GetInfo().Qty > 0)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                alert.SetTitle(Resource.String.Error);
                alert.SetMessage("پس از تغییر فروشگاه سبد خرید شما خالی میشود، ادامه می دهید؟");
                alert.SetPositiveButton(Resource.String.Ok, delegate
                {
                    if (StoreManager.Select(item) == true)
                    {
                        AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "first_fragment");
                        OnStoreSelected(item);
                    }
                });
                alert.SetNegativeButton(Resource.String.Cancel, delegate { });
                alert.Show();
                return;
            }
            else if (StoreManager.Select(item) == true)
            {
                AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "first_fragment");
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
        void OnStoreSelected(StoreModel store)
        {
            if (StoreSelected != null)
            {
                StoreSelected.Invoke(store);
            }
        }
        public event StoreSelectedHandler StoreSelected;
        public delegate void StoreSelectedHandler(StoreModel item);
    }
}