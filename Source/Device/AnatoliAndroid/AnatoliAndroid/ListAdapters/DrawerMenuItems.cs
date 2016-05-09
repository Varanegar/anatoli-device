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
using AnatoliAndroid.Activities;
using Square.Picasso;
using AnatoliAndroid.Components;

namespace AnatoliAndroid.ListAdapters
{
    public class DrawerMenuItems : BaseAdapter
    {
        List<DrawerItemType> _list;
        Activity _context;
        public DrawerMenuItems(List<DrawerItemType> list, Activity context)
        {
            _list = list;
            _context = context;
        }
        public override int Count
        {
            get { return _list.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            throw new NotImplementedException();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            DrawerItemType item = null;
            if (_list != null)
                item = _list[position];
            else
                return _context.LayoutInflater.Inflate(Resource.Layout.DrawerItemLayout, null);
            if (item.GetType() == typeof(DrawerMainItem))
            {
                if (item.ItemId == DrawerMainItem.DrawerMainItems.Login || item.ItemId == DrawerMainItem.DrawerMainItems.Favorits)
                    convertView = _context.LayoutInflater.Inflate(Resource.Layout.DrawerItemLayout, null);
                else
                    convertView = _context.LayoutInflater.Inflate(Resource.Layout.DrawerSubItemLayout, null);
            }
            else
                convertView = _context.LayoutInflater.Inflate(Resource.Layout.DrawerItemLayout, null);


            TextView itemTextView = convertView.FindViewById<TextView>(Resource.Id.itemTextView);
            if (item.GetType() == typeof(DrawerMainItem) && item.ItemId == DrawerMainItem.DrawerMainItems.Login)
                itemTextView.SetTextColor(Android.Graphics.Color.Green);

            ImageView itemImageView = convertView.FindViewById<ImageView>(Resource.Id.itemIconImageView);
            RelativeLayout relativeLayout = convertView.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            itemTextView.Text = item.Name;
            //if (item.ImageUrl == null)
            //{
            //    if (item.ImageResId != -1)
            //        itemImageView.SetImageResource(item.ImageResId);
            //}
            //else if (item.ImageUrl != null)
            //{
            //    Picasso.With(AnatoliApp.GetInstance().Activity).Load(item.ImageUrl).Into(itemImageView);
            //}
            if (item.ImageResId != -1)
                itemImageView.SetImageResource(item.ImageResId);
            else if (item.GetType() == typeof(DrawerPCItem))
            {
                var pcItem = item as DrawerPCItem;
                if (pcItem.ItemType == DrawerPCItem.ItemTypes.Parent)
                {
                    itemImageView.SetImageResource(Resource.Drawable.ic_clear_white_24dp);
                }
                else
                {
                    itemImageView.Visibility = ViewStates.Invisible;
                    if (pcItem.ItemType == DrawerPCItem.ItemTypes.Leaf)
                    {
                        convertView.SetBackgroundResource(Resource.Color.lightgray);
                    }
                    else
                    {
                        itemImageView.LayoutParameters.Width += 50;
                        itemImageView.RequestLayout();
                    }
                }
            }
            else
            {
                itemImageView.Visibility = ViewStates.Invisible;
                itemImageView.LayoutParameters.Width = 5;
                itemImageView.RequestLayout();
            }

            if (item.GetType() == typeof(DrawerMainItem))
            {
                if (item.ItemId == DrawerMainItem.DrawerMainItems.MainMenu)
                {
                    relativeLayout.SetBackgroundResource(Resource.Color.lightgray);
                }
                if (item.ItemId == DrawerMainItem.DrawerMainItems.Profile)
                {
                    if (AnatoliApp.GetInstance().AnatoliUser != null)
                    {
                        Picasso.With(AnatoliApp.GetInstance().Activity).Load(item.ImageUrl).Placeholder(Resource.Drawable.ic_person_gray_24dp).Into(itemImageView);
                    }
                    else
                        relativeLayout.SetBackgroundResource(Resource.Color.lightgray);
                    itemTextView.SetTextSize(Android.Util.ComplexUnitType.Dip, 15);
                    convertView.FindViewById<ImageView>(Resource.Id.inside_imageview).Visibility = ViewStates.Visible;
                }
            }
            return convertView;
        }
    }
    public abstract class DrawerItemType
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public int ImageResId { get; set; }
        public string ImageUrl { get; set; }
        public DrawerItemType(string itemId = "", string name = "Menu Item", int imageResId = -1)
        {
            ItemId = itemId;
            Name = name;
            ImageResId = imageResId;
        }
    }
    class DrawerPCItem : DrawerItemType
    {
        public ItemTypes ItemType;

        public DrawerPCItem(string itemId, string itemName, ItemTypes type = ItemTypes.Normal)
            : base(itemId, itemName, -1)
        {
            ItemType = type;
        }
        public enum ItemTypes
        {
            Parent = 0,
            Normal,
            Leaf
        }
    }
    class DrawerMainItem : DrawerItemType
    {
        public DrawerMainItem() : base() { }
        public DrawerMainItem(string itemId, string name)
            : base(itemId, name)
        {
        }
        public struct DrawerMainItems
        {
            public const string MainMenu = "MainMenu";
            public const string ProductCategries = "ProductCategries";
            public const string ShoppingCard = "ShoppingCard";
            public const string Profile = "Profile";
            public const string Help = "Help";
            public const string Login = "Login";
            public const string StoresList = "StoresList";
            public const string Favorits = "Favorits";
            public const string Logout = "Logout";
            public const string Messages = "Messages";
            public const string Orders = "Orders";
            public const string Update = "Update";
            public const string FirstPage = "FirstPage";
            public const string Settings = "Settings";
            public const string About = "About";
        }
    }
}