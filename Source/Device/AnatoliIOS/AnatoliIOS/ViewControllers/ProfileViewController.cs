using System;

using UIKit;
using CoreGraphics;
using Anatoli.App.Model.Store;
using System.Collections.Generic;
using Anatoli.App.Manager;
using Anatoli.App.Model;
using AnatoliIOS.Components;
using CoreAnimation;
using Foundation;
using Anatoli.Framework.AnatoliBase;
using System.Drawing;
using SDWebImage;
using System.Threading.Tasks;

namespace AnatoliIOS.ViewControllers
{
	public partial class ProfileViewController : BaseController
	{
		bool _uploading = false;

		public ProfileViewController ()
			: base ("ProfileViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		public async override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			this.SetToolbarItems (AnatoliApp.GetInstance ().CreateToolbarItems (), true);

			var level1PickerViewController = new RegionChooserViewController (await CityRegionManager.GetFirstLevelAsync ());
			var level2PickerViewController = new RegionChooserViewController (await CityRegionManager.GetGroupsAsync (AnatoliApp.GetInstance ().Customer.RegionLevel1Id));
			var level3PickerViewController = new RegionChooserViewController (await CityRegionManager.GetGroupsAsync (AnatoliApp.GetInstance ().Customer.RegionLevel2Id));
			var level4PickerViewController = new RegionChooserViewController (await CityRegionManager.GetGroupsAsync (AnatoliApp.GetInstance ().Customer.RegionLevel3Id));


			level1PickerViewController.SelectByGroupId (AnatoliApp.GetInstance ().Customer.RegionLevel1Id);
			UpdatePickerTitle (level1Picker, level1PickerViewController);
			level2PickerViewController.SelectByGroupId (AnatoliApp.GetInstance ().Customer.RegionLevel2Id);
			UpdatePickerTitle (level2Picker, level2PickerViewController);
			level3PickerViewController.SelectByGroupId (AnatoliApp.GetInstance ().Customer.RegionLevel3Id);
			UpdatePickerTitle (level3Picker, level3PickerViewController);
			level4PickerViewController.SelectByGroupId (AnatoliApp.GetInstance ().Customer.RegionLevel4Id);
			UpdatePickerTitle (level4Picker, level4PickerViewController);

			level1PickerViewController.ItemSelected += async (item) => {
				if (item != null)
					level2PickerViewController.SetItems (await CityRegionManager.GetGroupsAsync (item.group_id));
				UpdatePickerTitle (level1Picker, level1PickerViewController);
				level2PickerViewController.Select (0);
				level3PickerViewController.Select (0);
				level4PickerViewController.Select (0);
			};
			level2PickerViewController.ItemSelected += async (item) => {
				if (item != null)
					level3PickerViewController.SetItems (await CityRegionManager.GetGroupsAsync (item.group_id));
				UpdatePickerTitle (level2Picker, level2PickerViewController);
				level3PickerViewController.Select (0);
				level4PickerViewController.Select (0);
			};
			level3PickerViewController.ItemSelected += async (item) => {
				if (item != null)
					level4PickerViewController.SetItems (await CityRegionManager.GetGroupsAsync (item.group_id));
				UpdatePickerTitle (level3Picker, level3PickerViewController);
				level4PickerViewController.Select (0);
			};
			level4PickerViewController.ItemSelected += (item) => {
				UpdatePickerTitle (level4Picker, level4PickerViewController);
			};

			level1Picker.TouchUpInside += delegate {
				PresentViewController (level1PickerViewController, true, null);
			};
			level2Picker.TouchUpInside += delegate {
				if (level1PickerViewController.SelectedItem == null) {
					return;
				}
				PresentViewController (level2PickerViewController, true, null);
			};
			level3Picker.TouchUpInside += delegate {
				if (level2PickerViewController.SelectedItem == null) {
					return;
				}
				PresentViewController (level3PickerViewController, true, null);
			};
			level4Picker.TouchUpInside += delegate {
				if (level3PickerViewController.SelectedItem == null) {
					return;
				}
				PresentViewController (level4PickerViewController, true, null);
			};

			saveButton.TouchUpInside += async (object sender, EventArgs e) => {
				if (String.IsNullOrEmpty (addressTextField.Text) ||
				                String.IsNullOrEmpty (lastNameTextField.Text) ||
				                String.IsNullOrEmpty (nameTextField.Text) ||
				                String.IsNullOrEmpty (emailTextField.Text) ||
				                String.IsNullOrEmpty (nationalCodeTextField.Text) ||
				                level1PickerViewController.SelectedItem == null ||
				                level2PickerViewController.SelectedItem == null ||
				                level3PickerViewController.SelectedItem == null ||
				                level4PickerViewController.SelectedItem == null) {
					var alert = UIAlertController.Create ("خطا", "لطفا همه اطلاعات را وارد نمایید", UIAlertControllerStyle.Alert);
					alert.AddAction (UIAlertAction.Create ("باشه", UIAlertActionStyle.Default, null));
					PresentViewController (alert, true, null);
					return;
				}
				var customer = AnatoliApp.GetInstance ().Customer;
				customer.Address = addressTextField.Text;
				customer.MainStreet = addressTextField.Text;
				customer.LastName = lastNameTextField.Text;
				customer.FirstName = nameTextField.Text;
				customer.Email = emailTextField.Text;
				customer.Mobile = numberLabel.Text;
				customer.NationalCode = nationalCodeTextField.Text;
				customer.RegionLevel1Id = level1PickerViewController.SelectedItem.group_id;
				customer.RegionLevel2Id = level2PickerViewController.SelectedItem.group_id;
				customer.RegionLevel3Id = level3PickerViewController.SelectedItem.group_id;
				customer.RegionLevel4Id = level4PickerViewController.SelectedItem.group_id;
				LoadingOverlay loadingOverlay;
				var bounds = UIScreen.MainScreen.Bounds;
				loadingOverlay = new LoadingOverlay (bounds);
				View.Add (loadingOverlay);
				try {
					var result = await CustomerManager.UploadCustomerAsync (customer);
					if (result != null) {
						if (result.IsValid) {
							try {
								var downloadedcustomer = await CustomerManager.DownloadCustomerAsync (AnatoliApp.GetInstance ().User, null);
								await CustomerManager.SaveCustomerAsync (downloadedcustomer);
								AnatoliApp.GetInstance ().Customer = downloadedcustomer;
								var alert = UIAlertController.Create ("", "اطلاعات شما ذخیره شد", UIAlertControllerStyle.Alert);
								alert.AddAction (UIAlertAction.Create ("خب", UIAlertActionStyle.Default, delegate {
									AnatoliApp.GetInstance ().PushViewController (new FirstPageViewController ());
								}));
								PresentViewController (alert, true, null);
							} catch (Exception) {
								var alert = UIAlertController.Create ("", "خطا در ذخیره اطلاعات", UIAlertControllerStyle.Alert);
								alert.AddAction (UIAlertAction.Create ("خب", UIAlertActionStyle.Default, null));
								PresentViewController (alert, true, null);
							}
						}
					}
				} catch (ServerUnreachableException) {
					var connectionalert = UIAlertController.Create ("خطا", "خطا در برقرای ارتباط", UIAlertControllerStyle.Alert);
					connectionalert.AddAction (UIAlertAction.Create ("باشه", UIAlertActionStyle.Default, null));
					PresentViewController (connectionalert, true, null);
				} catch (NoInternetAccessException) {
					var connectionalert = UIAlertController.Create ("خطا", "لطفا دستگاه خود را به اینترنت متصل نمایید", UIAlertControllerStyle.Alert);
					connectionalert.AddAction (UIAlertAction.Create ("باشه", UIAlertActionStyle.Default, null));
					PresentViewController (connectionalert, true, null);
				} catch (AnatoliWebClientException ex) {
					if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest) {
						var alert = UIAlertController.Create ("خطا", ex.MetaInfo.ModelStateString, UIAlertControllerStyle.Alert);
						alert.AddAction (UIAlertAction.Create ("باشه", UIAlertActionStyle.Default, null));
						PresentViewController (alert, true, null);
					}
				} catch (Exception) {
					var alert = UIAlertController.Create ("", "درخواست شما با خطا مواجه شد", UIAlertControllerStyle.Alert);
					alert.AddAction (UIAlertAction.Create ("خب", UIAlertActionStyle.Default, null));
					PresentViewController (alert, true, null);
				}
				loadingOverlay.Hide ();

			};

		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Perform any additional setup after loading the view, typically from a nib.
			Title = "پروفایل";

			if (AnatoliApp.GetInstance ().Customer != null) {
				nameTextField.ShouldReturn += delegate {
					nameTextField.ResignFirstResponder ();
					return true;
				};
				lastNameTextField.ShouldReturn += delegate {
					lastNameTextField.ResignFirstResponder ();
					return true;
				};
				emailTextField.ShouldReturn += delegate {
					emailTextField.ResignFirstResponder ();
					return true;
				};
				addressTextField.ShouldReturn += delegate {
					addressTextField.ResignFirstResponder ();
					return true;
				};
				nationalCodeTextField.ShouldReturn += delegate {
					nationalCodeTextField.ResignFirstResponder ();
					return true;
				};

				nameTextField.Text = AnatoliApp.GetInstance ().Customer.FirstName;
				lastNameTextField.Text = AnatoliApp.GetInstance ().Customer.LastName;
				emailTextField.Text = AnatoliApp.GetInstance ().Customer.Email;
				addressTextField.Text = AnatoliApp.GetInstance ().Customer.MainStreet;
				nationalCodeTextField.Text = AnatoliApp.GetInstance ().Customer.NationalCode;
				titleLabel.Text = AnatoliApp.GetInstance ().Customer.FirstName + " " + AnatoliApp.GetInstance ().Customer.LastName;
				numberLabel.Text = AnatoliApp.GetInstance ().Customer.Mobile;
				var imageUri = CustomerManager.GetImageAddress (AnatoliApp.GetInstance ().Customer.UniqueId);
				Task.Run (() => {
					using (var url = new NSUrl (imageUri)) {
						using (var data = NSData.FromUrl (url)) {
							if (data != null) {
								try {
									InvokeOnMainThread (() => {
										profileImageView.SetImage (url, UIImage.FromBundle ("ic_person_gray_24dp"));
									});
								} catch (Exception) {

								}
							}
						}
					}
				});
                
				CALayer profileImageViewLayer = profileImageView.Layer;
				profileImageViewLayer.CornerRadius = profileImageViewLayer.Bounds.Width / 2;
				profileImageViewLayer.MasksToBounds = true;
			}

			System.Threading.CancellationTokenSource token = new System.Threading.CancellationTokenSource ();

			var imagePicker = new UIImagePickerController ();
			imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
			imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes (UIImagePickerControllerSourceType.PhotoLibrary);
			imagePicker.FinishedPickingMedia += async (object sender, UIImagePickerMediaPickedEventArgs e) => {
				if (e.Info [UIImagePickerController.MediaType].ToString () == "public.image") {
					NSUrl referenceURL = e.Info [new NSString ("UIImagePickerControllerReferenceUrl")] as NSUrl;
					if (referenceURL != null)
						Console.WriteLine ("Url:" + referenceURL.ToString ());
					// get the original image
					UIImage originalImage = e.Info [UIImagePickerController.OriginalImage] as UIImage;
					if (originalImage != null) {
						// do something with the image
						var resizeImage = MaxResizeImage (originalImage, 300f, 300f);
						using (NSData imageData = resizeImage.AsJPEG ()) {
							Byte[] myByteArray = new Byte[imageData.Length];
							System.Runtime.InteropServices.Marshal.Copy (imageData.Bytes, myByteArray, 0, Convert.ToInt32 (imageData.Length));
							imagePicker.DismissViewController (true, null);
							LoadingOverlay loading = new LoadingOverlay (HeaderView.Frame, true);
							loading.Message = "بروزرسانی تصویر";
							loading.Canceled += delegate {
								token.Cancel ();
							};
							HeaderView.AddSubview (loading);
							try {
								_uploading = true;
								pickImageButton.SetTitle ("", UIControlState.Normal);
								await CustomerManager.UploadImageAsync (AnatoliApp.GetInstance ().Customer.UniqueId, myByteArray, token);
								pickImageButton.SetTitle ("ویرایش", UIControlState.Normal);
								profileImageView.Image = resizeImage; // display
							} catch (Exception) {
								var alert = new UIAlertView ("خطا", "تصویر ارسال نشد", null, "باشه");
								alert.Show ();
							} finally {
								_uploading = false;
								pickImageButton.SetTitle ("ویرایش", UIControlState.Normal);
								loading.Hide ();
							}
						}
					}

				}
			};
			imagePicker.Canceled += (object sender, EventArgs e) => {
				imagePicker.DismissViewController (true, null);
			};
			pickImageButton.TouchUpInside += (object sender, EventArgs e) => {
				if (!_uploading) {
					PresentViewController (imagePicker, true, null);
				} else {
					token.Cancel ();
					_uploading = false;
				}
			};


            
			logoutButton.TouchUpInside += async (object sender, EventArgs e) => {
				await AnatoliApp.GetInstance ().LogOutAsync ();
				AnatoliApp.GetInstance ().ReplaceViewController (new FirstPageViewController ());
			};
            

		}

		public UIImage MaxResizeImage (UIImage sourceImage, float maxWidth, float maxHeight)
		{
			var sourceSize = sourceImage.Size;
			var maxResizeFactor = Math.Max (maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
			if (maxResizeFactor > 1)
				return sourceImage;
			var width = maxResizeFactor * sourceSize.Width;
			var height = maxResizeFactor * sourceSize.Height;
			UIGraphics.BeginImageContext (new SizeF ((float)width, (float)height));
			sourceImage.Draw (new RectangleF (0, 0, (float)width, (float)height));
			var resultImage = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return resultImage;
		}


		void UpdatePickerTitle (UIButton pickerButton, RegionChooserViewController chooserViewController)
		{
			if (chooserViewController.SelectedItem != null)
				pickerButton.SetTitle (chooserViewController.SelectedItem.group_name, UIControlState.Normal);
			else
				pickerButton.SetTitle (" - - - ", UIControlState.Normal);
		}
	}



}