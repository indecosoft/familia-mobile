﻿using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Refractored.Controls;
using Square.Picasso;

namespace FamiliaXamarin
{
    class ChatAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public event EventHandler<SucHolderViewAdapterClickEventArgs> ItemLongClick;

        private List<ChatModel> _messages;
        private LayoutInflater mInflater;

        public ChatAdapter(Context context, List<ChatModel> messages)
        {
            mInflater = LayoutInflater.From(context);
            _messages = messages;
            //_imageManager = new ImageManager(resources);
        }

        public override int GetItemViewType(int position)
        {
            return _messages[position].Type;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Inflate our CrewMemberItem Layout
            //View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Srow, parent, false);

            //Create our ViewHolder to cache the layout view references and register

            //the OnClick event.
            int layout = -1;
            switch (viewType)
            {
                case 0:
                    layout = Resource.Layout.item_message;
                    break;
                case 1:
                    layout = Resource.Layout.item_log;
                    break;
                case 2:
                    layout = Resource.Layout.item_action;
                    break;
                case 3:
                    layout = Resource.Layout.item_my_message;
                    break;
            }
            //            layout = Resource.Layout.item_my_message;

            var itemView = mInflater.Inflate(layout, parent, false);

            var viewHolder = new SucHolder(itemView, OnClick);
            return viewHolder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as SucHolder;

            //            var currentCrewMember = _messages[position];
            //
            //            //Bind our data from our data source to our View References
            //            viewHolder.UsernameView.Text = currentCrewMember.Username;
            //            viewHolder.MessageView.Text = currentCrewMember.Message;
            //viewHolder._image.Text = currentCrewMember.Username;

            var message = _messages[position];
            holder.Time.Text = DateTime.Now.ToShortTimeString();
            holder.MessageView.Text = message.Message;

            //viewHolder.setAvatar(message.getAvatar());
        }

        public void Clear()
        {
            _messages.Clear();
        }
        //private readonly ImageManager _imageManager;

        //This will fire any event handlers that are registered with our ItemClick
        //event.
        private void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }
        public override int ItemCount => _messages.Count;

        //        void OnClick(DevicesRecyclerViewAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(SucHolderViewAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        class SucHolder : RecyclerView.ViewHolder
        {
            public TextView Time { get; set; }
            public TextView MessageView { get; set; }
            //public CircleImageView Image { get; set; }
            public SucHolder(View itemView, Action<int> listener)
                : base(itemView)
            {
                Time = itemView.FindViewById<TextView>(Resource.Id.time);
//                MessageView = itemView.FindViewById<TextView>(Resource.Id.message);
//                Image = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
//                UsernameView = itemView.FindViewById<TextView>(Resource.Id.username);
                MessageView = itemView.FindViewById<TextView>(Resource.Id.message);
                //                Image = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
                itemView.Click += (sender, e) => listener(base.Position);
            }
        }
        public class SucHolderViewAdapterClickEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
        }
    }
}