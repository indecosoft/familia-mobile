using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Familia.Games.entities;

namespace Familia.Games
{
    class GamesGridViewAdapter : BaseAdapter
    {

        Context context;
        List<Game> list;

        public GamesGridViewAdapter(Context context, HashSet<Game> list)
        {
            this.context = context;
            this.list = new List<Game>();
            foreach (var item in list) {
                this.list.Add(item);
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            GamesGridViewAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as GamesGridViewAdapterViewHolder;

            if (holder == null)
            {
                holder = new GamesGridViewAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.item_game, parent, false);
                holder.text = view.FindViewById<TextView>(Resource.Id.tv_game_title);
                holder.image = view.FindViewById<ImageView>(Resource.Id.iw_icon);
                view.Tag = holder;
            }

            holder.text.Text = list[position].Name;
            int resID = context.Resources.GetIdentifier("thumbnailjoc"+list[position].Type, "drawable", context.PackageName);
            holder.image.SetImageResource(resID);

            return view;
        }

        public override int Count
        {
            get
            {
                return list.Count;
            }
        }

    }

    class GamesGridViewAdapterViewHolder : Java.Lang.Object
    {
        public ImageView image { get; set; }
        public TextView text { get; set; }
    
    }
}