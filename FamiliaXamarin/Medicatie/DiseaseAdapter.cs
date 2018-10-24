﻿using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using FamiliaXamarin.Medicatie.Entities;

namespace FamiliaXamarin.Medicatie
{
    class DiseaseAdapter : RecyclerView.Adapter
    {

        private IOnBoalaClickListener listenerBoala;
        private List<Disease> boalaList;

        public DiseaseAdapter(List<Disease> data)
        {
            boalaList= data;
            
        }
        public DiseaseAdapter()
        {
            boalaList = new List<Disease>();
        }
        public void setBoli(List<Disease> data)
        {
            boalaList.Clear();
            boalaList= data;
        }
        public void SetListenerBoala(IOnBoalaClickListener listenerBoala)
        {
            this.listenerBoala = listenerBoala;
        }
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            LayoutInflater inflater = LayoutInflater.From(parent.Context);
            View v = inflater.Inflate(Resource.Layout.item_boala, parent, false);
            return new BoalaAdapterViewHolder(v);
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as BoalaAdapterViewHolder;
            Disease boala = boalaList[position];
            holder.tvBoala.Text = boala.DiseaseName;
            holder.Boala = boala;
            holder.listenerBoala = listenerBoala;


        }

      
        public override int ItemCount => boalaList.Count;
        public void addBoala(Disease boala)
        {
            boalaList.Add(boala);
        }
        public void removeBoala(Disease boala) { boalaList.Remove(boala); }
        //        void OnClick(BoalaAdapterClickEventArgs args) => listenerBoala?.Invoke(this, args);
        //
        //       
        //        void OnLongClick(BoalaAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }

    public class BoalaAdapterViewHolder : RecyclerView.ViewHolder
    {
    //public TextView TextView { get; set; }
        public TextView tvBoala;
        public RelativeLayout rlBoliContainer;
        public Button btnDelete;
        public Disease Boala;
        public IOnBoalaClickListener listenerBoala;

    public BoalaAdapterViewHolder(View itemView) : base(itemView)
        {
            tvBoala = itemView.FindViewById<TextView>(Resource.Id.tv_boala);
            rlBoliContainer = itemView.FindViewById<RelativeLayout>(Resource.Id.rv_boala_container);
            btnDelete = itemView.FindViewById<Button>(Resource.Id.btn_delete_boala);
            rlBoliContainer.Click += delegate {
                if (listenerBoala != null)
                {
                    listenerBoala.OnBoalaClick(Boala);
                }
            };

            btnDelete.Click += delegate
            {
                if (listenerBoala != null)
                {
                    listenerBoala.OnBoalaDelete(Boala);
                }
            };
            //TextView = v;
            //            itemView.Click += (sender, e) => clickListener(new BoalaAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            //            itemView.LongClick += (sender, e) => longClickListener(new BoalaAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public interface IOnBoalaClickListener
    {
        void OnBoalaClick(Disease boala);
        void OnBoalaDelete(Disease boala);
    }

}