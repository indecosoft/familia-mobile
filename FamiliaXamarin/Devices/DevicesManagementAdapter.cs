using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Familia;

namespace FamiliaXamarin.Devices
{
    class DevicesManagementAdapter : RecyclerView.Adapter
    {
        public event EventHandler<DevicesManagementAdapterClickEventArgs> ItemClick;
        public event EventHandler<DevicesManagementAdapterClickEventArgs> ItemLongClick;
        private readonly List<DevicesManagementModel> _devices;

        public DevicesManagementAdapter(List<DevicesManagementModel> data)
        {
            _devices = data;
        }
        public override int GetItemViewType(int position)
        {
            return _devices[position].ItemType;
        }

        public DevicesManagementModel GetItemModel(int position)
        {
            return _devices[position].ItemType == 1 ?_devices[position] : null;
        }
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            //View itemView = null;
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

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

            var vh = new DevicesManagementAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            //var item = _devices[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as DevicesManagementAdapterViewHolder;
            holder.TextViewName.Text = _devices[position].Device.Name;
            switch (_devices[position].ItemType)
            {
                case 0:
                    holder.TextViewName.Text =
                        _devices[position].ItemValue ==
                        Application.Context.GetString(Resource.String.blood_pressure_device)
                            ?
                            "Tensiometre"
                            : _devices[position].ItemValue ==
                              Application.Context.GetString(Resource.String.blood_glucose_device)
                                ? "Glucometre"
                                : "Bratari Smart";
                    break;
                case 1:
                    holder.TextViewName.Text = _devices[position].Device.Name;
                    break;
            }
        }

        public void AddMessage(DevicesManagementModel model)
        {
            _devices.Add(model);
        }
        public override int ItemCount => _devices.Count;

        void OnClick(DevicesManagementAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(DevicesManagementAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public void Clear()
        {
            _devices.Clear();
        }
    }

    public class DevicesManagementAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView TextViewName { get; set; }


        public DevicesManagementAdapterViewHolder(View itemView, Action<DevicesManagementAdapterClickEventArgs> clickListener,
                            Action<DevicesManagementAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            TextViewName = itemView.FindViewById<TextView>(Resource.Id.name);
            itemView.Click += (sender, e) => clickListener(new DevicesManagementAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new DevicesManagementAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class DevicesManagementAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}