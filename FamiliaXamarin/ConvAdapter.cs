using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using FamiliaXamarin.JsonModels;

namespace FamiliaXamarin
{
    class ConvAdapter : RecyclerView.Adapter
    {
        public event EventHandler<ConvAdapterClickEventArgs> ItemClick;
        public event EventHandler<ConvAdapterClickEventArgs> ItemLongClick;
        private List<ConverstionsModel> mContacts;

        public ConvAdapter(List<ConverstionsModel> data)
        {
            mContacts = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new ConvAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var contact = mContacts[position].Conversations;         
            var holder = viewHolder as ConvAdapterViewHolder;
            holder.nameTextView.Text = contact[0];
            // Replace the contents of the view with that element
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => mContacts.Count;

        void OnClick(ConvAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ConvAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class ConvAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public TextView nameTextView;

        public ConvAdapterViewHolder(View itemView, Action<ConvAdapterClickEventArgs> clickListener,
                            Action<ConvAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            nameTextView = itemView.FindViewById<TextView>(Resource.Id.contact_name);
            itemView.Click += (sender, e) => clickListener(new ConvAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new ConvAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class ConvAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}