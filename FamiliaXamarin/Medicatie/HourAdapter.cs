using System;
using System.Collections.Generic;
using Familia;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie
{
    class HourAdapter : RecyclerView.Adapter
    {
        private List<Hour> hours;

        private OnHourClickListener listener;

        public HourAdapter()
        {
            hours = new List<Hour>();
        }

        public interface OnHourClickListener
        {
            void onHourClicked(Hour hour);
        }

        public HourAdapter(List<Hour> data)
        {
            hours = data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater inflater = LayoutInflater.From(parent.Context);
            View v = inflater.Inflate(Resource.Layout.item_hour, parent, false);
            return new HourAdapterViewHolder(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as HourAdapterViewHolder;
            Hour hour = hours[position];
            if (holder != null)
            {
                holder.tvHour.Text = hour.HourName;
                holder.Hour = hour;
                holder.Listener = listener;

            }
        }

        public override int ItemCount => hours.Count;

        public void SetList(List<Hour> list)
        {
            this.hours = list;
        }

        public void SetListener(OnHourClickListener listener)
        {
            this.listener = listener;
        }


        public Hour GetHourById(string idHour)
        {
            foreach (Hour hour in hours)
            {
                if (hour.Id.Equals(idHour))
                {
                    return hour;
                }
            }

            return null;
        }
        public void AddHour(Hour hour)
        {
            hours.Add(hour);
            NotifyDataSetChanged();
        }
        public void ClearList()
        {
            hours.Clear();
        }
        public void updateHour(Hour myHour)
        {
            Hour mHour = GetHourById(myHour.Id);

        }
        public List<Hour> GetList()
        {
            return hours;
        }

        public class HourAdapterViewHolder : RecyclerView.ViewHolder
        {
            public RelativeLayout rlHour;
            public TextView tvHour;
            public OnHourClickListener Listener;
            public Hour Hour;

            public HourAdapterViewHolder(View itemView) : base(itemView)
            {
                rlHour = itemView.FindViewById<RelativeLayout>(Resource.Id.rl_hours);
                tvHour = itemView.FindViewById<TextView>(Resource.Id.tv_hour);
                rlHour.Click += delegate
                {
                    if (Listener != null)
                    {
                        Listener.onHourClicked(Hour);
                    }
                };
            }
        }

        public class HourAdapterClickEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
        }
    }
}