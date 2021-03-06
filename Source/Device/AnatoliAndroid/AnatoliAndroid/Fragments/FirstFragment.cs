﻿using System;
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
using AnatoliAndroid.Components;
using AnatoliAndroid.Activities;
using System.Threading.Tasks;
using Android.Graphics;
using Anatoli.App.Manager;
using Anatoli.App.Model.Product;
using Square.Picasso;
using Anatoli.Framework.AnatoliBase;

namespace AnatoliAndroid.Fragments
{
    public class FirstFragment : AnatoliFragment
    {
        ImageView _slideShowImageView;
        AnatoliSlideShow _slideShow;
        GridView _groupsGridView;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.FirstLayout, container, false);
            _slideShowImageView = view.FindViewById<ImageView>(Resource.Id.slideShowImageView);
            var progress = view.FindViewById<ProgressBar>(Resource.Id.progress);
            _groupsGridView = view.FindViewById<GridView>(Resource.Id.groupsGridView);
            _slideShow = new AnatoliSlideShow(_slideShowImageView, progress);

            
            //AnatoliAndroid.Components.AnatoliSlideShow.OnClick click1 = new AnatoliAndroid.Components.AnatoliSlideShow.OnClick(() => { Toast.MakeText(AnatoliApp.GetInstance().Activity, "Item 1 selected", ToastLength.Short).Show(); });
            var c1 = new Tuple<string, AnatoliAndroid.Components.AnatoliSlideShow.OnClick>("http://parastoo.varanegar.com:7000/Content/Images/F739D5DD-8B18-42DD-9E16-910EF346B365/original/slide-1.jpg", null);
            _slideShow.Source.Add(c1);
            //AnatoliAndroid.Components.AnatoliSlideShow.OnClick click2 = new AnatoliAndroid.Components.AnatoliSlideShow.OnClick(() => { Toast.MakeText(AnatoliApp.GetInstance().Activity, "Item 2 selected", ToastLength.Short).Show(); });
            var c2 = new Tuple<string, AnatoliAndroid.Components.AnatoliSlideShow.OnClick>("http://parastoo.varanegar.com:7000/Content/Images/F739D5DD-8B18-42DD-9E16-910EF346B365/original/slide-2.jpg", null);
            _slideShow.Source.Add(c2);
            //AnatoliAndroid.Components.AnatoliSlideShow.OnClick click3 = new AnatoliAndroid.Components.AnatoliSlideShow.OnClick(() => { Toast.MakeText(AnatoliApp.GetInstance().Activity, "Item 3 selected", ToastLength.Short).Show(); });
            var c3 = new Tuple<string, AnatoliAndroid.Components.AnatoliSlideShow.OnClick>("http://parastoo.varanegar.com:7000/Content/Images/F739D5DD-8B18-42DD-9E16-910EF346B365/original/slide-3.jpg", null);
            _slideShow.Source.Add(c3);
            return view;
        }
        public override async void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideMenuIcon();
            AnatoliApp.GetInstance().ShowSearchIcon();
            Title = "ایگ مارکت";
            await AnatoliApp.GetInstance().SyncDatabase();
            try
            {
                var categories = ProductGroupManager.GetFirstLevel();
                if (categories != null)
                {
                    var groupAdapter = new GroupListAdapter(AnatoliApp.GetInstance().Activity, categories);
                    _groupsGridView.Adapter = groupAdapter;
                    ViewGroup.LayoutParams lparams = _groupsGridView.LayoutParameters;
                    var scale = Resources.DisplayMetrics.Density;
                    int pixels = (int)(120 * scale + 0.5f);
                    var c = groupAdapter.Count % 2 == 0 ? groupAdapter.Count / 2 + 1 : groupAdapter.Count / 2 + 2;
                    lparams.Height = pixels * c;
                    _groupsGridView.LayoutParameters = lparams;
                    _groupsGridView.RequestLayout();
                }
            }
            catch (Exception ex)
            {
                ex.SendTrace();
            }
            await System.Threading.Tasks.Task.Run(() => { _slideShow.Start(); });
        }


    }

    public class GroupListAdapter : BaseAdapter<ProductGroupModel>
    {
        List<ProductGroupModel> _list;
        Activity _context;
        public GroupListAdapter(Activity context, List<ProductGroupModel> list)
        {
            _list = list;
            _context = context;
        }
        public override ProductGroupModel this[int position]
        {
            get { return _list[position]; }
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
            convertView = _context.LayoutInflater.Inflate(Resource.Layout.ProductGroupGridViewItem, null);

            ProductGroupModel item = null;
            if (_list != null)
                item = _list[position];
            else
                return convertView;

            ImageView imageView1 = convertView.FindViewById<ImageView>(Resource.Id.imageView1);
            TextView textView1 = convertView.FindViewById<TextView>(Resource.Id.textView1);
            textView1.Text = item.GroupName;

            string imguri = ProductGroupManager.GetImageAddress(item.UniqueId, item.Image);
            try
            {
                if (imguri != null)
                {
                    Picasso.With(AnatoliApp.GetInstance().Activity).Load(imguri).Placeholder(Resource.Drawable.igmart).Into(imageView1);
                }
            }
            catch (Exception)
            {

            }

            imageView1.Click += (s, e) =>
            {
                var p = new ProductsListFragment();
                p.SetCatId(item.UniqueId);
                AnatoliApp.GetInstance().PushFragment(p, "products_fragment");

            };
            return convertView;
        }

    }
}