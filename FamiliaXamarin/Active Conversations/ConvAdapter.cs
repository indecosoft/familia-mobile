﻿using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FamiliaXamarin.JsonModels;

namespace FamiliaXamarin
{
    class ConvAdapter : RecyclerView.Adapter
    {
        public event EventHandler<ConvAdapterClickEventArgs> ItemClick;
        public event EventHandler<ConvAdapterClickEventArgs> ItemLongClick;
        private List<ConverstionsModel> _listOfActiveConversations;
        public ConvAdapter(List<ConverstionsModel> data)
        {
            _listOfActiveConversations = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            Context context = parent.Context;
            LayoutInflater inflater = LayoutInflater.From(context);

            // Inflate the custom layout
            View contactView = inflater.Inflate(Resource.Layout.item_conversations, parent, false);

            // Return a new holder instance
            ConvAdapterViewHolder viewHolder = new ConvAdapterViewHolder(contactView, OnClick, OnLongClick);
            return viewHolder;

        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
 
            var item = _listOfActiveConversations[position];
            var viewHolder = holder as ConvAdapterViewHolder;

            viewHolder.NameTextView.Text = item.Username;
        }

        public override int ItemCount => _listOfActiveConversations.Count;
        public void DeleteConversation(int position)
        {
            _listOfActiveConversations.RemoveAt(position);
        }

        void OnClick(ConvAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ConvAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class ConvAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView NameTextView { get; set; }

        public ConvAdapterViewHolder(View itemView, Action<ConvAdapterClickEventArgs> clickListener,
                            Action<ConvAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            NameTextView = itemView.FindViewById<TextView>(Resource.Id.contact_name);
            itemView.Click += (sender, e) => clickListener(new ConvAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new ConvAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class ConvAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}