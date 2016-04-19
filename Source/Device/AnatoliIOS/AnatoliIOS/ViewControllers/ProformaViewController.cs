﻿using System;

using UIKit;
using CoreGraphics;
using Anatoli.App.Model.Store;
using Anatoli.Framework.AnatoliBase;
using AnatoliIOS.TableViewCells;
using Foundation;
using ObjCRuntime;
using System.Collections.Generic;
using Anatoli.App.Manager;

namespace AnatoliIOS.ViewControllers
{
    public partial class ProformaViewController : BaseController
    {
        PurchaseOrderViewModel _order;
        DeliveryTimeModel _deliveryTime;
        DeliveryTypeModel _deliveryType;
        public ProformaViewController()
            : base("ProformaViewController", null)
        {
        }

        public ProformaViewController(PurchaseOrderViewModel order, DeliveryTimeModel deliveryTime, DeliveryTypeModel deliveryType)
            : this()
        {
            _order = order;
            _deliveryTime = deliveryTime;
            _deliveryType = deliveryType;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            Title = "پیش فاکتور";
            var headerView = ProformHeader.Create();
            header.AddSubview(headerView);

            var footerView = FooterView.Create();
            footer.AddSubview(footerView);
            cancelButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                DismissViewController(true, null);
            };
            orderDate.Text = _order.OrderDate.ToString();
            orderSum.Text = _order.NetAmount.ToCurrency() + " تومان";
            orderAddress.Text = AnatoliApp.GetInstance().Customer.MainStreet;
            footerView.Price = _order.Amount.ToCurrency();
            footerView.Discount = _order.DiscountAmount.ToCurrency();
            decimal count = 0;
            foreach (var item in _order.LineItems)
            {
                count += item.Qty;
            }
            footerView.Count = count.ToString("N0");
            footerView.Tax = (_order.ChargeAmount + _order.TaxAmount).ToCurrency();
            table.Source = new OrderItemsTableViewSource(_order.LineItems);
            okButton.TouchUpInside += async delegate
            {
                try
                {
                    var result = await ShoppingCardManager.Checkout(AnatoliApp.GetInstance().Customer,
                    AnatoliApp.GetInstance().User.Id,
                    AnatoliApp.GetInstance().DefaultStore.store_id,
                    _deliveryType.id,
                    _deliveryTime);
                    if (result != null && result.IsValid)
                    {
                        var alert = UIAlertController.Create("", "سفارش شما ثبت شد", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                        {
                            AnatoliApp.GetInstance().PushViewController(new FirstPageViewController());
                        }));
                        PresentViewController(alert, true, null);
                    }
                    else
                    {
                        var alert = UIAlertController.Create("خطا", "خطا در ارسال سفارش", UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                        {
                            DismissViewController(true, null);
                        }));
                        PresentViewController(alert, true, null);
                    }
                }
                catch (ServerUnreachableException)
                {
                    var connectionalert = UIAlertController.Create("خطا", "خطا در برقرای ارتباط", UIAlertControllerStyle.Alert);
                    connectionalert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(connectionalert, true, null);
                }
                catch (NoInternetAccessException)
                {
                    var connectionalert = UIAlertController.Create("خطا", "لطفا دستگاه خود را به اینترنت متصل نمایید", UIAlertControllerStyle.Alert);
                    connectionalert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(connectionalert, true, null);
                }
                catch (AnatoliWebClientException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var alert = UIAlertController.Create("خطا", ex.MetaInfo.ModelStateString, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                        PresentViewController(alert, true, null);
                    }
                }
                catch (Exception)
                {
                    var alert = UIAlertController.Create("خطا", "خطا در ارسال سفارش", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, delegate
                    {
                        DismissViewController(true, null);
                    }));
                    PresentViewController(alert, true, null);
                }
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
    class OrderItemsTableViewSource : UITableViewSource
    {
        List<PurchaseOrderLineItemViewModel> _items;
        public OrderItemsTableViewSource(List<PurchaseOrderLineItemViewModel> items)
        {
            _items = items;
        }
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _items.Count;
        }
        public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
        {
            OrderItemTableViewCell cell;
            cell = tableView.DequeueReusableCell(OrderItemTableViewCell.Key) as OrderItemTableViewCell;
            if (cell == null)
            {
                var views = NSBundle.MainBundle.LoadNib(OrderItemTableViewCell.Key, tableView, null);
                cell = Runtime.GetNSObject(views.ValueAt(0)) as OrderItemTableViewCell;
            }
            if (_items != null)
            {
                if (_items.Count > indexPath.Row)
                {
                    cell.Update(_items[indexPath.Row], tableView, indexPath);
                }
            }
            return cell;
        }
    }
}


