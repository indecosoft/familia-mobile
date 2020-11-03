using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Familia.Profile.Data;

namespace Familia.Profile
{
    class DiseasesAdapter : RecyclerView.Adapter
    {

        Context context;
        private List<PersonalDisease> list;
        public DiseasesAdapter(Context context, List<PersonalDisease> list)
        {
            this.context = context;
            this.list = list;
        }

        public DiseasesAdapter(Context context)
        {
            this.context = context;
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            LayoutInflater inflater = LayoutInflater.From(parent.Context);
            View v = inflater.Inflate(Resource.Layout.item_profile_disease, parent, false);

            return new DiseasesAdapterViewHolder(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as DiseasesAdapterViewHolder;
            PersonalDisease disease = list[position];
            holder.tvDisease.Text = disease.Name;

        }

        public override int ItemCount => list.Count;
    }

    class DiseasesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView tvDisease;

        public DiseasesAdapterViewHolder(View itemView) : base(itemView)
        {
            tvDisease = itemView.FindViewById<TextView>(Resource.Id.tv_title);
        }
    }
}