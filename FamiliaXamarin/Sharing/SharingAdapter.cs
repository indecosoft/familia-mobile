using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace FamiliaXamarin.Sharing
{
    class SharingAdapter : RecyclerView.Adapter
    {
        public event EventHandler<SharingAdapterClickEventArgs> ItemClick;
        public event EventHandler<SharingAdapterClickEventArgs> ItemLongClick;
        string[] items;

        public SharingAdapter(string[] data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new SharingAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as SharingAdapterViewHolder;
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => items.Length;

        void OnClick(SharingAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(SharingAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class SharingAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }


        public SharingAdapterViewHolder(View itemView, Action<SharingAdapterClickEventArgs> clickListener,
                            Action<SharingAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            itemView.Click += (sender, e) => clickListener(new SharingAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new SharingAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class SharingAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}