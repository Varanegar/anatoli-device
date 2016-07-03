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
using Anatoli.App.Manager;
using AnatoliAndroid.ListAdapters;
using Anatoli.Framework.AnatoliBase;
using AnatoliAndroid.Activities;

namespace AnatoliAndroid.Fragments
{
    class MessagesListFragment : BaseListFragment<MessageManager, MessageListAdapter, MessageModel>
    {
        List<int> msgIds;
        public MessagesListFragment()
        {
            _listAdapter.MessageDeleted += (item) =>
                {
                    _listAdapter.List.Remove(item);
                    _listAdapter.NotifyDataSetChanged();
                    _listView.InvalidateViews();
                };
            msgIds = new List<int>();
            _listAdapter.MessageView += (msgId) =>
            {
                if (!msgIds.Contains(msgId))
                {
                    msgIds.Add(msgId);
                    _listView.InvalidateViews();
                }
            };
        }

        public override void OnDetach()
        {
            base.OnDetach();
            MessageManager.SetViewFlag(msgIds);
        }
        public override void OnStart()
        {
            base.OnStart();
            AnatoliApp.GetInstance().HideSearchIcon();
            Title = "پیغام ها";
        }

      
    }
}