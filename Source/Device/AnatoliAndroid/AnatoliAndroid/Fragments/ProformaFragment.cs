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
using Anatoli.Framework.AnatoliBase;
using AnatoliAndroid.Activities;
using Anatoli.App.Manager;
using Anatoli.App.Model.Product;
using Java.Lang;
using Anatoli.App.Model;
using System.Globalization;

namespace AnatoliAndroid.Fragments
{
    public class ProformaFragment : DialogFragment
    {
        PurchaseOrderViewModel _orderViewModel;
        CustomerViewModel _customerViewModel;
        ListView _itemsListView;
        public ProformaFragment(PurchaseOrderViewModel orderViewModel, CustomerViewModel customerViewModel)
        {
            _orderViewModel = orderViewModel;
            _customerViewModel = customerViewModel;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.ProformaLayout, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            Dialog.SetCanceledOnTouchOutside(false);

            view.FindViewById<TextView>(Resource.Id.deliveryAddressTextView).Text = _customerViewModel.MainStreet;
            var pc = new PersianCalendar();
            view.FindViewById<TextView>(Resource.Id.orderDateTextView).Text = "تاریخ : " + pc.GetYear(_orderViewModel.OrderDate.Value).ToString() + "/" + pc.GetMonth(_orderViewModel.OrderDate.Value).ToString() + "/" + pc.GetDayOfMonth(_orderViewModel.OrderDate.Value).ToString();
            view.FindViewById<TextView>(Resource.Id.orderPriceTextView).Text = "مبلغ قابل پرداخت : " + _orderViewModel.NetAmount.ToCurrency();
            view.FindViewById<TextView>(Resource.Id.totalPriceTextView).Text = _orderViewModel.Amount.ToCurrency();
            decimal tCount = 0;
            foreach (var item in _orderViewModel.LineItems)
            {
                tCount += item.Qty;
            }
            view.FindViewById<TextView>(Resource.Id.totalCountTextView).Text = tCount.ToString("N0");
            view.FindViewById<TextView>(Resource.Id.totalDiscountTextView).Text = _orderViewModel.DiscountAmount.ToCurrency();
            view.FindViewById<TextView>(Resource.Id.taxTextView).Text = (_orderViewModel.ChargeAmount + _orderViewModel.TaxAmount).ToCurrency();
            var button = view.FindViewById<Button>(Resource.Id.okButton);
            button.UpdateWidth();
            button.Click += (s, e) =>
            {
                OnProformaAccepted();
            };

            _itemsListView = view.FindViewById<ListView>(Resource.Id.itemsListView);

            return view;
        }
        public async override void OnStart()
        {
            base.OnStart();

            List<ProductModel> products = new List<ProductModel>();
            ProgressDialog p = new ProgressDialog(Activity);
            p.SetMessage(Resources.GetString(Resource.String.PleaseWait));
            p.Show();
            try
            {
                foreach (var item in _orderViewModel.LineItems)
                {
                    products.Add(ProductManager.GetItem(item.ProductId, _orderViewModel.StoreGuid));
                }
                _itemsListView.Adapter = new ProformaListAdapter(AnatoliApp.GetInstance().Activity, _orderViewModel.LineItems, products);
            }
            catch (System.Exception)
            {
                p.Dismiss();
                var alert = new AlertDialog.Builder(Activity);
                alert.SetMessage(Resource.String.ErrorOccured);
                alert.SetTitle(Resource.String.Error);
                alert.SetPositiveButton(Resource.String.Ok, delegate { });
                alert.Show();
            }
            finally
            {
                p.Dismiss();
            }

        }
        public class ProformaListAdapter : BaseAdapter<PurchaseOrderLineItemViewModel>
        {
            List<PurchaseOrderLineItemViewModel> _list;
            List<ProductModel> _products;
            Activity _context;
            public ProformaListAdapter(Activity context, List<PurchaseOrderLineItemViewModel> list, List<ProductModel> products)
            {
                _list = list;
                _context = context;
                _products = products;
            }
            public override int Count
            {
                get { return _list.Count; }
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var view = _context.LayoutInflater.Inflate(Resource.Layout.SimpleOrderItemLayout, null);
                var item = _list[position];

                view.FindViewById<TextView>(Resource.Id.itemCountTextView).Text = item.Qty.ToString("N0");
                view.FindViewById<TextView>(Resource.Id.itemPriceTextView).Text = item.NetAmount.ToCurrency();
                view.FindViewById<TextView>(Resource.Id.rowTextView).Text = (position + 1).ToString();
                if (_products[position] != null)
                {
                    view.FindViewById<TextView>(Resource.Id.itemNameTextView).Text = _products[position].StoreProductName;
                }
                return view;
            }

            public override PurchaseOrderLineItemViewModel this[int position]
            {
                get { return (_list[position] != null) ? _list[position] : null; }
            }
        }

        void OnProformaAccepted()
        {
            if (ProformaAccepted != null)
            {
                ProformaAccepted.Invoke(this, new EventArgs());
            }
        }
        public event EventHandler ProformaAccepted;
    }
}