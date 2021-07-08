using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace Familia.OngBenefits.ShowBenefits
{
    class BenefitsAdapter : RecyclerView.Adapter
    {
        private List<Benefit> list;

        public BenefitsAdapter(List<Benefit> list)
        {
            this.list = list;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater inflater = LayoutInflater.From(parent.Context);
            View v = inflater.Inflate(Resource.Layout.item_benefit_detailed, parent, false);
            return new BenefitViewHolder(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as BenefitViewHolder;
            Benefit item = list[position];

            holder.tvName.Text = item.Name;
            holder.tvDetails.Text = item.Details;
            holder.tvObservations.Text = item.Observations;
            holder.tvDateAndTime.Text = ParseDateAndTime(item.DateTimeReceived);
        }

        public override int ItemCount => list.Count;

        private string ParseDateAndTime(string dateAndTime) =>
            DateTimeOffset.Parse(dateAndTime).UtcDateTime.AddHours(2).ToString("dd/MM/yyyy HH:mm");


        public class BenefitViewHolder : RecyclerView.ViewHolder
        {
            public TextView tvName;
            public TextView tvDetails;
            public TextView tvObservations;
            public TextView tvDateAndTime;

            public BenefitViewHolder(View itemView) : base(itemView)
            {
                tvName = itemView.FindViewById<TextView>(Resource.Id.tvName);
                tvDetails = itemView.FindViewById<TextView>(Resource.Id.tvDetails);
                tvObservations = itemView.FindViewById<TextView>(Resource.Id.tvObservations);
                tvDateAndTime = itemView.FindViewById<TextView>(Resource.Id.tvDateAndTime);
            }
        }
    }
}