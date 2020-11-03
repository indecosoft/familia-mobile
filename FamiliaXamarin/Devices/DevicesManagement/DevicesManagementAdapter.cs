using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Familia.Devices.DevicesManagement.Dialogs.Models;
using Familia.Devices.Helpers;

namespace Familia.Devices.DevicesManagement {
	class DevicesManagementAdapter : RecyclerView.Adapter {
		public event EventHandler<DevicesManagementAdapterClickEventArgs> ItemClick;
		public event EventHandler<DevicesManagementAdapterClickEventArgs> ItemLongClick;
		private readonly List<DeviceEditingManagementModel> _devices;

		public DevicesManagementAdapter(List<DeviceEditingManagementModel> data) {
			_devices = data;
		}

		public override int GetItemViewType(int position) {
			return _devices[position].ItemType;
		}

		public DeviceEditingManagementModel? GetItemModel(int position) {
			return _devices[position].ItemType == 1 ? _devices?[position] : null;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
			Context context = parent.Context;
			LayoutInflater inflater = LayoutInflater.From(context);

			int layout = viewType switch {
				0 => Resource.Layout.item_device_type,
				1 => Resource.Layout.rowlayout,
				_ => Resource.Layout.rowlayout
			};

			View itemView = inflater.Inflate(layout, parent, false);

			var vh = new DevicesManagementAdapterViewHolder(itemView, OnClick, OnLongClick);
			return vh;
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
			if (!(holder is DevicesManagementAdapterViewHolder viewHolder)) return;
			viewHolder.TextViewName.Text = _devices[position].ItemType switch {
				0 => (_devices[position].Device.DeviceType == DeviceType.BloodPressure ? "Tensiometre" :
					_devices[position].Device.DeviceType == DeviceType.Glucose ? "Glucometre" : "Bratari Smart"),
				1 => _devices[position].Device.Name,
				_ => _devices[position].Device.Name
			};
		}

		public void AddDevice(DeviceEditingManagementModel model) {
			_devices.Add(model);
		}

		public override int ItemCount => _devices.Count;

		void OnClick(DevicesManagementAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
		void OnLongClick(DevicesManagementAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

		public void Clear() {
			_devices.Clear();
		}
	}

	public class DevicesManagementAdapterViewHolder : RecyclerView.ViewHolder {
		public TextView TextViewName { get; }


		public DevicesManagementAdapterViewHolder(View itemView,
			Action<DevicesManagementAdapterClickEventArgs> clickListener,
			Action<DevicesManagementAdapterClickEventArgs> longClickListener) : base(itemView) {
			//TextView = v;
			TextViewName = itemView.FindViewById<TextView>(Resource.Id.name);
			itemView.Click += (sender, e) =>
				clickListener(new DevicesManagementAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
			itemView.LongClick += (sender, e) =>
				longClickListener(
					new DevicesManagementAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
		}
	}

	public class DevicesManagementAdapterClickEventArgs : EventArgs {
		public View View { get; set; }
		public int Position { get; set; }
	}
}