using System;
using System.Collections.Generic;
using Android.App;
using Android.Bluetooth;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Familia;

namespace FamiliaXamarin.Devices {
    class DevicesRecyclerViewAdapter : RecyclerView.Adapter {
        public event EventHandler<DevicesRecyclerViewAdapterClickEventArgs> ItemClick;
        private readonly List<BluetoothDevice> mData = new List<BluetoothDevice>();
        private readonly LayoutInflater mInflater = LayoutInflater.From(Application.Context);

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
            var itemView = mInflater.Inflate(Resource.Layout.rowlayout, parent, false);
            return new DevicesRecyclerViewAdapterViewHolder(itemView, OnClick);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
            if (holder is DevicesRecyclerViewAdapterViewHolder viewholder) viewholder.TextViewName.Text = mData[position].Name;
        }
        public override int ItemCount => mData.Count;
        public void Add(BluetoothDevice device) {
            if (mData.Find(d => d.Address == device.Address) is null) {
                mData.Add(device);
                NotifyDataSetChanged();
            }
        }
        public BluetoothDevice GetItem(int position) => mData[position];

        void OnClick(DevicesRecyclerViewAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
    }

    public class DevicesRecyclerViewAdapterViewHolder : RecyclerView.ViewHolder {
        public TextView TextViewName { get; set; }

        public DevicesRecyclerViewAdapterViewHolder(View itemView, Action<DevicesRecyclerViewAdapterClickEventArgs> listener) : base(itemView) {
            TextViewName = (TextView)itemView.FindViewById(Resource.Id.name);
            itemView.Click += (sender, e) => listener(new DevicesRecyclerViewAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class DevicesRecyclerViewAdapterClickEventArgs : EventArgs {
        public View View { get; set; }
        public int Position { get; set; }
    }
}