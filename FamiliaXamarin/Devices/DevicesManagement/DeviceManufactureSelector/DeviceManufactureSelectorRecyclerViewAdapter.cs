using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Familia.Devices.Helpers;
using Familia.Devices.Models;

namespace Familia.Devices {
    public class DeviceManufactureSelectorRecyclerViewAdapter : RecyclerView.Adapter{
        
        public event EventHandler<DeviceSelectorAdapterClickEventArgs> ItemClick;
        private List<SupportedDeviceModel> items;
        private readonly List<SupportedDeviceModel> itemsCopy;
        private readonly LayoutInflater mInflater;

        public DeviceManufactureSelectorRecyclerViewAdapter(Context context, List<SupportedDeviceModel> items) {
            mInflater = LayoutInflater.From(context);
            this.items = items;
            this.itemsCopy = items;
        }

        public override int ItemCount => items.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
            var item = items[position];
            if (!(holder is DeviceSelectorHolder viewHolder)) return;
            viewHolder.Item.Text = item.DeviceName;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
            int layout = Resource.Layout.item_device_selector;

            var itemView = mInflater.Inflate(layout, parent, false);

            var viewHolder = new DeviceSelectorHolder(itemView, OnClick);
            return viewHolder;
        }
        void OnClick(DeviceSelectorAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);

        public SupportedDeviceModel GetItem(int position) => items[position];

        public void Search(string textToSearch) {
            items = itemsCopy.Where(c => c.DeviceName.ToLower().StartsWith(textToSearch.ToLower(), StringComparison.Ordinal)).ToList();
            NotifyDataSetChanged();
        }
        
    }
    class DeviceSelectorHolder : RecyclerView.ViewHolder {
        public TextView Item { get; set; }
        public DeviceSelectorHolder(View itemView, Action<DeviceSelectorAdapterClickEventArgs> clickListener) : base(itemView) {
            itemView.Click += (sender, e) => clickListener(new DeviceSelectorAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            Item = itemView.FindViewById<TextView>(Resource.Id.name);
            Item.JustificationMode = Android.Text.JustificationMode.InterWord;

        }

    }
    public class DeviceSelectorAdapterClickEventArgs : EventArgs {
        public View View { get; set; }
        public int Position { get; set; }
    }
}
