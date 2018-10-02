﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Java.Lang;
using Object = Java.Lang.Object;

namespace FamiliaXamarin
{
    class UserCardAdapter : ArrayAdapter<UserCard>
    {
    public UserCardAdapter(Context context) : base(context, 0)
    {
        //super(context, 0);
    }

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        ViewHolder holder;

        if (convertView == null)
        {
            LayoutInflater inflater = LayoutInflater.From(Context);
            convertView = inflater.Inflate(Resource.Layout.item_tourist_spot_card, parent, false);
            holder = new ViewHolder(convertView);
            convertView.Tag = holder;
        }
        else
        {
            holder = (ViewHolder)convertView.Tag;
        }

        UserCard spot = GetItem(position);

        holder.Name.Text = spot.Name;
        holder.Probleme.Text = spot.Probleme;

        holder.Name.Text = spot.Name;
        holder.Probleme.Text = spot.Probleme ;
        holder.Image.SetImageResource(spot.Url);

        Glide.With(Context).Load(spot.Url).Into(holder.Image);

        return convertView;

    }

    private class ViewHolder : Object
    {
        public TextView Name;
        public TextView Probleme;
        public ImageView Image;
        private View Curentview;

            public ViewHolder(View view)
        {
            Name = (TextView)view.FindViewById(Resource.Id.item_tourist_spot_card_name);
            Probleme = (TextView)view.FindViewById(Resource.Id.item_tourist_spot_card_city);
            Image = (ImageView)view.FindViewById(Resource.Id.item_tourist_spot_card_image);
        }
    }
}
}