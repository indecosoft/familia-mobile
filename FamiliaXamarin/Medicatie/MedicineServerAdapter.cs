using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using FamiliaXamarin.Medicatie.Entities;

namespace Familia.Medicatie
{
    class MedicineServerAdapter : RecyclerView.Adapter
    {
        public event EventHandler<MedicineServerAdapterClickEventArgs> ItemClick;
        public event EventHandler<MedicineServerAdapterClickEventArgs> ItemLongClick;
//        string[] items;

        private List<MedicationSchedule> list;


        public MedicineServerAdapter()
        {
            list = new List<MedicationSchedule>();
        }

        public MedicineServerAdapter(List<MedicationSchedule> data)
        {
            list = data;
        }

        public void setMedsList(List<MedicationSchedule> data)
        {
            list.Clear();
            list = data;
        }

        public List<MedicationSchedule> getList()
        {
            return list;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            LayoutInflater inflater = LayoutInflater.From(parent.Context);
            View v = inflater.Inflate(Resource.Layout.item_medser, parent, false);
            return new MedicineServerAdapterViewHolder(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as MedicineServerAdapterViewHolder;
            MedicationSchedule medication = list[position];
            holder.tvMedSer.Text = medication.Title;
            holder.tvContent.Text = medication.Content;
            holder.tvDateTime.Text = medication.Timestampstring.Substring(0, medication.Timestampstring.Length - 6);
//            var date = medication.Timestampstring.Substring(0, 10);
//            var time = medication.Timestampstring.Substring(11, 5);
//            holder.tvDate.Text = date;
//            holder.tvTime.Text = time;

        }

        public override int ItemCount => list.Count;

        void OnClick(MedicineServerAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(MedicineServerAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class MedicineServerAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView tvMedSer;
        public TextView tvContent;
        public TextView tvDate;
        public TextView tvTime;

        public TextView tvDateTime;


        public MedicineServerAdapterViewHolder(View itemView) : base(itemView)
        {
            tvMedSer = itemView.FindViewById<TextView>(Resource.Id.tv_medser);
            tvContent = itemView.FindViewById<TextView>(Resource.Id.info_content);
            tvDateTime = itemView.FindViewById<TextView>(Resource.Id.info_date_time);
//            tvDate = itemView.FindViewById<TextView>(Resource.Id.info_date);
//            tvTime = itemView.FindViewById<TextView>(Resource.Id.info_hour);
//            
        }
    }

    public class MedicineServerAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}