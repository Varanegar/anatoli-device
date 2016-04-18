﻿using System;

using UIKit;
using CoreGraphics;
using Anatoli.App.Model.Store;
using System.Collections.Generic;
using Anatoli.App.Manager;
using Anatoli.App.Model;
using AnatoliIOS.Components;
using CoreAnimation;
using Foundation;

namespace AnatoliIOS.ViewControllers
{
    public partial class ProfileViewController : BaseController
    {
        bool _uploading = false;

        public ProfileViewController()
            : base("ProfileViewController", null)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
            Title = "پروفایل";
            View.Bounds = UIScreen.MainScreen.Bounds;
            if (AnatoliApp.GetInstance().Customer != null)
            {
                nameTextField.Text = AnatoliApp.GetInstance().Customer.FirstName;
                lastNameTextField.Text = AnatoliApp.GetInstance().Customer.LastName;
                emailTextField.Text = AnatoliApp.GetInstance().Customer.Email;
                addressTextField.Text = AnatoliApp.GetInstance().Customer.MainStreet;
                titleLabel.Text = AnatoliApp.GetInstance().Customer.FirstName + " " + AnatoliApp.GetInstance().Customer.LastName;
                numberLabel.Text = AnatoliApp.GetInstance().Customer.Mobile;
                var imageUri = CustomerManager.GetImageAddress(AnatoliApp.GetInstance().Customer.UniqueId);
                using (var url = new NSUrl(imageUri))
                {
                    using (var data = NSData.FromUrl(url))
                    {
                        if (data != null)
                        {
                            try
                            {
                                profileImageView.Image = UIImage.LoadFromData(data);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
                CALayer profileImageViewLayer = profileImageView.Layer;
                profileImageViewLayer.CornerRadius = 30;
                profileImageViewLayer.MasksToBounds = true;
            }

            System.Threading.CancellationTokenSource token = new System.Threading.CancellationTokenSource();

            var imagePicker = new UIImagePickerController();
            imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);
            imagePicker.FinishedPickingMedia += async (object sender, UIImagePickerMediaPickedEventArgs e) =>
            {
                if (e.Info[UIImagePickerController.MediaType].ToString() == "public.image")
                {
                    NSUrl referenceURL = e.Info[new NSString("UIImagePickerControllerReferenceUrl")] as NSUrl;
                    if (referenceURL != null)
                        Console.WriteLine("Url:" + referenceURL.ToString());
                    // get the original image
                    UIImage originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                    if (originalImage != null)
                    {
                        // do something with the image
                        profileImageView.Image = originalImage; // display
                        using (NSData imageData = originalImage.AsPNG())
                        {
                            Byte[] myByteArray = new Byte[imageData.Length];
                            System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                            try
                            {
                                imagePicker.DismissViewController(true, null);
                                _uploading = true;
                                pickImageButton.SetTitle("بی خیال", UIControlState.Normal);
                                await CustomerManager.UploadImageAsync(AnatoliApp.GetInstance().Customer.UniqueId, myByteArray, token);
                                pickImageButton.SetTitle("ویرایش", UIControlState.Normal);
                            }
                            catch (Exception ex)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    pickImageButton.SetTitle("ویرایش", UIControlState.Normal);
                                }
                                else
                                {
                                    pickImageButton.SetTitle("خطا!", UIControlState.Normal);
                                }
                            }
                            finally
                            {
                                _uploading = false;
                            }
                        }
                    }

                }
            };
            imagePicker.Canceled += (object sender, EventArgs e) =>
            {
                imagePicker.DismissViewController(true, null);
            };
            pickImageButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                if (!_uploading)
                {
                    PresentViewController(imagePicker, true, null);
                }
                else
                {
                    token.Cancel();
                    _uploading = false;
                }
            };


            var level1PickerViewController = new RegionChooserViewController(await CityRegionManager.GetFirstLevelAsync());
            var level2PickerViewController = new RegionChooserViewController(await CityRegionManager.GetGroupsAsync(AnatoliApp.GetInstance().Customer.RegionLevel1Id));
            var level3PickerViewController = new RegionChooserViewController(await CityRegionManager.GetGroupsAsync(AnatoliApp.GetInstance().Customer.RegionLevel2Id));
            var level4PickerViewController = new RegionChooserViewController(await CityRegionManager.GetGroupsAsync(AnatoliApp.GetInstance().Customer.RegionLevel3Id));


            level1PickerViewController.SelectByGroupId(AnatoliApp.GetInstance().Customer.RegionLevel1Id);
            level1Picker.SetTitle(level1PickerViewController.SelectedItem.group_name, UIControlState.Normal);
            level2PickerViewController.SelectByGroupId(AnatoliApp.GetInstance().Customer.RegionLevel2Id);
            level2Picker.SetTitle(level2PickerViewController.SelectedItem.group_name, UIControlState.Normal);
            level3PickerViewController.SelectByGroupId(AnatoliApp.GetInstance().Customer.RegionLevel3Id);
            level3Picker.SetTitle(level3PickerViewController.SelectedItem.group_name, UIControlState.Normal);
            level4PickerViewController.SelectByGroupId(AnatoliApp.GetInstance().Customer.RegionLevel4Id);
            level4Picker.SetTitle(level4PickerViewController.SelectedItem.group_name, UIControlState.Normal);

            level1PickerViewController.ItemSelected += async (item) =>
            {
                if (item != null)
                {
                    level1Picker.SetTitle(level1PickerViewController.SelectedItem.group_name, UIControlState.Normal);
                    level2PickerViewController.SetItems(await CityRegionManager.GetGroupsAsync(item.group_id));
                }
                else
                    level1Picker.SetTitle(" - - -", UIControlState.Normal);
                level2Picker.SetTitle(" - - -", UIControlState.Normal);
                level2PickerViewController.Select(0);
                level3PickerViewController.Select(0);
                level4PickerViewController.Select(0);
            };
            level2PickerViewController.ItemSelected += async (item) =>
            {
                if (item != null)
                {
                    level2Picker.SetTitle(level2PickerViewController.SelectedItem.group_name, UIControlState.Normal);
                    level3PickerViewController.SetItems(await CityRegionManager.GetGroupsAsync(item.group_id));
                }
                else
                    level2Picker.SetTitle(" - - -", UIControlState.Normal);
                level3Picker.SetTitle(" - - -", UIControlState.Normal);

                level3PickerViewController.Select(0);
                level4PickerViewController.Select(0);
            };
            level3PickerViewController.ItemSelected += async (item) =>
            {
                if (item != null)
                {
                    level3Picker.SetTitle(level3PickerViewController.SelectedItem.group_name, UIControlState.Normal);
                    level4PickerViewController.SetItems(await CityRegionManager.GetGroupsAsync(item.group_id));
                }
                else
                    level3Picker.SetTitle(" - - -", UIControlState.Normal);
                level4Picker.SetTitle(" - - -", UIControlState.Normal);

                level4PickerViewController.Select(0);
            };
            level4PickerViewController.ItemSelected += async (item) =>
            {
                if (item != null)
                {
                    level4Picker.SetTitle(level4PickerViewController.SelectedItem.group_name, UIControlState.Normal);
                }
                else
                    level4Picker.SetTitle(" - - -", UIControlState.Normal);
            };

            level1Picker.TouchUpInside += delegate
            {
                PresentViewController(level1PickerViewController, true, null);
            };
            level2Picker.TouchUpInside += delegate
            {
                if (level1PickerViewController.SelectedItem == null)
                {
                    return;
                }
                PresentViewController(level2PickerViewController, true, null);
            };
            level3Picker.TouchUpInside += delegate
            {
                if (level2PickerViewController.SelectedItem == null)
                {
                    return;
                }
                PresentViewController(level3PickerViewController, true, null);
            };
            level4Picker.TouchUpInside += delegate
            {
                if (level3PickerViewController.SelectedItem == null)
                {
                    return;
                }
                PresentViewController(level4PickerViewController, true, null);
            };

            logoutButton.TouchUpInside += async (object sender, EventArgs e) =>
            {
                await AnatoliApp.GetInstance().LogOutAsync();
                AnatoliApp.GetInstance().ReplaceViewController(new FirstPageViewController());
            };
            saveButton.TouchUpInside += async (object sender, EventArgs e) =>
            {
                if (String.IsNullOrEmpty(addressTextField.Text) ||
                    String.IsNullOrEmpty(lastNameTextField.Text) ||
                    String.IsNullOrEmpty(nameTextField.Text) ||
                    String.IsNullOrEmpty(emailTextField.Text) ||
                    level1PickerViewController.SelectedItem == null ||
                    level2PickerViewController.SelectedItem == null ||
                    level3PickerViewController.SelectedItem == null ||
                    level4PickerViewController.SelectedItem == null)
                {
                    var alert = UIAlertController.Create("خطا", "لطفا همه اطلاعات را وارد نمایید", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("باشه", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                    return;
                }
                CustomerViewModel customer = new CustomerViewModel();
                customer.Address = addressTextField.Text;
                customer.MainStreet = addressTextField.Text;
                customer.LastName = lastNameTextField.Text;
                customer.FirstName = nameTextField.Text;
                customer.Email = emailTextField.Text;
                customer.RegionLevel1Id = level1PickerViewController.SelectedItem.group_id;
                customer.RegionLevel2Id = level2PickerViewController.SelectedItem.group_id;
                customer.RegionLevel3Id = level3PickerViewController.SelectedItem.group_id;
                customer.RegionLevel4Id = level4PickerViewController.SelectedItem.group_id;
                LoadingOverlay loadingOverlay;
                var bounds = UIScreen.MainScreen.Bounds;
                loadingOverlay = new LoadingOverlay(bounds);
                View.Add(loadingOverlay);
                try
                {
                    var result = await CustomerManager.UploadCustomerAsync(customer);
                    if (result != null)
                    {
                        if (result.IsValid)
                        {
                            try
                            {
                                await CustomerManager.DownloadCustomerAsync(AnatoliApp.GetInstance().User, null);
                                AnatoliApp.GetInstance().Customer = await CustomerManager.ReadCustomerAsync();
                                var alert = UIAlertController.Create("", "اطلاعات شما ذخیره شد", UIAlertControllerStyle.Alert);
                                alert.AddAction(UIAlertAction.Create("خب", UIAlertActionStyle.Default, delegate
                                {
                                    AnatoliApp.GetInstance().PushViewController(new FirstPageViewController());
                                }));
                                PresentViewController(alert, true, null);
                            }
                            catch (Exception)
                            {
                                var alert = UIAlertController.Create("", "خطا در ذخیره اطلاعات", UIAlertControllerStyle.Alert);
                                alert.AddAction(UIAlertAction.Create("خب", UIAlertActionStyle.Default, null));
                                PresentViewController(alert, true, null);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    var alert = UIAlertController.Create("", "درخواست شما با خطا مواجه شد", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("خب", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                loadingOverlay.Hide();

            };

        }


    }



}