using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Familia.Devices.Models;

namespace Familia.Devices.DevicesManagement.DeviceManufactureSelector {
	public class DeviceManufactureSelectorRecyclerViewAdapter : RecyclerView.Adapter {
		public event EventHandler<DeviceSelectorAdapterClickEventArgs> ItemClick;
		private List<SupportedDeviceModel> _items;
		private readonly List<SupportedDeviceModel> _itemsCopy;
		private readonly LayoutInflater _mInflater;

		public DeviceManufactureSelectorRecyclerViewAdapter(Context context, List<SupportedDeviceModel> items) {
			_mInflater = LayoutInflater.From(context);
			_items = items;
			_itemsCopy = items;
		}

		public override int ItemCount => _items.Count;

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
			SupportedDeviceModel item = _items[position];
			if (!(holder is DeviceSelectorHolder viewHolder)) return;
			viewHolder.Item.Text = item.DeviceName;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
			const int layout = Resource.Layout.item_device_selector;

			View itemView = _mInflater.Inflate(layout, parent, false);

			var viewHolder = new DeviceSelectorHolder(itemView, OnClick);
			return viewHolder;
		}

		private void OnClick(DeviceSelectorAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);

		public SupportedDeviceModel GetItem(int position) => _items[position];

		public void Search(string textToSearch) {
			_items = _itemsCopy.Where(c =>
				c.DeviceName.ToLower().StartsWith(textToSearch.ToLower(), StringComparison.Ordinal)).ToList();
			NotifyDataSetChanged();
		}
	}

	internal class DeviceSelectorHolder : RecyclerView.ViewHolder {
		public TextView Item { get; }

		public DeviceSelectorHolder(View itemView, Action<DeviceSelectorAdapterClickEventArgs> clickListener) :
			base(itemView) {
			itemView.Click += (sender, e) =>
				clickListener(new DeviceSelectorAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
			Item = itemView.FindViewById<TextView>(Resource.Id.name);
			Item.JustificationMode = JustificationMode.InterWord;
		}
	}

	public class DeviceSelectorAdapterClickEventArgs : EventArgs {
		public View View { get; set; }
		public int Position { get; set; }
	}
}