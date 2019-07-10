﻿using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using FamiliaXamarin.Medicatie.Entities;
using RelativeLayout = Android.Widget.RelativeLayout;

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
        }

        public void SetListener(IOnMedSerListener listener)
        {
            this.listener = listener;
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
            holder.medication = medication;
            holder.listener = listener;


            string hourPrefix = Convert.ToDateTime(medication.Timestampstring).Hour < 10 ? "0" : "";
            string minutePrefix = Convert.ToDateTime(medication.Timestampstring).Minute < 10 ? "0" : "";
            holder.tvDateTime.Text =
                $"{Convert.ToDateTime(medication.Timestampstring).Day}/{Convert.ToDateTime(medication.Timestampstring).Month}/{Convert.ToDateTime(medication.Timestampstring).Year} {hourPrefix}{Convert.ToDateTime(medication.Timestampstring).Hour.ToString()}:{minutePrefix}{Convert.ToDateTime(medication.Timestampstring).Minute.ToString()}";//medication.Timestampstring.Substring(0, medication.Timestampstring.Length - 6);

            Log.Error("Date time in Adapter:", holder.tvDateTime.Text);
            Log.Error("Date time inainte:", medication.Timestampstring);

        }

        public override int ItemCount => list.Count;

//        void OnClick(MedicineServerAdapterClickEventArgs args)
//        {
//            ItemClick?.Invoke(this, args);
//            Log.Error("ARGS ",args.ToString());
//        }
//
//        
//        void OnLongClick(MedicineServerAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

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

    public class MedicineServerAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public interface IOnMedSerListener
    {
        void OnMedSerClick(MedicationSchedule med);
//        void OnBoalaDelete(Disease boala);
    }
}