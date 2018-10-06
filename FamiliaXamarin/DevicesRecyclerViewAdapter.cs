using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace FamiliaXamarin
{
    class DevicesRecyclerViewAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public event EventHandler<DevicesRecyclerViewAdapterClickEventArgs> ItemLongClick;
        //string[] items;
        private List<string> mData;
        private LayoutInflater mInflater;

        public DevicesRecyclerViewAdapter(Context context, List<string> data)
        {
            this.mInflater = LayoutInflater.From(context);
            this.mData = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = mInflater.Inflate(Resource.Layout.rowlayout, parent, false);

            //Create our ViewHolder to cache the layout view references and register
            //the OnClick event.
            var viewHolder = new DevicesRecyclerViewAdapterViewHolder(itemView, OnClick);
            return viewHolder;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as DevicesRecyclerViewAdapterViewHolder ;
            var currentCrewMember = mData[position];
            holder.myTextView.Text = currentCrewMember;

            // Replace the contents of the view with that element
            //var holder = viewHolder as DevicesRecyclerViewAdapterViewHolder;
            //holder.TextView.Text = items[position];
        }
        private void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }
        public override int ItemCount => mData.Count;

//        void OnClick(DevicesRecyclerViewAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(DevicesRecyclerViewAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class DevicesRecyclerViewAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public TextView myTextView { get; set; }

        public DevicesRecyclerViewAdapterViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            myTextView = (TextView) itemView.FindViewById(Resource.Id.name);
            itemView.Click += (sender, e) => listener(base.Position);
            //itemView.SetOnClickListener(this);
        }

//        public DevicesRecyclerViewAdapterViewHolder(View itemView, Action<DevicesRecyclerViewAdapterClickEventArgs> clickListener,
//                            Action<DevicesRecyclerViewAdapterClickEventArgs> longClickListener) : base(itemView)
//        {
//            //TextView = v;
//            itemView.Click += (sender, e) => clickListener(new DevicesRecyclerViewAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
//            itemView.LongClick += (sender, e) => longClickListener(new DevicesRecyclerViewAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
//        }

    }

    public class DevicesRecyclerViewAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}