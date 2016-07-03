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
using Anatoli.App.Manager;
using Anatoli.App.Model.AnatoliUser;
using AnatoliAndroid.Activities;
using Anatoli.Framework.AnatoliBase;
using Anatoli.App.Model;
using Anatoli.App.Model.Store;
using Android.Graphics;
using System.Net;
using System.Threading.Tasks;
using Android.Provider;
using Android.Database;
using AnatoliAndroid.Components;
using Square.Picasso;
using Anatoli.Framework;
using AnatoliAndroid.ListAdapters;

namespace AnatoliAndroid.Fragments
{
    public class ProfileFragment : DialogFragment
    {
        EditText _firstNameEditText;
        EditText _lastNameEditText;
        EditText _emailEditText;
        TextView _telTextView;
        EditText _addressEditText;
        EditText _idEditText;
        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level4List;
        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level3List;
        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level2List;
        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level1List;
        TextView _exitTextView;
        TextView _fullNametextView;
        Button _saveButton;
        RoundedImageView _avatarImageView;
        CustomerViewModel _customerViewModel;
        ProgressBar _progress;
        ImageView _cancelImageView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.ProfileLayout, container, false);
            _firstNameEditText = view.FindViewById<EditText>(Resource.Id.firstNameEditText);
            _lastNameEditText = view.FindViewById<EditText>(Resource.Id.lastNameEditText);
            _emailEditText = view.FindViewById<EditText>(Resource.Id.emailEditText);
            _idEditText = view.FindViewById<EditText>(Resource.Id.idEditText);
            _telTextView = view.FindViewById<TextView>(Resource.Id.telTextView);
            _addressEditText = view.FindViewById<EditText>(Resource.Id.addressEditText);
            _level4List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level4Spinner);
            _level3List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level3Spinner);
            _level2List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level2Spinner);
            _level1List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level1Spinner);
            _avatarImageView = view.FindViewById<RoundedImageView>(Resource.Id.avatarImageView);
            _progress = view.FindViewById<ProgressBar>(Resource.Id.progress);
            _cancelImageView = view.FindViewById<ImageView>(Resource.Id.cancelImageView);

            ImageUploaded += (s, e) =>
            {
                var imageUri = CustomerManager.GetImageAddress(_customerViewModel.UniqueId);
                Picasso.With(AnatoliApp.GetInstance().Activity).Load(imageUri).MemoryPolicy(MemoryPolicy.NoCache).NetworkPolicy(NetworkPolicy.NoCache).Placeholder(Resource.Drawable.ic_account_circle_white_24dp).Into(_avatarImageView);
                Toast.MakeText(AnatoliApp.GetInstance().Activity, "تصویر ارسال شد", ToastLength.Short).Show();
                _progress.Visibility = ViewStates.Gone;
            };
            ImageUploadFailed += (s, e) =>
            {
                Picasso.With(AnatoliApp.GetInstance().Activity).Load(CustomerManager.GetImageAddress(_customerViewModel.UniqueId)).Placeholder(Resource.Drawable.ic_account_circle_white_24dp).Into(_avatarImageView);
                Toast.MakeText(AnatoliApp.GetInstance().Activity, "خطا در ارسال تصویر", ToastLength.Short).Show();
                _progress.Visibility = ViewStates.Gone;
            };

            _avatarImageView.Click += (s, e) =>
            {
                OpenImage();
            };
            _fullNametextView = view.FindViewById<TextView>(Resource.Id.fullNametextView);
            view.FindViewById<TextView>(Resource.Id.changePassTextView).Click += (s, e) =>
            {
                if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    var alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetMessage("لطفا دستگاه خود را به اینترنت متصل نمایید");
                    alert.SetPositiveButton(Resource.String.Ok, delegate { });
                    alert.Show();
                    return;
                }
                ChangePassFragment fragment = new ChangePassFragment();
                var transaction = AnatoliApp.GetInstance().Activity.FragmentManager.BeginTransaction();
                fragment.Show(transaction, "changepass_fragment");
                Dismiss();
            };

            _saveButton = view.FindViewById<Button>(Resource.Id.saveButton);
            _saveButton.UpdateWidth();

            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            Dialog.SetCanceledOnTouchOutside(false);


            _saveButton.Click += async (s, e) =>
            {
                if (!IsValidEmail(_emailEditText.Text))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage(Resource.String.PleaseEnterValidEmail);
                    alert.Show();
                    return;
                }
                if (String.IsNullOrEmpty(_idEditText.Text) || String.IsNullOrEmpty(_firstNameEditText.Text) || String.IsNullOrEmpty(_lastNameEditText.Text) || String.IsNullOrEmpty(_addressEditText.Text))
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    alert.SetTitle(Resource.String.Error);
                    alert.SetMessage("ورود نام، نام خانوادگی، شماره ملی و آدرس الزامی است");
                    alert.Show();
                    return;
                }
                _customerViewModel.MainStreet = _addressEditText.Text;
                _customerViewModel.Email = _emailEditText.Text;
                _customerViewModel.NationalCode = _idEditText.Text;
                _customerViewModel.FirstName = _firstNameEditText.Text;
                _customerViewModel.LastName = _lastNameEditText.Text;
                if (_level1List.SelectedItem != null)
                {
                    _customerViewModel.RegionLevel1Id = _level1List.SelectedItem.UniqueId;
                }
                if (_level2List.SelectedItem != null)
                {
                    _customerViewModel.RegionLevel2Id = _level2List.SelectedItem.UniqueId;
                }
                if (_level3List.SelectedItem != null)
                {
                    _customerViewModel.RegionLevel3Id = _level3List.SelectedItem.UniqueId;
                }
                if (_level4List.SelectedItem != null)
                {
                    _customerViewModel.RegionLevel4Id = _level4List.SelectedItem.UniqueId;
                }
                AlertDialog.Builder dialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                {
                    dialog.SetTitle(Resources.GetText(Resource.String.NetworkAccessFailed));
                    dialog.SetMessage(Resources.GetText(Resource.String.PleaseConnectToInternet));
                    dialog.SetPositiveButton(Resource.String.Ok, (s2, e2) =>
                    {
                        Intent intent = new Intent(Android.Provider.Settings.ActionSettings);
                        AnatoliApp.GetInstance().Activity.StartActivity(intent);
                    });
                    dialog.SetNegativeButton(Resource.String.Cancel, (s2, e2) => { });
                    dialog.Show();
                    return;
                }
                ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                try
                {
                    pDialog.SetTitle(Resources.GetText(Resource.String.Updating));
                    pDialog.SetMessage(Resources.GetText(Resource.String.PleaseWait));
                    pDialog.Show();
                    var result = await CustomerManager.UploadCustomerAsync(_customerViewModel);
                    pDialog.Dismiss();
                    if (result.IsValid)
                    {
                        await CustomerManager.SaveCustomerAsync(_customerViewModel);
                        dialog.SetTitle("");
                        dialog.SetMessage("اطلاعات بروزرسانی شد");
                        dialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                        dialog.Show();
                        OnProfileUpdated();
                        Dismiss();
                    }
                    else
                    {
                        dialog.SetTitle("خطا");
                        dialog.SetMessage(result.ModelStateString);
                        dialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                        dialog.Show();
                    }
                }
                catch (Exception ex)
                {
                    ex.SendTrace();
                    pDialog.Dismiss();
                    dialog.SetMessage(Resource.String.ErrorOccured);
                    dialog.SetTitle("خطا");
                    dialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                    dialog.Show();
                }
            };
            _exitTextView = view.FindViewById<TextView>(Resource.Id.logoutTextView);
            _exitTextView.Click += (s, e) =>
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                alert.SetMessage(Resources.GetText(Resource.String.AreYouSure));
                alert.SetPositiveButton(Resources.GetText(Resource.String.Yes), (s2, e2) =>
                {
                    try
                    {
                        bool result = AnatoliApp.GetInstance().Logout();
                        if (result)
                        {
                            AnatoliApp.GetInstance().PushFragment(new FirstFragment(), "first_fragment");
                            Dismiss();
                        }

                    }
                    catch (Exception ex)
                    {
                        ex.SendTrace();
                    }
                });
                alert.SetNegativeButton(Resources.GetText(Resource.String.No), (s2, e2) => { });
                alert.Show();
            };
            return view;
        }


        public async override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideMenuIcon();

            _level1List.ItemSelected += _level1_ItemSelected;
            _level2List.ItemSelected += _level2_ItemSelected;
            _level3List.ItemSelected += _level3_ItemSelected;
            var list = CityRegionManager.GetFirstLevel();
            foreach (var item in list)
            {
                item.UniqueId = item.UniqueId;
            }
            _level1List.SetList(list);
            try
            {
                if (_customerViewModel == null)
                    _customerViewModel = AnatoliApp.GetInstance().Customer;
                if (_customerViewModel == null)
                    _customerViewModel = await AnatoliApp.GetInstance().RefreshCutomerProfile(true);
                if (_customerViewModel != null)
                {
                    _firstNameEditText.Text = _customerViewModel.FirstName;
                    _lastNameEditText.Text = _customerViewModel.LastName;
                    _idEditText.Text = _customerViewModel.NationalCode;
                    _addressEditText.Text = _customerViewModel.MainStreet;
                    _emailEditText.Text = _customerViewModel.Email;
                    _telTextView.Text = _customerViewModel.Mobile;
                    _fullNametextView.Text = _customerViewModel.FirstName.Trim() + " " + _customerViewModel.LastName.Trim();
                    Picasso.With(AnatoliApp.GetInstance().Activity).Load(CustomerManager.GetImageAddress(_customerViewModel.UniqueId)).Placeholder(Resource.Drawable.ic_account_circle_white_24dp).Into(_avatarImageView);

                    if (_customerViewModel.RegionLevel1Id != null)
                    {
                        var level1 = CityRegionManager.GetGroupInfo((Guid)_customerViewModel.RegionLevel1Id);
                        _level1List.SelectItem(level1.UniqueId);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.SendTrace();
            }

        }
        public static bool IsValidEmail(string target)
        {
            if (target == null)
            {
                return false;
            }
            else
            {
                return Android.Util.Patterns.EmailAddress.Matcher(target).Matches();
            }
        }
        public static bool IsValidPhoneNumber(string target)
        {
            if (target == null)
            {
                return false;
            }
            else
            {
                return Android.Util.Patterns.Phone.Matcher(target).Matches();
            }
        }
        void _level2_ItemSelected(CityRegionModel item)
        {
            try
            {
                var list = CityRegionManager.GetGroups(item.UniqueId);
                foreach (var t in list)
                {
                    t.UniqueId = t.UniqueId;
                }
                _level3List.SetList(list);
                if (_customerViewModel.RegionLevel3Id != null)
                {
                    var level3 = CityRegionManager.GetGroupInfo((Guid)_customerViewModel.RegionLevel3Id);
                    _level3List.SelectItem(level3.UniqueId);
                }
                else
                {
                    _level3List.Text = "محله 1";
                }
            }
            catch (Exception)
            {

            }
        }

        void _level3_ItemSelected(CityRegionModel item)
        {
            try
            {
                var list = CityRegionManager.GetGroups(item.UniqueId);
                foreach (var t in list)
                {
                    t.UniqueId = t.UniqueId;
                }
                _level4List.SetList(list);
                if (_customerViewModel.RegionLevel4Id != null)
                {
                    var level4 = CityRegionManager.GetGroupInfo((Guid)_customerViewModel.RegionLevel4Id);
                    _level4List.SelectItem(level4.UniqueId);
                }
                else
                {
                    _level4List.Text = "محله 2";
                }
            }
            catch (Exception)
            {

            }
        }

        async void _level1_ItemSelected(CityRegionModel item)
        {
            try
            {
                var list = CityRegionManager.GetGroups(item.UniqueId);
                foreach (var t in list)
                {
                    t.UniqueId = t.UniqueId;
                }
                _level2List.SetList(list);
                if (_customerViewModel.RegionLevel2Id != null)
                {
                    var level2 = CityRegionManager.GetGroupInfo((Guid)_customerViewModel.RegionLevel2Id);
                    _level2List.SelectItem(level2.UniqueId);
                }
                else
                {
                    _level2List.Deselect();
                }
            }
            catch (Exception)
            {

            }
        }
        public static readonly int OpenImageRequestCode = 1234;
        public override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == OpenImageRequestCode)
            {
                try
                {
                    _progress.Visibility = ViewStates.Visible;
                    _cancelImageView.Visibility = ViewStates.Visible;
                    _avatarImageView.Enabled = false;
                    if (data.Data != null)
                    {
                        var path = AndroidFileIO.GetPathToImage(data.Data, AnatoliApp.GetInstance().Activity);
                        var imageBytes = AnatoliClient.GetInstance().FileClient.ReadAllBytes(path);
                        System.Threading.CancellationTokenSource cancelToken = new System.Threading.CancellationTokenSource();
                        _cancelImageView.Click += delegate
                        {
                            cancelToken.Cancel();
                            _progress.Visibility = ViewStates.Gone;
                            _cancelImageView.Visibility = ViewStates.Gone;
                        };
                        byte[] scaledImageBytes;
                        Bitmap bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                        var r = bitmap.Width / 300.0;
                        var scaledBitMap = Bitmap.CreateScaledBitmap(bitmap, 300, (int)Math.Floor(bitmap.Height / r), false);
                        System.IO.MemoryStream stream = new System.IO.MemoryStream();
                        scaledBitMap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);

                        scaledImageBytes = stream.ToArray();
                        await CustomerManager.UploadImageAsync(_customerViewModel.UniqueId, scaledImageBytes, cancelToken);
                        OnImageUploaded();
                    }
                }
                catch (Exception e)
                {
                    if (e.GetType() != typeof(TaskCanceledException))
                    {
                        OnImageUploadFailed();
                        e.SendTrace();
                    }
                }
                finally
                {
                    _progress.Visibility = ViewStates.Gone;
                    _cancelImageView.Visibility = ViewStates.Gone;
                    _avatarImageView.Enabled = true;
                }
            }
        }
        void OnImageUploaded()
        {
            if (ImageUploaded != null)
            {
                ImageUploaded.Invoke(this, new EventArgs());
            }
        }
        public EventHandler ImageUploaded;

        void OnImageUploadFailed()
        {
            if (ImageUploadFailed != null)
            {
                ImageUploadFailed.Invoke(this, new EventArgs());
            }
        }
        public EventHandler ImageUploadFailed;
        public void OpenImage()
        {
            try
            {
                Intent intent = new Intent(Intent.ActionGetContent);
                intent.SetType("image/*");
                StartActivityForResult(intent, OpenImageRequestCode);
            }
            catch (ActivityNotFoundException e)
            {

            }
        }

        void OnProfileUpdated()
        {
            if (ProfileUpdated != null)
            {
                ProfileUpdated.Invoke();
            }
        }
        public event ProfileUpdatedEventHandler ProfileUpdated;
        public delegate void ProfileUpdatedEventHandler();
    }
}