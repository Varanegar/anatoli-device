using System;

using UIKit;
using AnatoliIOS.TableViewSources;
using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model.Product;
using System.Threading.Tasks;
using System.Collections.Generic;
using Anatoli.App.Model.Store;
using AnatoliIOS.Components;

namespace AnatoliIOS.ViewControllers
{
    public partial class ShoppingCardViewController : BaseController
    {
        ShoppingCardTableViewSource _productTableViewSource;
        public ShoppingCardViewController()
            : base("ShoppingCardViewController", null)
        {
        }
        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _productTableViewSource = new ShoppingCardTableViewSource();
            _productTableViewSource.SetDataQuery(ShoppingCardManager.GetAll(AnatoliApp.GetInstance().DefaultStore.store_id));
            await _productTableViewSource.RefreshAsync();
            if (_productTableViewSource.ItemsCount > 0)
                tableEmptyLabel.Hidden = true;
            else
                tableEmptyLabel.Hidden = false;
            productsTableView.Source = _productTableViewSource;
            productsTableView.ReloadData();
            if (AnatoliApp.GetInstance().DefaultStore != null)
            {
                if (AnatoliApp.GetInstance().DefaultStore.store_name != null)
                    storeNameLabel.Text = AnatoliApp.GetInstance().DefaultStore.store_name;
                if (!String.IsNullOrEmpty(AnatoliApp.GetInstance().DefaultStore.store_tel))
                    storeTelLabel.Text = AnatoliApp.GetInstance().DefaultStore.store_tel;
                else
                    storeTelLabel.Text = "نامشخص";
            }
            if (AnatoliApp.GetInstance().Customer != null)
            {
                if (AnatoliApp.GetInstance().Customer.MainStreet != null)
                {
                    addressLabel.Text = AnatoliApp.GetInstance().Customer.MainStreet;
                }
            }
            editAddressButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                AnatoliApp.GetInstance().PushViewController(new ProfileViewController());
            };

            var deliveryTypeModel = new DeliveryTypePickerViewModel(await DeliveryTypeManager.GetDeliveryTypesAsync());
            deliveryTypePicker.Model = deliveryTypeModel;
            deliveryTypePicker.ReloadComponent(0);
            deliveryTypeModel.ItemSelected += async (item) =>
            {
                if (deliveryTypeModel.SelectedItem != null)
                {
                    var model = new TimePickerViewModel(await DeliveryTimeManager.GetAvailableDeliveryTimes(AnatoliApp.GetInstance().DefaultStore.store_id, DateTime.Now, deliveryTypeModel.SelectedItem.id));
                    timePicker.Model = model;
                    timePicker.Select(0, 0, true);
                    model.Selected(timePicker, 0, 0);
                }
            };
            deliveryTypePicker.Select(0, 0, true);
            deliveryTypeModel.Selected(deliveryTypePicker, 0, 0);

            itemCountLabel.Text = await ShoppingCardManager.GetItemsCountAsync() + " عدد";
            totalPriceLabel.Text = (await ShoppingCardManager.GetTotalPriceAsync()).ToCurrency() + " تومان";
            ShoppingCardManager.ItemChanged += UpdateLabels;
            checkoutButton.TouchUpInside += async (object sender, EventArgs e) =>
            {
                CalcPromo();
            };
        }
        async void CalcPromo()
        {
            try
            {
                await ShoppingCardManager.ValidateRequest(AnatoliApp.GetInstance().Customer);
                if ((deliveryTypePicker.Model as DeliveryTypePickerViewModel).SelectedItem == null)
                {
                    var alert = UIAlertController.Create("خطا", "نحوه تحویا سفارش را انتخاب نمایید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                    return;
                }
                if ((timePicker.Model as TimePickerViewModel).SelectedItem == null)
                {
                    var alert = UIAlertController.Create("خذا", "زمان تحویل کالا را انتخاب نمایید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                    return;
                }
                LoadingOverlay loading = new LoadingOverlay(View.Bounds);
                View.AddSubview(loading);
                try
                {
                    var order = await ShoppingCardManager.CalcPromo(AnatoliApp.GetInstance().Customer,
                    AnatoliApp.GetInstance().User.Id,
                    AnatoliApp.GetInstance().DefaultStore.store_id,
                    (deliveryTypePicker.Model as DeliveryTypePickerViewModel).SelectedItem.id,
                    (timePicker.Model as TimePickerViewModel).SelectedItem);
                    if (order != null)
                    {
                        if (order.IsValid)
                        {
                            AnatoliApp.GetInstance().PresentViewController(new ProformaViewController(order));
                        }
                    }
                }
                catch (Exception)
                {
                    var alert = UIAlertController.Create("خطا", "درخواست شما با خطا روبرو شد", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("بی خیال", UIAlertActionStyle.Cancel, null));
                    alert.AddAction(UIAlertAction.Create("دوباره تلاش کن", UIAlertActionStyle.Default, delegate { CalcPromo(); }));
                    PresentViewController(alert, true, null);
                }
                finally
                {
                    loading.Hidden = true;
                }
            }
            catch (ValidationException ex)
            {
                if (ex.Code == ValidationErrorCode.CustomerInfo)
                {
                    var alert = UIAlertController.Create("خطا", "اطلاعات خود را کامل نمایید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("بیخیال", UIAlertActionStyle.Cancel, null));
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                    {
                        AnatoliApp.GetInstance().PushViewController(new ProfileViewController());
                    }));
                    PresentViewController(alert, true, null);
                }
                else if (ex.Code == ValidationErrorCode.NoLogin)
                {
                    var alert = UIAlertController.Create("خطا", "لطفا وارد حساب کاربری خود شوید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("بیخیال", UIAlertActionStyle.Cancel, null));
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                    {
                        AnatoliApp.GetInstance().PushViewController(new LoginViewController());
                    }));
                }
            }
        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            Title = "سبد خرید";

        }

        public async void UpdateLabels(ProductModel item)
        {
            var count = await ShoppingCardManager.GetItemsCountAsync();
            itemCountLabel.Text = count + " عدد";
            totalPriceLabel.Text = (await ShoppingCardManager.GetTotalPriceAsync()).ToCurrency() + " تومان";
            if (count == 0)
            {
                productsTableView.ReloadData();
                tableEmptyLabel.Hidden = false;
            }
        }
        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            ShoppingCardManager.ItemChanged -= UpdateLabels;
        }
        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.

        }
    }

    class TimePickerViewModel : UIPickerViewModel
    {
        List<DeliveryTimeModel> _items;
        public DeliveryTimeModel SelectedItem;
        public TimePickerViewModel(List<DeliveryTimeModel> items)
        {
            _items = items;
        }
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return _items[(int)row].ToString();
        }
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            if (_items.Count > (int)row)
            {
                SelectedItem = _items[(int)row];
            }
        }
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _items.Count;
        }
        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            var label = new UILabel();
            label.BackgroundColor = UIColor.Clear;
            label.TextAlignment = UITextAlignment.Center;
            label.Font = UIFont.FromName("IRAN", 10);
            label.TextColor = UIColor.Black;
            if (_items != null)
            {
                if (_items[(int)row] != null)
                {
                    label.Text = _items[(int)row].ToString();
                }
            }
            return label;
        }
    }
    class DeliveryTypePickerViewModel : UIPickerViewModel
    {
        List<DeliveryTypeModel> _items;
        public DeliveryTypeModel SelectedItem;
        public DeliveryTypePickerViewModel(List<DeliveryTypeModel> items)
        {
            _items = items;
        }
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return _items[(int)row].ToString();
        }
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            if (_items.Count > (int)row)
            {
                SelectedItem = _items[(int)row];
                if (ItemSelected != null)
                {
                    ItemSelected.Invoke(SelectedItem);
                }
            }
        }
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _items.Count;
        }
        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            var label = new UILabel();
            label.BackgroundColor = UIColor.Clear;
            label.TextAlignment = UITextAlignment.Center;
            label.Font = UIFont.FromName("IRAN", 10);
            label.TextColor = UIColor.Black;
            if (_items != null)
            {
                if (_items[(int)row] != null)
                {
                    label.Text = _items[(int)row].ToString();
                }
            }
            return label;
        }
        public event ItemSelectedEventHandler ItemSelected;
        public delegate void ItemSelectedEventHandler(DeliveryTypeModel item);
    }
}

