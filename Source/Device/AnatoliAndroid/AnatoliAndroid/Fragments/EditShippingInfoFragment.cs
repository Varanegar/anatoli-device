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
using Anatoli.App.Model.Store;
using AnatoliAndroid.Activities;
using Anatoli.App.Model;
using Anatoli.App.Manager;
using Anatoli.Framework.AnatoliBase;
using Anatoli.Framework;
using AnatoliAndroid.Components;
using AnatoliAndroid.ListAdapters;

namespace AnatoliAndroid.Fragments
{
    public class EditShippingInfoFragment : Android.Support.V4.App.DialogFragment
    {
        EditText _addressEditText;
        //EditText _nameEditText;
        //EditText _telEditText;
        Button _button;
        string _address;
        string _tel;
        string _name;

        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level4List;
        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level3List;
        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level2List;
        AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel> _level1List;

        CustomerViewModel _customerViewModel;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.ShippingInfoEditLayout, container, false);
            Dialog.SetCanceledOnTouchOutside(false);

            _level4List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level4Spinner);
            _level3List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level3Spinner);
            _level2List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level2Spinner);
            _level1List = view.FindViewById<AnatoliListBox<CityRegionListAdapter, CityRegionManager, CityRegionModel>>(Resource.Id.level1Spinner);

            _addressEditText = view.FindViewById<EditText>(Resource.Id.addressEditText);
            //_nameEditText = view.FindViewById<EditText>(Resource.Id.nameEditText);
            //_telEditText = view.FindViewById<EditText>(Resource.Id.telEditText);
            if (_address != null)
                _addressEditText.Text = _address;
            //if (_tel != null)
            //    _telEditText.Text = _tel;
            //if (_name != null)
            //    _nameEditText.Text = _name;

            _button = view.FindViewById<Button>(Resource.Id.saveButton);
            _button.UpdateWidth();
            _button.Click += async (s, e) =>
                {
                    _customerViewModel.MainStreet = _addressEditText.Text;
                    if (_level1List.SelectedItem != null)
                    {
                        _customerViewModel.RegionLevel1Id = _level1List.SelectedItem.group_id;
                    }
                    if (_level2List.SelectedItem != null)
                    {
                        _customerViewModel.RegionLevel2Id = _level2List.SelectedItem.group_id;
                    }
                    if (_level3List.SelectedItem != null)
                    {
                        _customerViewModel.RegionLevel3Id = _level3List.SelectedItem.group_id;
                    }
                    if (_level4List.SelectedItem != null)
                    {
                        _customerViewModel.RegionLevel4Id = _level4List.SelectedItem.group_id;
                    }
                    AlertDialog.Builder errDialog = new AlertDialog.Builder(AnatoliApp.GetInstance().Activity);
                    if (!AnatoliClient.GetInstance().WebClient.IsOnline())
                    {
                        errDialog.SetTitle(Resources.GetText(Resource.String.NetworkAccessFailed));
                        errDialog.SetMessage(Resources.GetText(Resource.String.PleaseConnectToInternet));
                        errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) =>
                        {
                            Intent intent = new Intent(Android.Provider.Settings.ActionSettings);
                            AnatoliApp.GetInstance().Activity.StartActivity(intent);
                        });
                        errDialog.SetNegativeButton(Resource.String.Cancel, (s2, e2) => { });
                        errDialog.Show();
                        return;
                    }
                    ProgressDialog pDialog = new ProgressDialog(AnatoliApp.GetInstance().Activity);
                    try
                    {
                        pDialog.SetTitle(AnatoliApp.GetResources().GetText(Resource.String.Updating));
                        pDialog.SetMessage(AnatoliApp.GetResources().GetText(Resource.String.PleaseWait));
                        pDialog.Show();
                        var result = await CustomerManager.UploadCustomerAsync(_customerViewModel);
                        pDialog.Dismiss();
                        if (result.IsValid)
                        {
                            await CustomerManager.SaveCustomerAsync(_customerViewModel);
                            errDialog.SetTitle("");
                            errDialog.SetMessage("اطلاعات بروزرسانی شد");
                            errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) =>
                            {
                                OnShippingInfoChanged();
                                Dismiss();
                            });
                            errDialog.Show();

                        }
                        else
                        {
                            pDialog.Dismiss();
                            errDialog.SetTitle("خطا");
                            errDialog.SetMessage(result.ModelStateString);
                            errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                            errDialog.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.SendTrace();
                        pDialog.Dismiss();
                        errDialog.SetMessage(Resource.String.ErrorOccured);
                        errDialog.SetPositiveButton(Resource.String.Ok, (s2, e2) => { });
                        errDialog.SetTitle("خطا");
                        errDialog.Show();
                    }
                };
            return view;
        }
        public override async void OnStart()
        {
            base.OnStart();

            if (_customerViewModel == null)
                _customerViewModel = AnatoliApp.GetInstance().Customer;
            if (_customerViewModel == null)
                _customerViewModel = await AnatoliApp.GetInstance().RefreshCutomerProfile(true);

            if (_customerViewModel.IsValid)
            {
                _addressEditText.Text = _customerViewModel.MainStreet;
                _level1List.ItemSelected += _level1_ItemSelected;
                _level2List.ItemSelected += _level2_ItemSelected;
                _level3List.ItemSelected += _level3_ItemSelected;
                var list = await CityRegionManager.GetFirstLevelAsync();
                foreach (var item in list)
                {
                    item.UniqueId = item.group_id;
                }
                _level1List.SetList(list);
                if (!String.IsNullOrEmpty(_customerViewModel.RegionLevel1Id))
                {
                    var level1 = await CityRegionManager.GetGroupInfoAsync(_customerViewModel.RegionLevel1Id);
                    _level1List.SelectItem(level1.group_id);
                }
            }

        }


        async void _level2_ItemSelected(CityRegionModel item)
        {
            try
            {
                var list = await CityRegionManager.GetGroupsAsync(item.group_id);
                foreach (var t in list)
                {
                    t.UniqueId = t.group_id;
                }
                _level3List.SetList(list);
                if (!string.IsNullOrEmpty(_customerViewModel.RegionLevel3Id))
                {
                    var level3 = await CityRegionManager.GetGroupInfoAsync(_customerViewModel.RegionLevel3Id);
                    _level3List.SelectItem(level3.group_id);
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

        async void _level3_ItemSelected(CityRegionModel item)
        {
            try
            {
                var list = await CityRegionManager.GetGroupsAsync(item.group_id);
                foreach (var t in list)
                {
                    t.UniqueId = t.group_id;
                }
                _level4List.SetList(list);
                if (!string.IsNullOrEmpty(_customerViewModel.RegionLevel4Id))
                {
                    var level4 = await CityRegionManager.GetGroupInfoAsync(_customerViewModel.RegionLevel4Id);
                    _level4List.SelectItem(level4.group_id);
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
                var list = await CityRegionManager.GetGroupsAsync(item.group_id);
                foreach (var t in list)
                {
                    t.UniqueId = t.group_id;
                }
                _level2List.SetList(list);
                if (!string.IsNullOrEmpty(_customerViewModel.RegionLevel2Id))
                {
                    var level2 = await CityRegionManager.GetGroupInfoAsync(_customerViewModel.RegionLevel2Id);
                    _level2List.SelectItem(level2.group_id);
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

        public void SetAddress(string address)
        {
            _address = address;
        }
        public void SetTel(string tel)
        {
            _tel = tel;
        }
        public void SetName(string name)
        {
            _name = name;
        }
        void OnShippingInfoChanged()
        {
            if (ShippingInfoChanged != null)
            {
                //ShippingInfoChanged.Invoke(_addressEditText.Text, _nameEditText.Text, _telEditText.Text);
                ShippingInfoChanged.Invoke(_addressEditText.Text, "", "");
            }
        }
        public event ShippingInfoChangedHandler ShippingInfoChanged;
        public delegate void ShippingInfoChangedHandler(string address, string name, string tel);

    }
}