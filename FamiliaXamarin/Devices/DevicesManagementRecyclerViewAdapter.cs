using System;
using System.Collections.Generic;
using Android.Content;
using Android.Locations;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace FamiliaXamarin.Devices
{
    public class DevicesManagementRecyclerViewAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public event EventHandler<DevicesManagementRecyclerViewAdapterClickEventArgs> ItemLongClick;
        List<DevicesManagementModel> _devices;
        LayoutInflater mInflater;
        
        public DevicesManagementRecyclerViewAdapter(Context context, List<DevicesManagementModel> data)
        {
            mInflater = LayoutInflater.From(context);
            _devices = data;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is DevicesRecyclerViewAdapterViewHolder viewHolder) viewHolder.TextViewName.Text = _devices[position].ItemValue;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var layout = -1;
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

            var itemView = mInflater.Inflate(layout, parent, false);

            var viewHolder = new DevicesManagementRecyclerViewAdapterViewHolder(itemView , OnClick);
            return viewHolder;
        }

        private void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
        public override int ItemCount { get; }
        private void OnLongClick(DevicesManagementRecyclerViewAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }
    public class DevicesManagementRecyclerViewAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView TextViewName { get; set; }

        public DevicesManagementRecyclerViewAdapterViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            TextViewName = itemView.FindViewById<TextView>(Resource.Id.name);
            itemView.Click += (sender, e) => listener?.Invoke(obj: Position);
        }

    }

    public abstract class DevicesManagementRecyclerViewAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}