using System;
using System.Collections.Generic;
using Android.App;
using Android.Bluetooth;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Familia.Devices {
    class DevicesRecyclerViewAdapter : RecyclerView.Adapter {
        public event EventHandler<DevicesRecyclerViewAdapterClickEventArgs> ItemClick;
        private readonly List<BluetoothDevice> _mData = new List<BluetoothDevice>();
        private readonly LayoutInflater _mInflater = LayoutInflater.From(Application.Context);

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
            View itemView = _mInflater.Inflate(Resource.Layout.rowlayout, parent, false);
            return new DevicesRecyclerViewAdapterViewHolder(itemView, OnClick);
        }
        
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
            if (holder is DevicesRecyclerViewAdapterViewHolder viewholder) viewholder.TextViewName.Text = _mData[position].Name;
        }
        public override int ItemCount => _mData.Count;
        public void Add(BluetoothDevice device) {
            if (!(_mData.Find(d => d.Address == device.Address) is null)) return;
            _mData.Add(device);
            NotifyDataSetChanged();
        }
        public BluetoothDevice GetItem(int position) => _mData[position];

        private void OnClick(DevicesRecyclerViewAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
    }

    public class DevicesRecyclerViewAdapterViewHolder : RecyclerView.ViewHolder {
        public TextView TextViewName { get; }

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