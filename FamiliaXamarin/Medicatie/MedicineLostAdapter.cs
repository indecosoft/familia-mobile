using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using FamiliaXamarin.Medicatie.Entities;
using RelativeLayout = Android.Widget.RelativeLayout;

namespace Familia.Medicatie
{
    class MedicineLostAdapter : RecyclerView.Adapter
    {

        public IOnMedLostListener listener;

        private List<MedicationSchedule> list;


        public MedicineLostAdapter()
        {
            list = new List<MedicationSchedule>();
            NotifyDataSetChanged();

        }

        public MedicineLostAdapter(List<MedicationSchedule> data)
        {
            list = data;
            NotifyDataSetChanged();

        }

        public void SetListener(IOnMedLostListener listener)
        {
            this.listener = listener;
        }

        public void setMedsList(List<MedicationSchedule> data)
        {
            //            list.Clear();
            list = data;
            NotifyDataSetChanged();
            Log.Error("ADAPTER MEDICINE LOST", "set list");

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
            View v = inflater.Inflate(Resource.Layout.item_med_lost, parent, false);
            return new MedicineLostAdapterViewHolder(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as MedicineLostAdapterViewHolder;
            MedicationSchedule medication = list[position];

            if (medication.Uuid.Contains("disease"))
            {
                holder.ivIcon.SetImageResource(Resource.Drawable.pill_lost_pers);
                holder.tvDateTime.SetTextColor(Color.ParseColor("#1AB188"));
            }
            else
            {
                holder.ivIcon.SetImageResource(Resource.Drawable.pill_lost);
                holder.tvDateTime.SetTextColor(Color.ParseColor("#f02b4c"));
            }

            holder.tvMedSer.Text = medication.Title;
            holder.tvContent.Text = medication.Content;
            holder.medication = medication;
            holder.listener = listener;


            string hourPrefix = Convert.ToDateTime(medication.Timestampstring).Hour < 10 ? "0" : "";
            string minutePrefix = Convert.ToDateTime(medication.Timestampstring).Minute < 10 ? "0" : "";
            holder.tvDateTime.Text =
                $"{Convert.ToDateTime(medication.Timestampstring).Day}/{Convert.ToDateTime(medication.Timestampstring).Month}/{Convert.ToDateTime(medication.Timestampstring).Year} {hourPrefix}{Convert.ToDateTime(medication.Timestampstring).Hour.ToString()}:{minutePrefix}{Convert.ToDateTime(medication.Timestampstring).Minute.ToString()}";//medication.Timestampstring.Substring(0, medication.Timestampstring.Length - 6);

            Log.Error("ADAPTER MEDICINE LOST", "on bind view holder");
        }

        public override int ItemCount => list.Count;
        
    }
    
    public class MedicineLostAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ImageView ivIcon;
        public TextView tvMedSer;
        public TextView tvContent;
        public TextView tvDateTime;
        public CardView rlContainer;
        public MedicationSchedule medication;

        public IOnMedLostListener listener;

        public MedicineLostAdapterViewHolder(View itemView) : base(itemView)
        {
            ivIcon = itemView.FindViewById<ImageView>(Resource.Id.icon);
            tvMedSer = itemView.FindViewById<TextView>(Resource.Id.tv_medser);
            tvContent = itemView.FindViewById<TextView>(Resource.Id.info_content);
            tvDateTime = itemView.FindViewById<TextView>(Resource.Id.info_date_time);
            rlContainer = itemView.FindViewById<CardView>(Resource.Id.card_view);
            rlContainer.Click += delegate {
                if (listener != null)
                {
                    listener.OnMedLostClick(medication);
                }
            };
        }
    }

    public interface IOnMedLostListener
    {
        void OnMedLostClick(MedicationSchedule med);
    }
}