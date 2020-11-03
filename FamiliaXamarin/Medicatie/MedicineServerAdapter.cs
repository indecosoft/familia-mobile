using System;
using System.Collections.Generic;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using Familia.Medicatie.Entities;

namespace Familia.Medicatie
{
    class MedicineServerAdapter : RecyclerView.Adapter
    {
//        public event EventHandler<MedicineServerAdapterClickEventArgs> ItemClick;
//        public event EventHandler<MedicineServerAdapterClickEventArgs> ItemLongClick;
        public IOnMedSerListener listener;

        private List<MedicationSchedule> list;


        public MedicineServerAdapter()
        {
            list = new List<MedicationSchedule>();
        }

        public MedicineServerAdapter(List<MedicationSchedule> data)
        {
            list = data;
            NotifyDataSetChanged();

        }

        public void SetListener(IOnMedSerListener listener)
        {
            this.listener = listener;
        }

        public void setMedsList(List<MedicationSchedule> data)
        {
            list.Clear();
            list = data;
            Log.Error("INAINTE 1", "" + list.Count + ", "+ data.Count);
            NotifyDataSetChanged();
            Log.Error("INAINTE 2", "" + list.Count + ", "+ data.Count);
        }

        public List<MedicationSchedule> getList()
        {
            return list;
        }

        public void removeItem(MedicationSchedule med)
        {
            if (med != null)
            {
                list.Remove(med);
                NotifyDataSetChanged();

            }
        }

        public void AddItem(MedicationSchedule med)
        {

            if (med != null)
            {
                list.Add(med);
                NotifyDataSetChanged();

            }
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
            holder.medication = medication;
            holder.listener = listener;


            string hourPrefix = Convert.ToDateTime(medication.Timestampstring).Hour < 10 ? "0" : "";
            string minutePrefix = Convert.ToDateTime(medication.Timestampstring).Minute < 10 ? "0" : "";
            holder.tvDateTime.Text =
                $"{Convert.ToDateTime(medication.Timestampstring).Day}/{Convert.ToDateTime(medication.Timestampstring).Month}/{Convert.ToDateTime(medication.Timestampstring).Year} {hourPrefix}{Convert.ToDateTime(medication.Timestampstring).Hour.ToString()}:{minutePrefix}{Convert.ToDateTime(medication.Timestampstring).Minute.ToString()}";//medication.Timestampstring.Substring(0, medication.Timestampstring.Length - 6);

            Log.Error("MEDICINE SERVER", "on bind view holder");

        }

        public override int ItemCount => list.Count;



    }

    public class MedicineServerAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView tvMedSer;
        public TextView tvContent;
        public TextView tvDateTime;
        public CardView rlContainer;
        public MedicationSchedule medication;

        public IOnMedSerListener listener;


        public MedicineServerAdapterViewHolder(View itemView) : base(itemView)
        {
            tvMedSer = itemView.FindViewById<TextView>(Resource.Id.tv_medser);
            tvContent = itemView.FindViewById<TextView>(Resource.Id.info_content);
            tvDateTime = itemView.FindViewById<TextView>(Resource.Id.info_date_time);
            rlContainer = itemView.FindViewById<CardView>(Resource.Id.card_view);
            rlContainer.Click += delegate {
                if (listener != null)
                {
                     listener.OnMedSerClick(medication);
                }
            };
        }
    }

//    public class MedicineServerAdapterClickEventArgs : EventArgs
//    {
//        public View View { get; set; }
//        public int Position { get; set; }
//    }

    public interface IOnMedSerListener
    {
          void OnMedSerClick(MedicationSchedule med);
//            void OnPosition();
    }
}