using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace FamiliaXamarin.Devices
{
    class DevicesRecyclerViewAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public event EventHandler<DevicesRecyclerViewAdapterClickEventArgs> ItemLongClick;
        //string[] items;
        List<string> mData;
        LayoutInflater mInflater;

        public DevicesRecyclerViewAdapter(Context context, List<string> data)
        {
            mInflater = LayoutInflater.From(context);
            mData = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = mInflater.Inflate(Resource.Layout.rowlayout, parent, false);

            //Create our ViewHolder to cache the layout view references and register
            //the OnClick event.
            var viewHolder = new DevicesRecyclerViewAdapterViewHolder(itemView, OnClick);
            return viewHolder;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewholder = holder as DevicesRecyclerViewAdapterViewHolder;
            var currentCrewMember = mData[position];
            if (viewholder != null) viewholder.TextViewName.Text = currentCrewMember;

            // Replace the contents of the view with that element
            //var holder = viewHolder as DevicesRecyclerViewAdapterViewHolder;
            //holder.TextView.Text = items[position];
        }
        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
        public override int ItemCount => mData.Count;

//        void OnClick(DevicesRecyclerViewAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(DevicesRecyclerViewAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class DevicesRecyclerViewAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView TextViewName { get; set; }

        public DevicesRecyclerViewAdapterViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            TextViewName = (TextView) itemView.FindViewById(Resource.Id.name);
            itemView.Click += (sender, e) => listener?.Invoke(obj: Position);
        }

    }

    public class DevicesRecyclerViewAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}