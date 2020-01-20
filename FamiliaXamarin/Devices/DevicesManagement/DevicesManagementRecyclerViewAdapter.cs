using System;
using System.Collections.Generic;
using Android.Content;
using Android.Locations;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Familia;

namespace FamiliaXamarin.Devices
{
    internal class DevicesManagementRecyclerViewAdapter : RecyclerView.Adapter
    {
        public event EventHandler<DevicesManagementRecyclerViewAdapterClickEventArgs> ItemClick;
        public event EventHandler<DevicesManagementRecyclerViewAdapterClickEventArgs> ItemLongClick;
        private readonly List<DeviceEditingManagementModel> _devices;
        
        public DevicesManagementRecyclerViewAdapter( List<DeviceEditingManagementModel> data)
        {
            _devices = data;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as DevicesManagementRecyclerViewAdapterViewHolder;
            if (viewHolder != null) viewHolder.TextViewName.Text = _devices[position].Device.Name;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var context = parent.Context;
            var inflater = LayoutInflater.From(context);
            
            var layout = Resource.Layout.rowlayout;
            switch (viewType)
            {
                case 0:
                    layout = Resource.Layout.item_device_type;
                    break;
                case 1:
                    layout = Resource.Layout.rowlayout;
                    break;
            }
            //            layout = Resource.Layout.item_my_message;

            var itemView = inflater.Inflate(layout, parent, false);

            var viewHolder = new DevicesManagementRecyclerViewAdapterViewHolder(itemView , OnClick, OnLongClick);
            return viewHolder;
        }

      
        public override int ItemCount => _devices.Count;
        private void OnClick(DevicesManagementRecyclerViewAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        private void OnLongClick(DevicesManagementRecyclerViewAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }
    public class DevicesManagementRecyclerViewAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView TextViewName { get; set; }

        public DevicesManagementRecyclerViewAdapterViewHolder(View itemView, Action<DevicesManagementRecyclerViewAdapterClickEventArgs> clickListener,
            Action<DevicesManagementRecyclerViewAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            TextViewName = itemView.FindViewById<TextView>(Resource.Id.name);
            itemView.Click += (sender, e) => clickListener(new DevicesManagementRecyclerViewAdapterClickEventArgs
                {View = itemView, Position = AdapterPosition});
            itemView.LongClick += (sender, e) => longClickListener(new DevicesManagementRecyclerViewAdapterClickEventArgs
                {View = itemView, Position = AdapterPosition});
        }

    }

    public class DevicesManagementRecyclerViewAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}