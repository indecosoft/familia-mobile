using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using Android.Content;
>>>>>>> master
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
<<<<<<< HEAD
        private List<ConverstionsModel> mContacts;

        public ConvAdapter(List<ConverstionsModel> data)
        {
            mContacts = data;
=======
        List<ConverstionsModel> items;

        public ConvAdapter(List<ConverstionsModel> data)
        {
            items = data;
>>>>>>> master
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

<<<<<<< HEAD
            //Setup your layout here
            View itemView = null;
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new ConvAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
=======
            Context context = parent.Context;
            LayoutInflater inflater = LayoutInflater.From(context);

            // Inflate the custom layout
            View contactView = inflater.Inflate(Resource.Layout.item_conversations, parent, false);

            // Return a new holder instance
            ConvAdapterViewHolder viewHolder = new ConvAdapterViewHolder(contactView, OnClick, OnLongClick);
            return viewHolder;
>>>>>>> master
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
<<<<<<< HEAD
            var contact = mContacts[position].Conversations;         
            var holder = viewHolder as ConvAdapterViewHolder;
            holder.nameTextView.Text = contact[0];
            // Replace the contents of the view with that element
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => mContacts.Count;
=======
            var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as ConvAdapterViewHolder;

            ConverstionsModel contact = items[position];

            // Set item views based on your views and data model

            holder.nameTextView.Text = contact.Username;
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => items.Count;
>>>>>>> master

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