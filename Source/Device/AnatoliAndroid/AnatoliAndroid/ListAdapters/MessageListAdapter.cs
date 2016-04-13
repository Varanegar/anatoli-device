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
using Anatoli.App.Manager;
using Anatoli.App.Model.Store;
using AnatoliAndroid.Fragments;

namespace AnatoliAndroid.ListAdapters
{
    class MessageListAdapter : BaseListAdapter<MessageManager, MessageModel>
    {
        public override View GetItemView(int position, View convertView, ViewGroup parent)
        {
            convertView = _context.LayoutInflater.Inflate(Resource.Layout.MessageItemLayout, null);
            MessageModel item = null;
            if (List != null)
                item = List[position];
            else
                return convertView;
            TextView contentTextView = convertView.FindViewById<TextView>(Resource.Id.contentTextView);
            TextView storeNameTextView = convertView.FindViewById<TextView>(Resource.Id.storeNameTextView);
            TextView dateTextView = convertView.FindViewById<TextView>(Resource.Id.dateTextView);
            TextView timeTextView = convertView.FindViewById<TextView>(Resource.Id.timeTextView);
            Button removeButton = convertView.FindViewById<Button>(Resource.Id.removeButton);

            removeButton.UpdateWidth();
            contentTextView.Text = item.content;
            storeNameTextView.Text = item.store_name;
            dateTextView.Text = item.date;
            timeTextView.Text = item.time;
            //MessageManager.SetViewFlag(item.msg_id);
            if (item.IsNewMsg)
            {
                item.new_flag = 0;
                NotifyDataSetChanged();
                OnMessageView(item.msg_id);
            }
            
            removeButton.Click += async (s, e) =>
                {
                    if (await MessageManager.DeleteAsync(item.msg_id))
                    {
                        OnMessageDeleted(item);
                    }
                };

            return convertView;
        }

        void OnMessageDeleted(MessageModel item)
        {
            if (MessageDeleted != null)
            {
                MessageDeleted.Invoke(item);
            }
        }
        public event MessageDeletedHandler MessageDeleted;
        public delegate void MessageDeletedHandler(MessageModel item);
        void OnMessageView(int id)
        {
            if (MessageView != null)
            {
                MessageView.Invoke(id);
            }
        }
        public event MessageViewHandler MessageView;
        public delegate void MessageViewHandler(int msgId);
    }
}