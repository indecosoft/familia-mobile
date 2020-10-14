using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Java.Lang;

namespace Familia.Chat
{
    class UserCardAdapter : ArrayAdapter<UserCard>
    {
    public UserCardAdapter(Context context, IList<UserCard> people) : base(context, Resource.Layout.item_tourist_spot_card, people)
    {
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
        holder.Probleme.Text = spot.Problems;

        holder.Name.Text = spot.Name;
        holder.Probleme.Text = spot.Problems ;
        //holder.Image.SetImageResource(spot.Url);

        Glide.With(Context).Load(spot.BackroundResourceId).Into(holder.Image);

        return convertView;

    }
        

    private class ViewHolder : Object
    {
        public readonly TextView Name;
        public readonly TextView Probleme;
        public readonly ImageView Image;

            public ViewHolder(View view)
        {
            Name = (TextView)view.FindViewById(Resource.Id.item_tourist_spot_card_name);
            Probleme = (TextView)view.FindViewById(Resource.Id.item_tourist_spot_card_city);
            Image = (ImageView)view.FindViewById(Resource.Id.item_tourist_spot_card_image);
        }
    }
}
}