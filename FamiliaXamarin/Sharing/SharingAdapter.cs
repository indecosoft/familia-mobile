using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Familia;
using FamiliaXamarin.JsonModels;

namespace FamiliaXamarin.Sharing
{
    class SharingAdapter : RecyclerView.Adapter
    {
        public event EventHandler<SharingAdapterClickEventArgs> ItemClick;
        public event EventHandler<SharingAdapterClickEventArgs> ItemLongClick;
        List<SharingModel> items;

        public SharingAdapter(List<SharingModel> data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

//            //Setup your layout here
//            View itemView = null;
//            //var id = Resource.Layout.__YOUR_ITEM_HERE;
//            //itemView = LayoutInflater.From(parent.Context).
//            //       Inflate(id, parent, false);
//
//            var vh = new SharingAdapterViewHolder(itemView, OnClick, OnLongClick);
           // return vh;

            var context = parent.Context;
            var inflater = LayoutInflater.From(context);

            // Inflate the custom layout
            var contactView = inflater.Inflate(Resource.Layout.item_sharing, parent, false);

            // Return a new holder instance
            var viewHolder = new SharingAdapterViewHolder(contactView, OnClick, OnLongClick);
            return viewHolder;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = items[position];

             if (holder is SharingAdapterViewHolder viewHolder)

                 viewHolder.NameTextView.Text = item.Name;

            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => items.Count;

        void OnClick(SharingAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(SharingAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public void DeleteItemAt(int position)
        {
            items.RemoveAt(position);
        }
    }

    public class SharingAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public TextView NameTextView { get; }

        public SharingAdapterViewHolder(View itemView, Action<SharingAdapterClickEventArgs> clickListener,
                            Action<SharingAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            NameTextView = itemView.FindViewById<TextView>(Resource.Id.person_name);
            itemView.Click += (sender, e) => clickListener(new SharingAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new SharingAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class SharingAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}